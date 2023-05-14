namespace bms_web_api.Data
{
    public class CustomerData
    {
        public int Id { get; set; }
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
        // một khách hàng sẽ có nhiều đơn hàng
        public HashSet<OrderData> Orders { get; set; }
    }
}
