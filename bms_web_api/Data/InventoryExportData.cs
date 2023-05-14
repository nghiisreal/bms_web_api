namespace bms_web_api.Data
{
    public class InventoryExportData
    {
        public string iep_id { get; set; }
        public DateTime export_date { get; set; } = DateTime.Now;
        public string orderId { get; set; }
        public OrderData Order { get; set; }
    }
}
