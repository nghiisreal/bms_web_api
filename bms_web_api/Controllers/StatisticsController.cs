using bms_web_api.Data;
using bms_web_api.Models;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public StatisticsController(MyDBContext context)
        {
            _context = context;
        }
        [HttpGet("TotalMonthYear")]
       
        public async Task<ActionResult<StatisticModel>> GetTotalMonthYear(int year)
        {
            // Lấy danh sách đơn đặt hàng đã hoàn thành trong năm
            var completedOrders = await _context.Orders.Where(o => o.status == "Đã nhận hàng" && o.order_date.Year == year).Include(o => o.OrderItems).ToListAsync();
            if (completedOrders != null)
            {
                // Tính tổng số đơn đặt hàng và tổng số sách đã bán
                var totalOrders = completedOrders.Count();
                //kiểm tra trước đó OrderItems có khác null hay không, nếu đúng thì mới tính toán tổng.
                // Nếu OrderItems bằng null, trả về giá trị mặc định là 0 bằng toán tử ??
                var totalBooksSold = completedOrders.Sum(o => o.OrderItems?.Sum(od => od.quantity) ?? 0);

                // Tính doanh thu bán được theo tháng
                // Nếu chưa thanh toán thì không được thống kê
                var monthRevenue = completedOrders.Where(o => o.payment != "Chưa thanh toán")
                                          .GroupBy(o => new { o.order_date.Year, o.order_date.Month })
                                         .Select(g => new MonthRevenueModel { Month = g.Key.Month, Year = g.Key.Year, Revenue = g.Sum(o => o.total_price) })
                                         .OrderBy(g => g.Year)
                                         .ThenBy(g => g.Month)
                                         .ToList();
                //Tổng doanh thu cả năm
                var totalYearRevenue = monthRevenue.Sum(m => m.Revenue);
                // Lấy danh sách tất cả đơn hàng trong năm
                var orderList = await _context.Orders.Where(o => o.order_date.Year == year).Include(o => o.OrderItems).ToListAsync();
                // Tổng số khách hàng đã đặt hàng trong 1 năm
                //Distinct: lấy giá trị duy nhất
                var totalCustomers = orderList.Select(o => o.customer_id).Distinct().Count();
                // Tạo một đối tượng StatisticModel để lưu thông tin thống kê
                var statistics = new StatisticModel
                {
                    total_orders = totalOrders,
                    total_booksSold = totalBooksSold,
                    total_customers = totalCustomers,
                    YearRevenue = totalYearRevenue,
                    MonthRevenue = monthRevenue,
                };


                // Trả về kết quả thống kê dưới dạng JSON
                return Ok(statistics);
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpGet("TopCustomers")]
       
        public async Task<ActionResult<HashSet<CustomerOrderStatisticModel>>> GetTopCustomers(int month, int year, int count)
        {
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            var completedOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.status == "Đã nhận hàng" && o.receive_date >= fromDate && o.receive_date <= toDate)
                .ToListAsync();

            var customerOrderStatistics = completedOrders
                .GroupBy(o => o.Customer)
                .Select(g => new CustomerOrderStatisticModel
                {
                    customer_name = g.Key.customer_name,
                    total_orders = g.Count()
                })
                .OrderByDescending(c => c.total_orders)
                .Take(count)
                .ToList();

            return Ok(customerOrderStatistics);
        }
        [HttpGet("TopSellingBooks")]
       
        public async Task<ActionResult<HashSet<TopSellingBookModel>>> GetTopSellingBooks()
        {
            var topSellingBooks = await _context.OrderItems
        .GroupBy(oi => oi.book_id)
        .Select(a => new
        {
            BookId = a.Key,
            Quantity = a.Sum(oi => oi.quantity)
        })
        .OrderByDescending(a => a.Quantity)
        .Take(5) // Lấy top 5 cuốn sách bán chạy nhất
        .ToListAsync();

            var bookIds = topSellingBooks.Select(a => a.BookId).ToList();

            var books = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();

            var result = books.Select(b => new TopSellingBookModel
            {
                title = b.book_title,
                total_quantity = topSellingBooks.FirstOrDefault(a => a.BookId == b.Id)?.Quantity ?? 0
            }).ToList();

            return Ok(result);
        }


    }
}
