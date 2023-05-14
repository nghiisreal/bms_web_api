namespace bms_web_api.Data
{
    public class OrderItemData
    {
        public int Id { get; set; }
        // là Id của OrderData
        public string order_id { get; set; }
        public OrderData Order { get; set; }
        public int book_id { get; set; }
        public BookData Book { get; set; }
        public int quantity { get; set; }
        public double book_price { get; set; }
    }
}
