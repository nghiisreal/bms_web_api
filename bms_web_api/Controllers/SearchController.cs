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
    public class SearchController : ControllerBase
    {
        private readonly MyDBContext _context;
        public SearchController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<IActionResult> SearchBook(string? search)
        {
            try
            {
                var book = _context.Books.AsQueryable();
                var author = _context.Authors.AsQueryable();
                var publisher = _context.Publishers.AsQueryable();
                var category = _context.Categories.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    book = book.Where(p => p.book_title.Contains(search));
                    return Ok(book);
                }
                #endregion

                // Ngược lại
                // Truy vấn nhiều bảng
                var result = await (from a in _context.Authors
                                    join b in _context.Books on a.Id equals b.author_id into ab
                                    from aubook in ab.DefaultIfEmpty()
                                    join c in category on aubook.category_id equals c.Id into bc
                                    from bookcate in bc.DefaultIfEmpty()
                                    join p in publisher on aubook.publisher_id equals p.Id into bp
                                    from bookpub in bp.DefaultIfEmpty()
                                    where aubook != null && bookcate != null && bookpub != null
                                    select new
                                    {
                                        author_id = a.Id,
                                        author_name = a.author_name,
                                        book_id = aubook.Id,
                                        ISBN = aubook.ISBN,
                                        book_title = aubook.book_title,
                                        book_price = aubook.book_price,
                                        book_image = aubook.book_image,
                                        user_book  =aubook.user_book,
                                        category_name = bookcate.category_name,
                                        publisher_name = bookpub.publisher_name,
                                        public_date = aubook.public_date
                                    }).ToListAsync();
                return Ok(result);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
       
        public async Task<IActionResult> SearchAuthor(string? search)
        {
            try
            {
                var book = _context.Books.AsQueryable();
                var author = _context.Authors.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    author = author.Where(p => p.author_name.Contains(search));
                    return Ok(author);
                }
                #endregion
                var result = await (from a in _context.Authors
                                    join b in _context.Books on a.Id equals b.author_id into ba
                                    from b in ba.DefaultIfEmpty() // Left join
                                    select new
                                    {
                                        author_id = a.Id,
                                        author_name = a.author_name,
                                        book_title = b == null ? "" : b.book_title
                                    })
               .GroupBy(x => new { x.author_id, x.author_name })
               .Select(x => new
               {
                   author_id = x.Key.author_id,
                   author_name = x.Key.author_name,
                   bookList = x.Where(y => !string.IsNullOrEmpty(y.book_title)).Select(y => new Author_Book()
                   {
                       Title_book = y.book_title
                   }).ToHashSet()
               })
               .ToListAsync();
                return Ok(result);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
       
        public async Task<IActionResult> SearchPublisher(string? search)
        {
            try
            {
                var book = _context.Books.AsQueryable();
                var publisher = _context.Publishers.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    publisher = publisher.Where(p => p.publisher_name.Contains(search));
                    return Ok(publisher);
                }
                #endregion

                // Ngược lại
                // Truy vấn nhiều bảng
                var result = await (from p in _context.Publishers
                                    join b in _context.Books on p.Id equals b.publisher_id into bp
                                    from b in bp.DefaultIfEmpty() // Left join
                                    select new
                                    {
                                        publisher_id = p.Id,
                                        publisher_name = p.publisher_name,
                                        publisher_address = p.publisher_address,
                                        publisher_phone = p.publisher_phone,
                                        book_title = b == null ? "" : b.book_title
                                    })
               .GroupBy(x => new { x.publisher_id, x.publisher_name, x.publisher_address, x.publisher_phone })
               .Select(x => new
               {
                   publisher_id = x.Key.publisher_id,
                   publisher_name = x.Key.publisher_name,
                   publisher_address = x.Key.publisher_address,
                   publisher_phone = x.Key.publisher_phone,
                   bookList = x.Where(y => !string.IsNullOrEmpty(y.book_title)).Select(y => new Publisher_Book()
                   {
                       Title_book = y.book_title
                   }).ToHashSet()
               })
               .ToListAsync();
                return Ok(result);
            }
            catch
            {
                return NotFound();
            }
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<IActionResult> GetCategoriesAll(string? search)
        {
            try
            {
                var book = _context.Books.AsQueryable();
                var category = _context.Categories.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    category = category.Where(p => p.category_name.Contains(search));
                    return Ok(category);
                }
                #endregion

                // Ngược lại
                // Truy vấn nhiều bảng
                var result = await (from c in _context.Categories
                                    join b in _context.Books on c.Id equals b.category_id into bc
                                    from b in bc.DefaultIfEmpty() // Left join
                                    select new
                                    {
                                        category_id = c.Id,
                                        category_name = c.category_name,
                                        book_title = b == null ? "" : b.book_title
                                    })
               .GroupBy(x => new { x.category_id, x.category_name })
               .Select(x => new
               {
                   category_id = x.Key.category_id,
                   category_name = x.Key.category_name,
                   bookList = x.Where(y => !string.IsNullOrEmpty(y.book_title)).Select(y => new Category_BooksModel()
                   {
                       TitleBooks = y.book_title,
                   }).ToHashSet()
               })
               .ToListAsync();
                return Ok(result);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
