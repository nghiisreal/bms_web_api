using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly MyDBContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public CustomersController(MyDBContext context)
        {
            _context = context;
        }
        // Lấy dữ liệu
        [HttpGet]
       
        public async Task<ActionResult<HashSet<CustomerIdModel>>> GetCustomersAll(string? search, string? sort, int page = 0)
        {
            try
            {
                var customer = _context.Customers.AsQueryable();
                #region Search
                if (!string.IsNullOrEmpty(search))
                {
                    customer = customer.Where(p => p.customer_name.Contains(search));
                    return Ok(customer);
                }
                #endregion

                #region Sort
                customer = customer.OrderBy(p => p.customer_name);
                if (!string.IsNullOrEmpty(sort))
                {
                    switch (sort)
                    {
                        // Tên giảm dần
                        case "customername_desc":
                            customer = customer.OrderByDescending(p => p.customer_name);
                            break;
                        // Tên tăng dần
                        case "customername_asc":
                            customer = customer.OrderBy(p => p.customer_name);
                            break;
                    }
                }

                #endregion


                // Ngược lại
                // await truy vấn bất đồng bộ
                var result = await customer.Select(p => new CustomerIdModel
                {
                    Id= p.Id,
                   customer_name= p.customer_name,
                   customer_address= p.customer_address,
                   customer_email= p.customer_email,
                   customer_phone= p.customer_phone,

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
       
        public async Task<IActionResult> GetCustomerId(int id)
        {
            try
            {
                // LINQ
                // Với điều kiện Id nhập vào bằng với Id customer thì
                // sẽ trả về một đối tượng book với kiểu CustomerModelId
                var getCustomer = await _context.Customers.Where(cus => cus.Id == id).Select(customer => new CustomerIdModel()
                {
                    Id = customer.Id,
                    customer_name = customer.customer_name,
                    customer_address = customer.customer_address,
                    customer_email = customer.customer_email,
                    customer_phone = customer.customer_phone,

                }).FirstOrDefaultAsync();
                return Ok(getCustomer);
            }
            catch
            {
                return NotFound();
            }
        }
        [HttpGet]
        public async Task<IActionResult> CustomersToExcel()
        {
            var ied = _context.Customers.AsQueryable();

            // Lấy dữ liệu từ cơ sở dữ liệu
            var result = await ied.Select(p => new CustomerIdModel
            {
                Id = p.Id,
                customer_name = p.customer_name,
                customer_address = p.customer_address,
                customer_email = p.customer_email,
                customer_phone = p.customer_phone,
            }).ToListAsync();

            // Tạo tệp Excel
            using (var package = new ExcelPackage())
            {
                // Tạo một trang tính mới
                var worksheet = package.Workbook.Worksheets.Add("Khách hàng");

                // Đặt tiêu đề cho các cột
                worksheet.Cells[1, 1].Value = "Mã khách hàng";
                worksheet.Cells[1, 2].Value = "Tên khách hàng";
                worksheet.Cells[1, 3].Value = "Địa chỉ";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Số điện thoại";

                // Ghi dữ liệu vào từng ô tương ứng
                for (int i = 0; i < result.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = result[i].Id;
                    worksheet.Cells[i + 2, 2].Value = result[i].customer_name;
                    worksheet.Cells[i + 2, 3].Value = result[i].customer_address;
                    worksheet.Cells[i + 2, 4].Value = result[i].customer_email;
                    worksheet.Cells[i + 2, 5].Value = result[i].customer_phone;
                }
                // Thiết lập tên tệp và kiểu MIME
                var fileName = "Customers.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Xuất tệp Excel như một mảng byte
                var fileBytes = package.GetAsByteArray();

                // Trả về tệp Excel dưới dạng phản hồi HTTP
                return File(fileBytes, contentType, fileName);
            }
        }
        // Thêm mới
        [HttpPost]
       
        public async Task<IActionResult> CreateNewCustomer(CustomerModel customer)
        {
            try
            {

                // Kiểm tra xem đã tồn tại email trên hệ thống chưa
                var existCustomer = await _context.Customers.FirstOrDefaultAsync(cus => cus.customer_email == customer.customer_email);

                if (existCustomer != null)
                {
                    // Nếu đã tồn tại thì trả về thông báo lỗi hoặc thực hiện hành động phù hợp
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Email khách hàng đã tồn tại !");

                }
                else
                {
                    var newCustomer = new CustomerData
                    {
                        customer_name = customer.customer_name,                 
                        customer_address = customer.customer_address,
                        customer_phone = customer.customer_phone,
                        customer_email = customer.customer_email,
                    };
                    _context.Add(newCustomer);
                    await _context.SaveChangesAsync();
                    return StatusCode(statusCode: StatusCodes.Status201Created, newCustomer);
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        // Cập nhật dữ liệu theo ID
        [HttpPut("{id}")]
       
        public async Task<IActionResult> Updateid(int id, CustomerModel model)
        {
            var customer = await _context.Customers.SingleOrDefaultAsync(cusId => cusId.Id == id);
            if (customer != null)
            {
                customer.customer_name = model.customer_name;
                customer.customer_address= model.customer_address;
                customer.customer_email= model.customer_email;
                customer.customer_phone = model.customer_phone;
                _context.SaveChanges();
                return Ok("Cập nhật thành công !");
            }
            else
            {
                return NotFound();
            }
        }
        // Xóa dữ liệu theo ID
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> Deleteid(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Include() để lấy cả thông tin các hóa đơn của KH đó, sau đó duyệt qua danh sách các hóa đơn và xóa chúng trước rồi mới đến KH
                    // Vì Order có quan hệ với oderItem nên phải dùng thêm phương thức ThenInclude để lấy được các sản phẩm từ hóa đơn đó
                    var customer = await _context.Customers.Include(a => a.Orders).ThenInclude(o => o.OrderItems).SingleOrDefaultAsync(a => a.Id == id);
                    if (customer != null)
                    {
                        foreach (var orderData in customer.Orders)
                        {
                            foreach (var item in orderData.OrderItems)
                            {
                                _context.OrderItems.Remove(item);
                            }
                            _context.Orders.Remove(orderData);
                        }
                        _context.Customers.Remove(customer);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return Ok("Xóa khách hàng thành công !");
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
}
