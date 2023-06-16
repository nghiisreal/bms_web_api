using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public BooksController(MyDBContext context)
        {
            _context = context;
        }

        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<BookModelId>>> GetBooksAll(string? search, string? sort, int page = 0)
        {
            try
            {
                var book = _context.Books.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    book = book.Where(p => p.book_title.Contains(search));
                    return Ok(book);
                }
                #endregion

                #region Sort
                book = book.OrderBy(p => p.book_title);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "booktitle_desc":
                            book = book.OrderByDescending(p => p.book_title);
                            break;
                        // Tên tăng dần
                        case "booktitle_asc":
                            book = book.OrderBy(p => p.book_title);
                            break;
                    }
                }

                #endregion


                // Ngược lại
                // await truy vấn bất đồng bộ
                var result = await book.Select(p => new BookModelId
                {
                    book_id = p.Id,
                    ISBN = p.ISBN,
                    book_title = p.book_title,
                    category_name = p.Category.category_name,
                    publisher_name = p.Publisher.publisher_name,
                    author_name = p.Author.author_name,
                    book_price = p.book_price,
                    book_quantity = p.book_quantity,
                    num_pages = p.num_pages,
                    book_des = p.book_des,
                    book_image = p.book_image,
                    user_book = p.user_book,
                    public_date = p.public_date.ToString("dd-MM-yyyy"),
                    // Chuyển đổi kiểu Datetime CSDL qua kiểu string cho model
                    //input_date = p.input_date.ToString("dd-MM-yyyy"),
                    update_date = p.update_date,
                    author_id = p.Author.Id,
                    publisher_id = p.Publisher.Id,
                    category_id = p.Category.Id

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
       
        public async Task<IActionResult> GetBookId(int id)
        {
            try
            {
                // LINQ
                // Với điều kiện Id nhập vào bằng với Id book thì
                // sẽ trả về một đối tượng book với kiểu BookModelId
                var getBook = await _context.Books.Where(b => b.Id == id).Select(book => new BookModelId()
                {
                    book_id = book.Id,
                    // Lấy ra được mã isbn, tên sách, ...
                   
                    ISBN = book.ISBN,
                    book_title = book.book_title,
                    book_price = book.book_price,
                    book_quantity = book.book_quantity,
                    num_pages = book.num_pages,
                    book_des = book.book_des,
                    book_image = book.book_image,
                    user_book = book.user_book,
                    public_date = book.public_date.ToString("dd-MM-yyyy"),
                    //input_date = book.input_date.ToString("dd-MM-yyyy"),
                    category_name = book.Category.category_name,
                    publisher_name = book.Publisher.publisher_name,
                    author_name = book.Author.author_name,
                    update_date = book.update_date,
                    author_id= book.author_id,
                    publisher_id= book.publisher_id,
                    category_id= book.category_id,
                }).FirstOrDefaultAsync();
                return Ok(getBook);
            }
            catch
            {
                return NotFound();
            }
        }
        [HttpGet]
        public async Task<IActionResult> BooksToExcel()
        {
            var ied = _context.Books.AsQueryable();

            // Lấy dữ liệu từ cơ sở dữ liệu
            var result = await ied.Select(p => new BookModelId
            {
                book_id = p.Id,
                ISBN = p.ISBN,
                book_title = p.book_title,
                category_name = p.Category.category_name,
                publisher_name = p.Publisher.publisher_name,
                author_name = p.Author.author_name,
                num_pages = p.num_pages,
                book_price = p.book_price,
                book_quantity = p.book_quantity,
                book_des = p.book_des,
                user_book = p.user_book,
                public_date = p.public_date.ToString("dd-MM-yyyy")
            }).ToListAsync();

            // Tạo tệp Excel
            using (var package = new ExcelPackage())
            {
                // Tạo một trang tính mới
                var worksheet = package.Workbook.Worksheets.Add("Sách");

                // Đặt tiêu đề cho các cột
                worksheet.Cells[1, 1].Value = "Mã sách";
                worksheet.Cells[1, 2].Value = "Mã ISBN";
                worksheet.Cells[1, 3].Value = "Tên sách";
                worksheet.Cells[1, 4].Value = "Thể loại";
                worksheet.Cells[1, 5].Value = "Nhà xuất bản";
                worksheet.Cells[1, 6].Value = "Tên tác giả";
                worksheet.Cells[1, 7].Value = "Số trang";
                worksheet.Cells[1, 8].Value = "Giá sách bán";
                worksheet.Cells[1, 9].Value = "Số lượng tồn";
                worksheet.Cells[1, 10].Value = "Mô tả";
                worksheet.Cells[1, 11].Value = "Đối tượng sử dụng";
                worksheet.Cells[1, 12].Value = "Ngày xuất bản";

                // Ghi dữ liệu vào từng ô tương ứng
                for (int i = 0; i < result.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = result[i].book_id;
                    worksheet.Cells[i + 2, 2].Value = result[i].ISBN;
                    worksheet.Cells[i + 2, 3].Value = result[i].book_title;
                    worksheet.Cells[i + 2, 4].Value = result[i].category_name;
                    worksheet.Cells[i + 2, 5].Value = result[i].publisher_name;
                    worksheet.Cells[i + 2, 6].Value = result[i].author_name;
                    worksheet.Cells[i + 2, 7].Value = result[i].num_pages;
                    worksheet.Cells[i + 2, 8].Value = result[i].book_price;
                    worksheet.Cells[i + 2, 9].Value = result[i].book_quantity;
                    worksheet.Cells[i + 2, 10].Value = result[i].book_des;
                    worksheet.Cells[i + 2, 11].Value = result[i].user_book;
                    worksheet.Cells[i + 2, 12].Value = result[i].public_date;
                }
                // Thiết lập tên tệp và kiểu MIME
                var fileName = "Books.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Xuất tệp Excel như một mảng byte
                var fileBytes = package.GetAsByteArray();

                // Trả về tệp Excel dưới dạng phản hồi HTTP
                return File(fileBytes, contentType, fileName);
            }
        }
        // Thêm mới
        [HttpPost]
       
        public async Task<IActionResult> CreateNewBookWithAuthor(BookModel book)
        {
            try
            {
                // Tìm theo tên loại sách LINQ [Object] Query SingleOrDefault

                var existCate = await _context.Categories.FirstOrDefaultAsync(c => c.category_name.ToUpper() == book.category_name.ToUpper());
                if (existCate == null)
                {
                    existCate = new CategoryData
                    {
                        category_name = book.category_name,
                    };
                    _context.Categories.Add(existCate);
                    await _context.SaveChangesAsync();
                }

                var existPublisher = await _context.Publishers.FirstOrDefaultAsync(p => p.publisher_name.ToUpper() == book.publisher_name.ToUpper());
                if (existPublisher == null)
                {
                    existPublisher = new PublisherData
                    {
                        publisher_name = book.publisher_name,
                        publisher_address = "No Infor",
                        publisher_phone = "No Infor"
                    };
                    _context.Publishers.Add(existPublisher);
                    await _context.SaveChangesAsync();
                }

                // Tìm theo tên tác giả LINQ [Object] Query SingleOrDefault
                var existAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.author_name.ToUpper() == book.author_name.ToUpper());

                // Nếu tên tác giả chưa tồn tại
                if (existAuthor == null)
                {
                    existAuthor = new AuthorData
                    {
                        author_name = book.author_name,
                    };
                    _context.Authors.Add(existAuthor);
                    await _context.SaveChangesAsync();
                };
                // Kiểm tra mã ISBN có tồn tại chưa
                var existBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
                if (existBook != null)
                {
                    return BadRequest("Mã ISBN " + book.ISBN + " đã tồn tại. Không thể tạo mới sách.");
                }
                else
                {
                    var newBook = new BookData
                    {
                        ISBN = book.ISBN,
                        book_title = book.book_title,
                        book_price = book.book_price,
                        num_pages = book.num_pages,
                        book_des = book.book_des,
                        book_image = book.book_image,
                        user_book = book.user_book,
                        // Chuyển đổi kiểu string ở model qua kiểu Datetime trong CSDL
                        //input_date = DateTime.ParseExact(book.input_date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        public_date = DateTime.ParseExact(book.public_date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        author_id = existAuthor.Id,
                        publisher_id = existPublisher.Id,
                        category_id = existCate.Id,
                        update_date = DateTime.Now,
                    };
                    _context.Books.Add(newBook);
                    await _context.SaveChangesAsync();
                    return Ok(newBook);
                }
            }
            catch
            {
                return BadRequest("Lỗi tạo mới !");
            }
           
        }

        // Cập nhật
        [HttpPut("{id}")]
       
        public async Task<IActionResult> Updateid(int id, BookModelUpdate model)
        {
            // Tìm theo tên tác giả, tên nhà xuất bản và tên loại
            var book = await _context.Books.Include(b => b.Author).Include(b => b.Publisher).Include(b => b.Category)
                     .SingleOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            if (book != null)
            {
                book.ISBN = model.ISBN;
                book.book_title = model.book_title;
                book.book_price = model.book_price;
                book.num_pages = model.num_pages;
                book.book_des = model.book_des;
                book.book_image = model.book_image;
                book.user_book = model.user_book;
                //book.input_date = DateTime.ParseExact(model.input_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                book.public_date = DateTime.ParseExact(model.public_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                book.update_date = DateTime.Now;
                book.author_id = model.author_id;
                book.category_id= model.category_id;
                book.publisher_id = model.publisher_id;
                await _context.SaveChangesAsync();
                return Ok(book);
            }
            else
            {
                return NotFound();
            }
        }

        // Xóa
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> Deleteid(int id)
        {
            try
            {
                var book = await _context.Books.SingleOrDefaultAsync(bId => bId.Id == id);
                if (book != null)
                {
                    // Kiểm tra xem quyển sách có liên kết với đơn hàng nào hay không
                    if (_context.OrderItems.Any(oi => oi.book_id == id))
                    {
                        // Nếu có liên kết, trả về thông báo lỗi
                        return BadRequest("Không thể xóa sách này vì nó đang nằm trong một đơn hàng.");
                    }
                    // Xóa toàn bộ lịch sử nhập kho của sách đó
                    var bookIrc = await _context.InventoryReceiptDatas.Where(bi => bi.book_id == id).ToListAsync();
                    _context.RemoveRange(bookIrc);
                    await _context.SaveChangesAsync();

                    // Nếu không có liên kết, xóa quyển sách
                    _context.Remove(book);
                    await _context.SaveChangesAsync();
                    return Ok("Xóa sách thành công !");
                }
                return NotFound();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
