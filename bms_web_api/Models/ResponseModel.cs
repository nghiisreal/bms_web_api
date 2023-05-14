namespace bms_web_api.Models
{
    public class ResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Token { get; set; }
    }
}
