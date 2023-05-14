using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public PublishersController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<PublisherIdModel>>> GetAllPublishers(string? search, string? sort, int page = 0)
        {
            try
            {
                var publisher = _context.Publishers.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    publisher = publisher.Where(p => p.publisher_name.Contains(search));
                    return Ok(publisher);
                }
                #endregion

                #region Sort
                publisher = publisher.OrderBy(p => p.publisher_name);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "publishername_desc":
                            publisher = publisher.OrderByDescending(p => p.publisher_name);
                            break;
                        // Tên tăng dần
                        case "publishername_asc":
                            publisher = publisher.OrderBy(p => p.publisher_name);
                            break;
                    }
                }

                #endregion

              
                var result = await publisher.Select(p => new PublisherIdModel
                {
                    publisher_id = p.Id,
                    publisher_name = p.publisher_name,
                    publisher_address = p.publisher_address,
                    publisher_phone = p.publisher_phone,
                }).ToListAsync();
           
                #region Pagination
                int totalRecords = result.Count();

                int totalPages = (int)Math.Ceiling((double)totalRecords / PAGE_SIZE);
                if (page == 0)
                {
                    return Ok(new
                    {
                        totalPages = 0,
                        currentPage = 0,
                        result
                    });
                }
                var pagedResult = result.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE);

                #endregion

                return Ok(new
                {
                    totalPages,
                    currentPage = page,
                    result = pagedResult
                });
            }
            catch
            {
                return NotFound();
            }
        }

        // Lấy dữ liệu theo ID
        [HttpGet("{id}")]
       
        public async Task<IActionResult> GetPublisherBook(int id)
        {
            try
            {
                // LINQ
                // Với điều kiện Id nhập vào bằng với Id publisher thì
                // sẽ trả về một đối tượng publisher với kiểu PublisherWithBook_Author
                var getPublisher = await _context.Publishers.Where(p => p.Id == id).Select(publisher => new PublisherWithBook()
                {
                    publisher_id = publisher.Id,
                    // Lấy ra được tên publisher
                    // và danh sách book tương ứng với author trả về một đối tượng với kiểu Book_Author
                    publisher_name = publisher.publisher_name,
                    publisher_address= publisher.publisher_address,
                    publisher_phone= publisher.publisher_phone,
                    PublisherBook = publisher.BookDatas.Select(p => new Publisher_Book()
                    {
                        // Lấy được tên sách 
                        Title_book = p.book_title,
                    }).ToHashSet(),

                }).FirstOrDefaultAsync();
                return Ok(getPublisher);
            }
            catch
            {
                return NotFound();
            }
        }
        // Thêm mới
        [HttpPost]
       
        public async Task<IActionResult> CreateNewPublisher(PublisherModel publisher)
        {
            try
            {

                // Kiểm tra xem đã tồn tại tên nhà xuất bản trên hệ thống chưa
                var existPublisher = await _context.Publishers.FirstOrDefaultAsync(p => p.publisher_name == publisher.publisher_name);

                if (existPublisher == null)
                {

                    var newPublisher = new PublisherData
                    {
                        publisher_name = publisher.publisher_name,
                        publisher_address = publisher.publisher_address,
                        publisher_phone = publisher.publisher_phone
                    };
                    _context.Add(newPublisher);
                    await _context.SaveChangesAsync();
                    return StatusCode(statusCode: StatusCodes.Status201Created, newPublisher);
                }
                else
                {
                    // Nếu tên NXB đã tồn tại thì trả về thông báo lỗi hoặc thực hiện hành động phù hợp
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Tên nhà xuất bản đã tồn tại !");
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        // Cập nhật dữ liệu theo ID
        [HttpPut("{id}")]
       
        public async Task<IActionResult> Updateid(int id, PublisherModel model)
        {
            var publisher = await _context.Publishers.SingleOrDefaultAsync(pId => pId.Id == id);
            if (publisher != null)
            {
                publisher.publisher_name = model.publisher_name;
                publisher.publisher_address = model.publisher_address;
                publisher.publisher_phone = model.publisher_phone;
                _context.SaveChanges();
                return Ok("Cập nhật thành công !");
            }
            else
            {
                return NotFound();
            }
        }
        // Xóa dữ liệu theo ID
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> Deleteid(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Include() để lấy cả thông tin các cuốn sách của NXB đó, sau đó duyệt qua danh sách các cuốn sách và xóa chúng trước rồi mới đến NXB
                    var publish = await _context.Publishers.Include(a => a.BookDatas).SingleOrDefaultAsync(a => a.Id == id);
                    if (publish != null)
                    {
                        // Kiểm tra liên kết giữa NXB và các quyển sách trong đơn hàng
                        var isLinkedWithOrder = await _context.OrderItems.AnyAsync(o => o.Book.category_id == id);
                        if (isLinkedWithOrder)
                        {
                            return BadRequest("Nhà xuất bản này đang liên kết với một hoặc nhiều quyển sách có trong đơn hàng. Không thể xóa!");
                        }
                        // Ngược lại
                        foreach (var bookData in publish.BookDatas.ToHashSet())
                        {
                            _context.Books.Remove(bookData);
                        }

                        _context.Publishers.Remove(publish);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return Ok("Xóa NXB thành công !");
                    }
                    else
                    {
                        return NotFound();

                    }
                }
                catch
                {
                    return BadRequest();
                }
            }
        }
    }
}
