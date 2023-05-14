namespace bms_web_api.Models
{
    public class InventoryExportModel
    {
        public string iep_id { get; set; }
        public string orderId { get; set; }
        public DateTime export_date { get; set; }
        public HashSet<OrderItemExportModel> OrderItemExport { get; set; }
    }

    public class OrderItemExportModel
    {
        public int book_id { get; set; }
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
