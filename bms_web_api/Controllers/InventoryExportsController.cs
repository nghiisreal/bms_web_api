using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InventoryExportsController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public InventoryExportsController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<InventoryExportData>>> GetInventoryExportsAll(string? search, string? sort, int page = 0)
        {
            try
            {
                var iep = _context.InventoryExportDatas.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    iep = iep.Where(p => p.iep_id.Contains(search));
                    return Ok(iep);
                }
                #endregion

                #region Sort
                iep = iep.OrderBy(p => p.iep_id);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "iep_desc":
                            iep = iep.OrderByDescending(p => p.iep_id);
                            break;
                        // Tên tăng dần
                        case "iep_asc":
                            iep = iep.OrderBy(p => p.iep_id);
                            break;
                    }
                }

                #endregion
                

                // Ngược lại
                // await truy vấn bất đồng bộ
                var result = await iep.Select(o => new InventoryExportModel
                {
                    orderId = o.orderId,
                    iep_id = o.iep_id,
                    export_date = o.export_date,
                    OrderItemExport = o.Order.OrderItems.Select(oi => new OrderItemExportModel
                    {
                        book_id = oi.book_id,
                        BookTitle = oi.Book.book_title,
                        Quantity = oi.quantity,
                        Price = oi.book_price
                    }).ToHashSet()

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
       
        public async Task<IActionResult> GetInventoryExportId(string id)
        {
            try
            {
                // LINQ
                // Với điều kiện Id nhập vào bằng với Id PNK thì
                // sẽ trả về một đối tượng book với kiểu InventoryExportModel
                try
                {

                    var inventoryExport = await _context.InventoryExportDatas.Where(iep => iep.iep_id == id).Select(iep => new InventoryExportModel()
                    {
                        iep_id = iep.iep_id,
                        export_date = iep.export_date,
                        orderId = iep.orderId,
                        OrderItemExport = iep.Order.OrderItems.Select(oi => new OrderItemExportModel
                        {
                            book_id = oi.book_id,
                            BookTitle = oi.Book.book_title,
                            Quantity = oi.quantity,
                            Price = oi.book_price
                        }).ToHashSet()
                    }).FirstOrDefaultAsync();

                    return Ok(inventoryExport);
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

        //// Mã phiếu XK kho có dạng 'XK0001'
        //[HttpGet]
        //public async Task<ActionResult<string>> GetNewInventoryExportId()
        //{
        //    // Tìm PNK có mã lớn nhất trong database
        //    var maxiepId = await _context.InventoryExportDatas.MaxAsync(o => o.iep_id);

        //    // Tách phần số của mã PXK (XK và 4 số ra riêng) và tăng giá trị lên 1
        //    var iepNumber = 1;
        //    if (maxiepId != null)
        //    {
        //        iepNumber = int.Parse(maxiepId.Substring(2)) + 1;
        //    }

        //    // Ghép phần số vào với ký tự 'TL' và các số 0 ở trước để tạo mã nhập kho mới
        //    // độ dài 4 ký tự và các số 0 ở trước (nếu cần)
        //    var newiepId = $"XK{iepNumber:D4}";

        //    return Ok(newiepId);
        //}
        
    }
}
