using bms_web_api.Data;
using bms_web_api.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Xml.Linq;
using System.Security.Principal;

namespace bms_web_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly JWT _jwt;
        public UsersController(MyDBContext context, IOptionsMonitor<JWT> jwt)
        {
            _context = context;
            _jwt = jwt.CurrentValue;
        }

        // Lấy dữ liệu
        [HttpGet]
        public async Task<IActionResult> GetUsersAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserId(int id)
        {
            // Tìm theo Id LINQ SingleOrDefault
            var user = await _context.Users.SingleOrDefaultAsync(uId => uId.userId == id);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet("{role}")]
        public async Task<IActionResult> GetRole(string role)
        {
            // Tìm theo Id LINQ SingleOrDefault
            var user = await _context.Users.Where(u => u.role.ToLower() == role.ToLower()).ToListAsync();
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }
        // mã hóa mật khẩu bằng SHA256
        private string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            SHA256Managed sha256 = new SHA256Managed();
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        // Mã xác nhận email
        [HttpGet]
        private string GenerateEmailConfirmationToken()
        {
            var bytes = new byte[32];
            using (var emailToken = new RNGCryptoServiceProvider())
            {
                emailToken.GetBytes(bytes);
            }
            return WebEncoders.Base64UrlEncode(bytes);
        }
        // Xóa token xác nhận email
        private async Task RemoveExpiredTokens()
        {
            var expiredTokens = await _context.Users
         .Where(t => t.isEmail_Confirmed == false && t.resetToken_time <= DateTime.Now)
         .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.Users.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmailUser(string emailToken)
        {
            try
            {
                // Tìm user có emailConfirmation_Token trùng với mã xác nhận nhập vào
                var user = await _context.Users.FirstOrDefaultAsync(t => t.emailConfirmation_Token == emailToken);

                // Kiểm tra thời gian resetToken_time và đảm bảo rằng token chỉ có thể được xác nhận trong vòng 1 phút sau khi được tạo ra
                var time = await _context.Users.FirstOrDefaultAsync(t => t.resetToken_time > DateTime.Now && t.isEmail_Confirmed == false);

                if (time == null)
                {
                    // Nếu đã hết hạn thì trả về thông báo lỗi
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Mã xác nhận đã hết hạn, vui lòng tạo lại!");
                }

                if (user == null)
                {
                    // Nếu không tìm thấy user nào với mã xác nhận này thì trả về thông báo lỗi
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Mã xác nhận không đúng!");
                }

                // Cập nhật thông tin user
                user.verify_time = DateTime.Now;
                user.isEmail_Confirmed = true;

                await _context.SaveChangesAsync();

                return Ok("Xác nhận thành công!");
            }
            catch
            {
                return BadRequest();
            }
        }
        // Thêm mới
        [HttpPost("register")]
        public async Task<IActionResult> CreateNewUser(RegisterModel user)
        {
            try
            {

                await RemoveExpiredTokens(); // Xóa tài khoản khi token hết hạn và không verify
                var existUser = await _context.Users.FirstOrDefaultAsync(us => us.user_email == user.user_email || us.username == user.username);
                if (existUser != null)
                {
                    // Nếu đã tồn tại thì trả về thông báo lỗi hoặc thực hiện hành động phù hợp
                    return StatusCode(statusCode: StatusCodes.Status409Conflict, "Email hoặc Tên đăng nhập này đã tồn tại !");

                }
                else
                {
                    var passwordHash = HashPassword(user.password);
                    //Console.WriteLine(passwordHash);
                    var newUser = new UserData
                    {
                        name = user.name,
                        username = user.username,
                        user_email = user.user_email,
                        password = passwordHash,
                        role = user.role,
                        isEmail_Confirmed = false,
                        emailConfirmation_Token = GenerateEmailConfirmationToken(),
                        // Mã xác nhận được giữ trong vòng 15 phút
                        resetToken_time = DateTime.Now.AddMinutes(15)
                    };
                    _context.Add(newUser);
                    await _context.SaveChangesAsync(); // Lưu lại
                    #region: SendEmailRegister
                    // Gửi email xác nhận đăng ký
                    var verify_token = newUser.emailConfirmation_Token;
                    var message = new MimeMessage();
                    // Người gửi email
                    message.From.Add(new MailboxAddress("Nhà sách Tin Lành", "tinlanhnhasach@gmail.com"));
                    // Người nhận email
                    message.To.Add(new MailboxAddress(newUser.username, newUser.user_email));
                    message.Subject = "Xác nhận đăng ký tài khoản";
                    message.Body = new TextPart("html")
                    {
                        Text = $"<h1>Tài khoản {newUser.username} đã được đăng ký!</h1>\n" +
                        $"<h3>Vui lòng xác nhận email của bạn bằng cách nhập mã <span style=\"color:red;\">{verify_token}</span> vào ô Xác nhận trên website.</h3>\n" +
                        $"<em>Lưu ý: Mã xác nhận sẽ hết hạn trong vòng 15 phút.</em>"
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
                    #endregion
                    return StatusCode(statusCode: StatusCodes.Status201Created, "Tạo tài khoản thành công!"); // 201 tạo mới thành công
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        private string GenerateToken(UserData user)
        {
            var jwtToken = new JwtSecurityTokenHandler();
            var secretKeyByte = Encoding.UTF8.GetBytes(_jwt.SecretKey);
            var tokenCode = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("email", user.user_email),
                    new Claim("name", user.name),
                    new Claim("username", user.username),
                    new Claim("id", user.userId.ToString()),
                     // roles
                     new Claim("role",user.role),
                     new Claim("tokenId", Guid.NewGuid().ToString())
                }),
                // Hết hạn token 30 ngày
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyByte), SecurityAlgorithms.HmacSha256Signature)
            };
            //Console.WriteLine(tokenCode);
            var tokenLogin = jwtToken.CreateToken(tokenCode);
            user.login_Token = jwtToken.WriteToken(tokenLogin);
            return user.login_Token;
        }
        //Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (model != null)
            {
                // Lấy ra user từ CSDL
                var result = await _context.Users.Where(p => p.username == model.username.ToLower() && p.role == model.role).FirstOrDefaultAsync();

                if (result == null) // Không đúng người dùng
                {
                    return BadRequest(new ResponseModel
                    {
                        Success = false,
                        Message = "Không tồn tại người dùng này"
                    });
                }
                else
                {
                    // Giải mã password hash của user từ database
                    string storedHash = result.password;

                    // Mã hóa password người dùng nhập vào
                    string passwordHash = HashPassword(model.password);
                    if (storedHash == passwordHash)
                    {
                        if (result.isEmail_Confirmed == true)
                        {
                            Console.WriteLine(User.Claims);
                            // Đăng nhập thành công

                            return Ok(new ResponseModel
                            {
                                Success = true,
                                Message = "Đăng nhập thành công!",
                                Token = GenerateToken(result)
                            });
                        }
                        return BadRequest(new ResponseModel
                        {
                            Success = false,
                            Message = "Chưa xác nhận email đăng ký tài khoản.\nKhông thể đăng nhập!"
                        });
                    }
                    else
                    {
                        // Đăng nhập thất bại
                        return BadRequest(new ResponseModel
                        {
                            Success = false,
                            Message = "Tên đăng nhập hoặc mật khẩu đã sai"
                        });
                    }
                }
            }
            else
            {
                return NotFound("No data !");
            }
        }
        // Cập nhật
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserId(int id, LoginModel model)
        {
            // Tìm theo Id LINQ [Object] Query SingleOrDefault
            var user = await _context.Users.SingleOrDefaultAsync(uId => uId.userId == id);
            var passwordHash = HashPassword(model.password);
            if (user != null)
            {
                user.username = model.username;
                user.password = passwordHash;
                user.role = model.role;
                await _context.SaveChangesAsync();
                return Ok("Cập nhật thành công !");
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPut("{email}")]
        public async Task<IActionResult> ChangePassword(string email, UpdatePasswordModel model)
        {
            // Tìm theo email LINQ [Object] Query SingleOrDefault
            var user = await _context.Users.SingleOrDefaultAsync(u => u.user_email == email);
            var passwordHash = HashPassword(model.password);
            if (user != null)
            {
                user.password = passwordHash;
                await _context.SaveChangesAsync();
                return Ok("Cập nhật thành công !");
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> ChangeRole(int id, UpdateRoleModel model)
        {
            // Tìm theo email LINQ [Object] Query SingleOrDefault
            // Lấy user cần cập nhật từ cơ sở dữ liệu
            var user = await _context.Users.FirstOrDefaultAsync(u => u.userId == id);

            if (user != null)
            {
                user.role = model.role;
                await _context.SaveChangesAsync();
                return Ok("Cập nhật thành công !");
            }
            else
            {
                return NotFound();
            }
        }
        // Xóa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserId(int id)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(uId => uId.userId == id);
                if (user != null)
                {
                    // Xóa
                    _context.Remove(user);
                    await _context.SaveChangesAsync();
                    return Ok("Xóa tài khoản thành công !");
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
