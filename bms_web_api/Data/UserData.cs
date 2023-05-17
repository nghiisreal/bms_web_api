using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bms_web_api.Data
{
    [Table("Users")]
    public class UserData
    {
        //[Key]
        public int userId { get; set; }
        // [Required(ErrorMessage = "Vui lòng nhập User Name !")] // NOT NULL
        // [MaxLength(20)]
        public string username { get; set; }
        public string name { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập Password !")] // NOT NULL
        // [MaxLength(20)]
        public string user_email { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string emailConfirmation_Token { get; set; }
        public bool isEmail_Confirmed { get; set; }
        public DateTime? verify_time { get; set; }
        public DateTime? resetToken_time { get; set; }
        public string login_Token { get; set; }
       
    }
}
