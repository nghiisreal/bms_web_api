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
    public class OrdersController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public OrdersController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<OrderIdModel>>> GetOrdersAll(string? search, string? sort, int page = 0)
        {
            try
            {
                var order = _context.Orders.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    order = order.Where(o => o.order_id.ToString().Contains(search));
                    return Ok(order);
                }
                #endregion

                #region Sort
                // Đang xử lý và chưa thanh toán sẽ ở đầu tiên
                order = order.OrderBy(o => o.status != "Đang xử lý" && o.payment != "Chưa thanh toán")
               .ThenBy(o => o.status == "Đang xử lý" ? 0 : 1)
               .ThenBy(o => o.status == "Đã duyệt đơn" ? 0 : 1)
               .ThenBy(o => o.status == "Đang giao hàng" ? 0 : 1)
               .ThenBy(o => o.status == "Đã nhận hàng" ? 0 : 1);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Mã hóa đơn giảm dần
                        case "orderid_desc":
                            order = order.OrderByDescending(o => o.order_id);
                            break;
                        // Mã hóa đơn tăng dần
                        case "orderid_asc":
                            order = order.OrderBy(o => o.order_id);
                            break;
                    }
                }

                #endregion

                // Ngược lại
                // await truy vấn bất đồng bộ
                var result = await order.Select(order => new OrderIdModel
                {
                    order_id = order.order_id,
                    customer_id = order.customer_id,
                    customer_name = order.Customer.customer_name,
                    customer_address = order.Customer.customer_address,
                    customer_email = order.Customer.customer_email,
                    customer_phone = order.Customer.customer_phone,
                    // LINQ Sum
                    total_price = order.OrderItems.Sum(oi => oi.Book.book_price * oi.quantity),
                    OrderItems = order.OrderItems.Select(oi => new OrderItemModel
                    {
                        book_id = oi.Book.Id,
                        BookTitle = oi.Book.book_title,
                        Quantity = oi.quantity,
                        Price = oi.Book.book_price
                    }).ToHashSet(),

                    order_date = order.order_date,
                    payment = order.payment,
                    status = order.status,
                    // fix kiểu Datetime?
                    receive_date = order.receive_date.GetValueOrDefault(),
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
       
        public async Task<IActionResult> GetOrderWithOrderItems(string id)
        {
            try
            {
                var getOrderWithOrderItems = await _context.Orders.Where(p => p.order_id == id).Select(order => new OrderIdModel()
                {
                    order_id = order.order_id,
                    customer_id = order.customer_id,
                    customer_name = order.Customer.customer_name,
                    customer_address = order.Customer.customer_address,
                    customer_email = order.Customer.customer_email,
                    customer_phone = order.Customer.customer_phone,
                    total_price = order.OrderItems.Sum(oi => oi.Book.book_price * oi.quantity),
                    OrderItems = order.OrderItems.Select(oi => new OrderItemModel
                    {
                        book_id = oi.book_id,
                        BookTitle = oi.Book.book_title,
                        Quantity = oi.quantity,
                        Price = oi.Book.book_price
                    }).ToHashSet(),
                    // Chuyển đổi kiểu Datetime CSDL qua kiểu string cho model
                    order_date = order.order_date,
                    payment = order.payment,
                    status = order.status,
                    receive_date = order.receive_date.GetValueOrDefault(),
                }).FirstOrDefaultAsync();
                return Ok(getOrderWithOrderItems);
            }
            catch
            {
                return NotFound();
            }
        }
        // Mã đơn hàng có dạng 'TL00001'
        [HttpGet]
        public async Task<ActionResult<string>> GetNewOrderId()
        {
            // Tìm đơn hàng có mã lớn nhất trong database
            var maxOrderId = await _context.Orders.MaxAsync(o => o.order_id);

            // Tách phần số của mã đơn hàng (TL và 5 số ra riêng) và tăng giá trị lên 1
            var orderNumber = 1;
            if (maxOrderId != null)
            {
                orderNumber = int.Parse(maxOrderId.Substring(2)) + 1;
            }

            // Ghép phần số vào với ký tự 'TL' và các số 0 ở trước để tạo mã đơn hàng mới
            // độ dài 5 ký tự và các số 0 ở trước (nếu cần)
            var newOrderId = $"TL{orderNumber:D5}";

            return Ok(newOrderId);
        }
        // Thêm mới
        [HttpPost]
       
        public async Task<IActionResult> CreateNewOrder(OrderCreateModel order)
        {
            try
            {

                var existCus = await _context.Customers.FirstOrDefaultAsync(c => c.customer_name.ToUpper() == order.customer_name.ToUpper());
                if (existCus == null)
                {
                    existCus = new CustomerData
                    {
                        customer_name = order.customer_name,
                        customer_address = order.customer_address,
                        customer_email = order.customer_email,
                        customer_phone = order.customer_phone,
                    };
                    _context.Customers.Add(existCus);
                    await _context.SaveChangesAsync();
                }
                // Thiết lập thông tin khách hàng cho đơn hàng
                order.customer_address = existCus.customer_address;
                order.customer_email = existCus.customer_email;
                order.customer_phone = existCus.customer_phone;
                HashSet<OrderItemData> orderItems = new HashSet<OrderItemData>();
                // Duyệt qua danh sách các quyển sách khách hàng đặt
                foreach (var item in order.OrderItems)
                {
                    // Lấy thông tin của sách từ cơ sở dữ liệu
                    var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == item.book_id);
                    if (book == null)
                    {
                        // Trả về lỗi nếu sách không tồn tại
                        return BadRequest("Sách với book_id " + item.book_id + " không tồn tại.");
                    }
                    // Kiểm tra số lượng sách khách hàng đặt hàng so với số lượng tồn kho
                    if (item.Quantity > book.book_quantity)
                    {
                        // Trả về lỗi nếu số lượng sách khách hàng đặt hàng lớn hơn số lượng tồn kho
                        return BadRequest("Số lượng sách khách hàng đặt hàng cho sách " + book.book_title + " với book_id " + item.book_id + " vượt quá số lượng tồn kho.");
                    } 
                    // Trừ số lượng sách tương ứng trong kho
                    //book.book_quantity -= item.Quantity;
                    // Tạo một đối tượng OrderItem mới từ thông tin sách và số lượng đặt hàng
                    var orderItem = new OrderItemData
                    {
                        book_id = book.Id,
                        Book = book,
                        quantity = item.Quantity,
                        book_price = book.book_price,
                    };

                    orderItems.Add(orderItem);


                }
                // Tạo mới đơn hàng
                var newOrder = new OrderData
                {
                    order_id = order.order_id,
                    customer_id = existCus.Id,
                    OrderItems = orderItems,
                    total_price = orderItems.Sum(item => item.quantity * item.Book.book_price),
                    // Chuyển đổi kiểu string ở model qua kiểu Datetime trong CSDL
                    //order_date = DateTime.ParseExact(order.order_date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    order_date = DateTime.Now,
                    payment = order.payment,
                    status = order.status,
                };

                _context.Add(newOrder);
                await _context.SaveChangesAsync();
                return StatusCode(statusCode: StatusCodes.Status201Created, newOrder);
            }
            catch
            {
                return BadRequest();
            }
        }
        // Cập nhật
        [HttpPut("{id}")]
       
        public async Task<IActionResult> Updateid(string id, OrderModelUpdate order)
        {
            try
            {
                // Lấy đơn hàng cần cập nhật từ cơ sở dữ liệu
                var existingOrder = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.order_id == id);

                // Nếu đơn hàng tồn tại thì thực hiện cập nhật thông tin khách hàng và sản phẩm
                if (existingOrder != null)
                {
                    // Kiểm tra nếu status là "Đã duyệt đơn" hoặc "Đã nhận hàng" hoặc "Đang giao hàng" thì không cho cập nhật đơn hàng nữa
                    if (existingOrder.status == "Đã duyệt đơn" || existingOrder.status == "Đã nhận hàng" || existingOrder.status == "Đang giao hàng")
                    {
                        return BadRequest("Đơn hàng không thể cập nhật!");
                    }
                    else //Chưa nhận hàng
                    {
                        // Lấy đối tượng khách hàng từ đơn hàng
                        var existingCustomer = existingOrder.Customer;

                        // Nếu khách hàng tồn tại thì thực hiện cập nhật thông tin
                        if (existingCustomer != null)
                        {
                            existingOrder.customer_id = order.customer_id;
                            existingOrder.Customer = existingCustomer;

                            await _context.SaveChangesAsync();
                        }
                        // Cập nhật thông tin dữ liệu đơn hàng
                        existingOrder.status = order.status;
                        existingOrder.payment = order.payment;
                        existingOrder.order_date = DateTime.Now;
                        await _context.SaveChangesAsync();
                        // Xóa các sản phẩm cũ trong đơn hàng
                        //foreach (var orderItem in existingOrder.OrderItems)
                        //{
                        //    var book = await _context.Books.FindAsync(orderItem.book_id);
                        //    book.book_quantity += orderItem.quantity;
                        //    _context.Remove(orderItem);
                        //}

                        // Thêm các sản phẩm mới vào đơn hàng
                        foreach (var orderItem in order.OrderItems)
                        {
                            var book = await _context.Books.FindAsync(orderItem.book_id);
                            if (book == null)
                            {
                                return BadRequest($"Sách với mã {orderItem.book_id} không tồn tại.");
                            }
                            if (orderItem.Quantity > book.book_quantity)
                            {
                                // Trả về lỗi nếu số lượng sách khách hàng đặt hàng lớn hơn số lượng tồn kho
                                return BadRequest("Số lượng sách khách hàng đặt hàng cho sách " + book.book_title + " với book_id " + orderItem.book_id + " vượt quá số lượng tồn kho là " + book.book_quantity);
                            }
                            var newOrderItem = new OrderItemData
                            {
                                Book = book,
                                quantity = orderItem.Quantity,
                                book_price = book.book_price
                            };
                            existingOrder.OrderItems.Add(newOrderItem);

                            // Cập nhật số lượng sách trong kho
                            //book.book_quantity -= orderItem.Quantity;

                        }

                        // Lưu thay đổi vào cơ sở dữ liệu
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
        // Hủy đơn
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> Cancelid(string id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                    .FirstOrDefaultAsync(o => o.order_id == id);

                if (order == null)
                {
                    return NotFound("Không tìm thấy đơn hàng với id = " + id);
                }
                // Kiểm tra nếu status là "Đã nhận hàng" hoặc "Đang giao hàng" thì không cho hủy đơn hàng nữa
                if (order.status == "Đã nhận hàng" || order.status == "Đang giao hàng" || order.status == "Đã duyệt đơn")
                {
                    return BadRequest("Đơn hàng không thể hủy!");
                }
                //if (order.status == "Đang xử lý")
                //{
                //    // Tăng số lượng sách trong kho
                //    foreach (var orderItem in order.OrderItems)
                //    {
                //        orderItem.Book.book_quantity += orderItem.quantity;
                //    }
                //}
                _context.RemoveRange(order.OrderItems);
                _context.Remove(order);

                await _context.SaveChangesAsync();

                return Ok("Hủy đơn hàng với mã " + order.order_id + " thành công!");
            }
            catch
            {
                return BadRequest();
            }
        }
        // Xóa đơn
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> Deleteid(string id)
        {
            //Chức năng này dùng để xóa đơn hàng nếu không muốn tồn tại trên hệ thống nữa
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                    .FirstOrDefaultAsync(o => o.order_id == id);

                if (order == null)
                {
                    return NotFound("Không tìm thấy đơn hàng với id = " + id);
                }
                _context.RemoveRange(order.OrderItems);
                _context.Remove(order);

                await _context.SaveChangesAsync();

                return Ok("Xóa đơn hàng với mã " + order.order_id + " thành công!");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
