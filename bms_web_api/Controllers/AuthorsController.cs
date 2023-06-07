using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Runtime.InteropServices;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public AuthorsController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<AuthorModel>>> GetAllAuthors(string? search, string? sort, int page = 0)
        {
            try
            {
                // AsQueryable: kiểm tra điều kiện search, sort
                var author = _context.Authors.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    // Khác rỗng hoặc Null
                    author = author.Where(p => p.author_name.Contains(search));
                    return Ok(author);
                }
                #endregion

                #region Sort
                author = author.OrderBy(p => p.author_name);
                if (!string.IsNullOrEmpty(sort))
                {
                    // Khác rỗng hoặc Null
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "authorname_desc":
                            author = author.OrderByDescending(p => p.author_name);
                            break;
                        // Tên tăng dần
                        case "authorname_asc":
                            author = author.OrderBy(p => p.author_name);
                            break;
                    }
                }
                #endregion

                
                // Ngược lại
                // await truy vấn bất đồng bộ
                var result = await author.Select(p => new AuthorModel
                {
                    author_id = p.Id,
                    author_name = p.author_name,
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
       
        public async Task<IActionResult> GetAuthorWithBooks(int id)
        {
            try
            {
                var getAuthorWithBook = await _context.Authors.Where(p => p.Id == id).Select(author => new AuthorWithBooksModel()
                {
                    // Lấy ra được tên tác giả
                    // và danh sách book tương ứng với author trả về một đối tượng với kiểu Book_Author
                    author_id = author.Id,
                    author_name = author.author_name,
                    AuthorAndBooks = author.BookDatas.Select(p => new Author_Book()
                    {
                        // Lấy được tên sách 
                        Title_book = p.book_title
                    }).ToHashSet(),

                }).FirstOrDefaultAsync();
                return Ok(getAuthorWithBook);
            }
            catch
            {
                return NotFound();
            }
        }
        // Thêm mới
        [HttpPost]
       
        public async Task<IActionResult> CreateNewAu(AuthorNoIdModel author)
        {
            try
            {
                // Kiểm tra xem đã tồn tại tên tác giả trên hệ thống chưa
                var existAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.author_name == author.author_name);

                if (existAuthor == null)
                {
                    // Nếu tác giả chưa tồn tại thì thêm mới vào CSDL
                    var newAuthor = new AuthorData
                    {
                        author_name = author.author_name
                    };
                    _context.Add(newAuthor);
                    await _context.SaveChangesAsync();
                    return StatusCode(statusCode: StatusCodes.Status201Created, newAuthor);
                }
                else
                {
                    // Nếu tác giả đã tồn tại thì trả về thông báo lỗi hoặc thực hiện hành động phù hợp
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Tên tác giả đã tồn tại !");
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        // Cập nhật dữ liệu theo ID
        [HttpPut("{id}")]
       
        public async Task<IActionResult> Updateid(int id, AuthorNoIdModel model)
        {
            var author = await _context.Authors.SingleOrDefaultAsync(auId => auId.Id == id);
            if (author != null)
            {
                author.author_name = model.author_name;
                await _context.SaveChangesAsync();
                return Ok(author);
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
                   // Include() để lấy cả thông tin các cuốn sách của tác giả đó, sau đó duyệt qua danh sách các cuốn sách và xóa chúng trước rồi mới đến tác giả
                    var author = await _context.Authors.Include(a => a.BookDatas).SingleOrDefaultAsync(a => a.Id == id);
                    if (author != null)
                    {
                        // Kiểm tra liên kết giữa tác giả và các quyển sách trong đơn hàng
                        var isLinkedWithOrder = await _context.OrderItems.AnyAsync(o => o.Book.author_id == id);
                        if (isLinkedWithOrder)
                        {
                            return BadRequest("Tác giả này đang liên kết với một hoặc nhiều quyển sách có trong đơn hàng. Không thể xóa!");
                        }
                        // Ngược lại
                        foreach (var bookData in author.BookDatas.ToHashSet())
                        {
                            _context.Books.Remove(bookData);
                        }

                        _context.Authors.Remove(author);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return Ok("Xóa tác giả thành công !");
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
