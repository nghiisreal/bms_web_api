using bms_web_api.Data;
using bms_web_api.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Crypto.Macs;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShippingController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public ShippingController(MyDBContext context)
        {
            _context = context;
        }
        private async Task SendEmail(OrderData existingOrder, Action callback)
        {
            try
            {
                // Lấy các item trong đơn hàng chi tiết
                if (existingOrder.OrderItems != null)
                {
                    string orderItemsDetails = "";
                    foreach (var item in existingOrder.OrderItems)
                    {
                        var book = _context.Books.FirstOrDefault(b => b.Id == item.book_id);
                        if (book != null)
                        {
                            orderItemsDetails += $"<h3>Tên sản phẩm: {book.book_title}, Giá: {item.book_price.ToString("C", new CultureInfo("vi-VN"))}, Số lượng: {item.quantity}</h3>\n";
                        }
                    }
                    // Gửi email hóa đơn
                    var message = new MimeMessage();
                    // Người gửi email
                    message.From.Add(new MailboxAddress("Nhà sách Tin Lành", "tinlanhnhasach@gmail.com"));
                    // Người nhận email
                    message.To.Add(new MailboxAddress(existingOrder.Customer.customer_name, existingOrder.Customer.customer_email));
                    message.Subject = $"Hóa đơn mua hàng {existingOrder.order_id}";
                    message.Body = new TextPart("html")
                    {
                        Text = $"<h1>Khách hàng {existingOrder.Customer.customer_name} đã nhận hàng và thanh toán đơn đặt hàng có mã {existingOrder.order_id}!</h1>\n" +
                        $"<h3>Chi tiết hóa đơn:</h3>\n" +
                        $"<p>Họ tên người mua: {existingOrder.Customer.customer_name}</p>\n" +
                        $"<p>Số điện thoại: {existingOrder.Customer.customer_phone}</p>\n" +
                        $"<p>Địa chỉ: {existingOrder.Customer.customer_address}</p>\n" +
                        $"<p>Ngày đặt hàng: {existingOrder.order_date}</p>\n" +
                        $"<p>Ngày nhận hàng: {existingOrder.receive_date}</p>\n" +
                        $"<p><span style=\"color:blue;\">{existingOrder.payment}</span></p>\n" +
                        $"<p><b><Chi tiết đơn hàng:</b></p>\n" +
                        $"{orderItemsDetails}" +
                          $"<h2>Tổng số tiền: <span style=\"color:red;\">{existingOrder.total_price.ToString("C", new CultureInfo("vi-VN"))}</span></h2>\n\n" + $"<p>Ngân hàng: Vietcombank</p>\n" + $"<p>STK: 0458784983927</p>\n\n" + $"<p>Mọi thắc mắc xin vui lòng liên hệ:</p> \n" + $"<p>Số điện thoại: <a href='tel:0913915763'>0913915763</a></p>\n" + $"<p>Email: <a href='mailto:tinlanhnhasach@gmail.com'>tinlanhnhasach@gmail.com</a></p>\n"
                    };
                    //Sử dụng một đối tượng SmtpClient để gửi email
                    using (var client = new SmtpClient())
                    {
                        //phương thức Connect để kết nối với máy chủ email
                        client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                        client.Authenticate("tinlanhnhasach@gmail.com", "ruknwqwrhxqumzlx");
                        client.Send(message);
                        client.Disconnect(true);
                    }
                }
                // Gọi hàm callback khi gửi email thành công
                callback();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Trả về lỗi
                // Trả về lỗi
                throw new Exception("Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.");
            }
        }
            // Cập nhật
            [HttpPut("{id}")]
       
        public async Task<IActionResult> UpdateShipping(string id, OrderStatusModel order)
        {
            try
            {
                // Lấy đơn hàng cần cập nhật từ cơ sở dữ liệu
                var existingOrder = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.order_id == id);

                // Nếu đơn hàng tồn tại thì thực hiện cập nhật tình trạng đơn hàng
                if (existingOrder != null)
                {
                    // Kiểm tra nếu status là "Đã nhận hàng" thì không cho cập nhật đơn hàng nữa
                    if (existingOrder.status == "Đã nhận hàng")
                    {
                        return BadRequest("Đơn hàng không thể cập nhật!");
                    }
                    else //Chưa nhận hàng
                    {
                        if (order.status == "Đã nhận hàng" && existingOrder.payment == "Đã thanh toán")
                        {
                            // Cập nhật thông tin dữ liệu đơn hàng
                            existingOrder.status = order.status;
                            existingOrder.receive_date = DateTime.Now;
                            // Gửi email bằng cách gọi hàm sendEmail với hàm callback
                            await SendEmail(existingOrder, () =>
                            {
                                // Hàm callback được gọi khi gửi email thành công
                                _context.SaveChanges();
                            });

                        }
                        else if (order.status == "Đang giao hàng")
                        {
                            // Cập nhật thông tin dữ liệu đơn hàng
                            existingOrder.status = order.status;
                            //default(DateTime): 01 / 01 / 0001 12:00:00 AM
                            existingOrder.receive_date = default(DateTime);
                        }
                        else if (order.status == "Không nhận hàng")
                        {
                            existingOrder.status = order.status;
                            existingOrder.receive_date = default(DateTime);
                            var ordered = await _context.Orders
                                            .Include(o => o.OrderItems)
                                            .ThenInclude(oi => oi.Book)
                                            .FirstOrDefaultAsync(o => o.order_id == id);

                            foreach (var orderItem in ordered.OrderItems)
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
                                orderItem.Book.book_quantity += orderItem.quantity;
                                var inventoryReceipt = new InventoryReceiptData
                                {
                                    irc_id = newIRCId,
                                    book_id = orderItem.book_id,
                                    book_quantity = orderItem.quantity,
                                    input_date = DateTime.Now
                                };
                                _context.InventoryReceiptDatas.Add(inventoryReceipt);

                                await _context.SaveChangesAsync();
                            }

                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // Cập nhật thông tin dữ liệu đơn hàng
                            //existingOrder.status = order.status;
                            //default(DateTime): 01 / 01 / 0001 12:00:00 AM
                            existingOrder.receive_date = default(DateTime);
                            return BadRequest("Vui lòng thanh toán đơn hàng trước khi nhận hàng!");
                        }
                        await _context.SaveChangesAsync();
                        // Trả về kết quả
                        return Ok(existingOrder);
                    }

                }
                return NoContent();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
