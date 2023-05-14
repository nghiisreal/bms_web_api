namespace bms_web_api.Data
{
    public class OrderData
    {
        public string order_id { get; set; }
        public double total_price { get; set; }
        public DateTime order_date { get; set; } = DateTime.Now;
        public string payment { get; set; }
        public string status { get; set; }
        public DateTime? receive_date { get; set;} // Có thể là giá trị null
        public int customer_id { get; set; }
        public CustomerData Customer { get; set; }

        // một đơn hàng sẽ có nhiều sản phẩm/order items
        public HashSet<OrderItemData> OrderItems { get; set; }
    }
}
