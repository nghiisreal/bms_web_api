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
    public class CategoriesController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public CategoriesController(MyDBContext context)
        {
            _context = context;
        }
        [HttpGet]
       
        public async Task<ActionResult<HashSet<CategoryIdModel>>> GetAllCate(string? search, string? sort, int page = 0) {
            try
            {
                var category = _context.Categories.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    category = category.Where(p => p.category_name.Contains(search));
                    return Ok(category);
                }
                #endregion

                #region Sort
                category = category.OrderBy(p => p.category_name);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "categoryname_desc":
                            category = category.OrderByDescending(p => p.category_name);
                            break;
                        // Tên tăng dần
                        case "categoryname_asc":
                            category = category.OrderBy(p => p.category_name);
                            break;
                    }
                }
                #endregion

              
               
                var result = await category.Select(p => new CategoryIdModel
                {
                    category_id = p.Id,
                    category_name = p.category_name,
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
        
        [HttpGet("{id}")]
       
        public async Task<IActionResult> GetCategoriesWithBook(int id) 
        {
            try
            {
                var getCategory = await _context.Categories.Where(p => p.Id == id).Select(cate => new CategoryWithBooksModel()
                {
                    category_id = cate.Id,
                    category_name = cate.category_name,
                    CategoryAndBooks = cate.BookDatas.Select(p => new Category_BooksModel()
                    {
                        TitleBooks = p.book_title
                    }).ToHashSet(),
                }).FirstOrDefaultAsync();
                return Ok(getCategory);
            }
            catch
            {
                return NotFound();
            }
        }
      
        [HttpPost]
       
        public async Task<IActionResult> CreateNewCate(CategoryModel cate)
        {
            try
            {
                // Kiểm tra xem đã tồn tại tên loại trên hệ thống chưa
                var existCate = await _context.Categories.FirstOrDefaultAsync(c => c.category_name == cate.category_name);

                if (existCate == null)
                {
                    // Nếu loại chưa tồn tại thì thêm mới vào CSDL
                    var newCate = new CategoryData
                    {
                        category_name = cate.category_name
                    };
                    _context.Add(newCate);
                    await _context.SaveChangesAsync();
                    return StatusCode(statusCode: StatusCodes.Status201Created, newCate);
                }
                else
                {
                    // Nếu loại đã tồn tại thì trả về thông báo lỗi hoặc thực hiện hành động phù hợp
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Tên loại đã tồn tại !");
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPut("{id}")]
       
        public async Task<IActionResult> Updateid(int id, CategoryModel model)
        {
            var cate = await _context.Categories.SingleOrDefaultAsync(cId => cId.Id == id);
            if (cate != null)
            {
                cate.category_name = model.category_name;
                await _context.SaveChangesAsync();
                return Ok("Cập nhật thành công !");
            }
            else
            {
                return NotFound();
            }
        }
     
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> Deleteid(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Include() để lấy cả thông tin các cuốn sách của loại đó, sau đó duyệt qua danh sách các cuốn sách và xóa chúng trước rồi mới đến loại
                    var cate = await _context.Categories.Include(a => a.BookDatas).SingleOrDefaultAsync(a => a.Id == id);
                    if (cate != null)
                    {
                        // Kiểm tra liên kết giữa loại sách và các quyển sách trong đơn hàng
                        var isLinkedWithOrder = await _context.OrderItems.AnyAsync(o => o.Book.category_id == id);
                        if (isLinkedWithOrder)
                        {
                            return BadRequest("Loại sách này đang liên kết với một hoặc nhiều quyển sách có trong đơn hàng. Không thể xóa!");
                        }
                        // Ngược lại
                        foreach (var bookData in cate.BookDatas.ToHashSet())
                        {
                            _context.Books.Remove(bookData);
                        }

                        _context.Categories.Remove(cate);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return Ok("Xóa loại sách thành công !");
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
    };
}
