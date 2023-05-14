namespace bms_web_api.Models
{
    public class LoginModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
    }
    public class UpdateUserModel
    {
        public string password { get; set; }
        public string role { get; set; }
    }
    public class UpdatePasswordModel
    {
        public string password { get; set; }
    }
    public class UpdateRoleModel
    {
        public string role { get; set; }
    }
}
