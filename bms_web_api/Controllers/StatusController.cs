using bms_web_api.Data;
using bms_web_api.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public StatusController(MyDBContext context)
        {
            _context = context;
        }
        // Cập nhật trạng thái đơn hàng
        [HttpPut("{id}")]
       
        public async Task<IActionResult> UpdateStatus(string id, OrderStatusModel order)
        {
            try
            {
                // Lấy đơn hàng cần cập nhật từ cơ sở dữ liệu
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.order_id == id);

                // Nếu đơn hàng tồn tại thì thực hiện cập nhật tình trạng đơn hàng
                if (existingOrder != null)
                {
                    // Kiểm tra nếu status của đơn hàng là "Đang xử lý" và chuyển sang trạng thái "Đã duyệt đơn" thì mới trừ số lượng sách trong kho
                    if (existingOrder.status == "Đang xử lý" && order.status == "Đã duyệt đơn")
                    {
                        foreach (var orderItem in existingOrder.OrderItems)
                        {
                            // Lấy thông tin của sách từ cơ sở dữ liệu
                            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == orderItem.book_id);
                            if (book == null)
                            {
                                // Trả về lỗi nếu sách không tồn tại
                                return BadRequest("Sách với book_id " + orderItem.book_id + " không tồn tại.");
                            }
                            if (orderItem.quantity > book.book_quantity)
                            {
                                // Trả về lỗi nếu số lượng sách khách hàng đặt hàng lớn hơn số lượng tồn kho
                                return BadRequest("Số lượng sách khách hàng đặt hàng cho sách " + book.book_title + " với book_id " + orderItem.book_id + " vượt quá số lượng tồn kho là " + book.book_quantity);
                            }
                            // Trừ số lượng sách trong kho
                            book.book_quantity -= orderItem.quantity;
                            // Cập nhật thông tin sách vào cơ sở dữ liệu
                            _context.Entry(book).State = EntityState.Modified;
                        }

                        // Cập nhật trạng thái đơn hàng mới
                        existingOrder.status = order.status;
                        //Nếu khác đã nhận hàng thì không có thời gian nhận
                        //default(DateTime): 01 / 01 / 0001 12:00:00 AM
                        existingOrder.receive_date = default(DateTime);

                        //--------------------- Mã PXK
                        // Tìm PNK có mã lớn nhất trong database
                        var maxiepId = await _context.InventoryExportDatas.MaxAsync(o => o.iep_id);

                        // Tách phần số của mã PXK (XK và 5 số ra riêng) và tăng giá trị lên 1
                        var iepNumber = 1;
                        if (maxiepId != null)
                        {
                            iepNumber = int.Parse(maxiepId.Substring(2)) + 1;
                        }

                        // Ghép phần số vào với ký tự 'XK' và các số 0 ở trước để tạo mã nhập kho mới
                        // độ dài 5 ký tự và các số 0 ở trước (nếu cần)
                        var newiepId = $"XK{iepNumber:D5}";
                        var exOrder = existingOrder.order_id;
                        // Duyệt đơn thì mới Tạo mới PXK
                        var newIep = new InventoryExportData
                        {
                            iep_id = newiepId,
                            orderId = exOrder,
                            export_date = DateTime.Now,
                        };
                        // Thêm phiếu xuất kho mới vào database
                        _context.Add(newIep);
                    }
                    else
                    {
                        if (existingOrder.payment == "Đã thanh toán" && order.status == "Đã nhận hàng")
                        {
                            existingOrder.status = order.status;
                            existingOrder.receive_date = DateTime.Now;
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
                                    var cus = _context.Customers.FirstOrDefault(c => c.Id == existingOrder.customer_id);
                                    message.To.Add(new MailboxAddress(cus.customer_name, cus.customer_email));
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
                                          $"<h2>Tổng số tiền: <span style=\"color:red;\">{existingOrder.total_price.ToString("C", new CultureInfo("vi-VN"))}</span></h2>\n\n" + $"<p>Ngân hàng: Vietcombank</p>\n" + $"<p>STK: 0458784983927</p>\n\n" + $"<p>Mọi thắc mắc xin vui lòng liên hệ: </p>\n" + $"<p>Số điện thoại: <a href='tel:0913915763'>0913915763</a></p>\n" + $"<p>Email: <a href='mailto:tinlanhnhasach@gmail.com'>tinlanhnhasach@gmail.com</a></p>\n"
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                // Trả về lỗi
                                return BadRequest("Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.");
                            }
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
                            existingOrder.receive_date = default(DateTime);
                            return BadRequest("Vui lòng thanh toán đơn hàng trước khi nhận hàng!");
                        }

                    }
                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Entry(existingOrder).State = EntityState.Modified;
                }
                else
                {
                    return NotFound();

                }

                await _context.SaveChangesAsync();
                return Ok(existingOrder);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
