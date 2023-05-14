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
    public class PaymentController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public PaymentController(MyDBContext context)
        {
            _context = context;
        }
        // Cập nhật thanh toán
        [HttpPut("{id}")]
       
        public async Task<IActionResult> UpdatePayment(string id, OrderPaymentModel order)
        {
            try
            {
                // Lấy đơn hàng cần cập nhật từ cơ sở dữ liệu
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.order_id == id);

                // Nếu đơn hàng tồn tại thì thực hiện cập nhật thanh toán
                if (existingOrder != null)
                {
                    // Cập nhật thanh toán đơn hàng mới
                    existingOrder.payment = order.payment;
                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Entry(existingOrder).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(existingOrder);
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
