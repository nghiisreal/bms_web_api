namespace bms_web_api.Models
{
    public class CustomerModel
    {
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
    }
    public class CustomerIdModel
    {
        public int Id { get; set; }
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
    }
}
