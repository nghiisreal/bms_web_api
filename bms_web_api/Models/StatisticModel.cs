namespace bms_web_api.Models
{
    public class StatisticModel
    {
        // Tổng đơn đặt hàng
        public int total_orders { get; set; }
        // Tổng số sách đã bán
        public int total_booksSold { get; set; }
        public int total_customers { get; set; }
        public double YearRevenue { get; set; }
        // Doanh thu 1 tháng
        public List<MonthRevenueModel> MonthRevenue { get; set; }
    }
    public class MonthRevenueModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public double Revenue { get; set; }
    }
    public class CustomerOrderStatisticModel
    {
        public string customer_name { get; set; }
        public int total_orders { get; set; }
    }
    public class TopSellingBookModel
    {
        public string title { get; set; }
        public int total_quantity { get; set; }
    }
}
