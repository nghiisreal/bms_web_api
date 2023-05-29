using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InventoryReceiptsController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public InventoryReceiptsController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<InventoryReceiptModel>>> GetInventoryReceiptsAll(string? search, string? sort, int page = 0)
        {
            try
            {
                var irc = _context.InventoryReceiptDatas.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    irc = irc.Where(p => p.irc_id.Contains(search));
                    return Ok(irc);
                }
                #endregion

                #region Sort
                irc = irc.OrderBy(p => p.irc_id);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "irc_desc":
                            irc = irc.OrderByDescending(p => p.irc_id);
                            break;
                        // Tên tăng dần
                        case "irc_asc":
                            irc = irc.OrderBy(p => p.irc_id);
                            break;
                    }
                }

                #endregion


                // Ngược lại
                // await truy vấn bất đồng bộ
                var result = await irc.Select(p => new InventoryReceiptModel
                {
                    irc_id = p.irc_id,
                    book_id = p.book_id,
                    book_title = p.Book.book_title,
                    quantity = p.book_quantity,
                    price = p.price,
                    totalPrice = p.totalPrice,
                    // Chuyển đổi kiểu Datetime CSDL qua kiểu string cho model
                    //input_date = p.input_date.ToString("dd-MM-yyyy")
                    input_date = p.input_date

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
       
        public async Task<IActionResult> GetInventoryReceiptId(string id)
        {
            try
            {
                // LINQ
                // Với điều kiện Id nhập vào bằng với Id PNK thì
                // sẽ trả về một đối tượng book với kiểu InventoryReceiptModel
                try
                {

                    var inventoryReceipt = await _context.InventoryReceiptDatas.Where(irc => irc.irc_id == id).Select(irc => new InventoryReceiptModel()
                    {
                        irc_id = irc.irc_id,
                        book_id = irc.book_id,
                        book_title = irc.Book.book_title,
                        quantity = irc.book_quantity,
                        price = irc.price,
                        totalPrice = irc.totalPrice,
                        //input_date = irc.input_date.ToString()
                        input_date = irc.input_date
                    }).FirstOrDefaultAsync();

                    return Ok(inventoryReceipt);
                }
                catch
                {
                    return BadRequest("Lỗi khi lấy lịch sử nhập kho");
                }

            }
            catch
            {
                return NotFound();
            }
        }

        // Mã phiếu nhập kho có dạng 'NK0001'
        [HttpGet]
       
        public async Task<ActionResult<string>> GetNewInventoryReceiptId()
        {
            // Tìm PNK có mã lớn nhất trong database
            var maxIRCId = await _context.InventoryReceiptDatas.MaxAsync(o => o.irc_id);

            // Tách phần số của mã PNK (NK và 5 số ra riêng) và tăng giá trị lên 1
            var ircNumber = 1;
            if (maxIRCId != null)
            {
                ircNumber = int.Parse(maxIRCId.Substring(2)) + 1;
            }

            // Ghép phần số vào với ký tự 'NK' và các số 0 ở trước để tạo mã nhập kho mới
            // độ dài 5 ký tự và các số 0 ở trước (nếu cần)
            var newIRCId = $"NK{ircNumber:D5}";

            return Ok(newIRCId);
        }
        // Thêm mới
        [HttpPost]
       
        public async Task<IActionResult> CreateInventoryReceipt(InventoryReceiptModel irc)
        {
            try
            {
                // Tìm theo id cuốn sách cần nhập kho
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == irc.book_id);
                if (book == null)
                {
                    return BadRequest("Không tìm thấy sách !");
                }

                var inventoryReceipt = new InventoryReceiptData
                {
                    irc_id = irc.irc_id,
                    book_id = irc.book_id,
                    book_quantity = irc.quantity,
                    price = irc.price,
                    totalPrice = irc.totalPrice,
                    // Chuyển đổi kiểu string ở model qua kiểu Datetime trong CSDL
                    //input_date = DateTime.ParseExact(irc.input_date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    input_date = DateTime.Now
                };

                _context.InventoryReceiptDatas.Add(inventoryReceipt);
                await _context.SaveChangesAsync();

                //Thêm số lượng sách được nhập vào từ phiếu nhập kho(irc.book_quantity) vào số lượng sách hiện tại
                //trong kho(book.book_quantity)
                book.book_quantity += irc.quantity;
                //Bảng book sẽ được cập nhật book_quantity đã thay đổi ở trên
                _context.Entry(book).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Ok(inventoryReceipt);
            }
            catch
            {
                return BadRequest("Lỗi tạo phiếu nhập kho !");
            }
        }
        // Xóa nhập kho
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> DeleteInventoryReceiptId(string id)
        {
            //Chức năng này dùng để phiếu nhập kho nếu không muốn tồn tại trên hệ thống nữa
            try
            {
                var irc = await _context.InventoryReceiptDatas
                    .FirstOrDefaultAsync(o => o.irc_id == id);

                if (irc == null)
                {
                    return NotFound("Không tìm phiếu nhập kho với id = " + id);
                }
                _context.Remove(irc);

                await _context.SaveChangesAsync();

                return Ok("Xóa phiếu nhập kho với mã " + irc.irc_id + " thành công!");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}