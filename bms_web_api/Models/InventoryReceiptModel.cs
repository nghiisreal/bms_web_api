namespace bms_web_api.Models
{
    public class InventoryReceiptModel
    {
        public string irc_id { get; set; }
        public DateTime input_date { get; set; }
        public int book_id { get; set; }
        public string book_title { get; set; }
        public int quantity { get; set; } = 1;

    }
}
