namespace bms_web_api.Data
{
    public class InventoryReceiptData
    {
        public string irc_id { get; set; }
        public DateTime input_date { get; set; } = DateTime.Now;
        public int book_quantity { get; set; }
        // Khóa ngoại
        public int book_id { get; set; }
        public BookData Book { get; set; }
        public double price { get; set; }
        public double totalPrice { get; set; }

    }
}
