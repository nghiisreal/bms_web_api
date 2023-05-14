using System.Globalization;

namespace bms_web_api.Models
{
    public class OrderModel
    {
        public string order_id { get; set; }
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
        public double total_price { get; set; }
        public DateTime order_date { get; set; }
        public string payment { get; set; }
        public string status { get; set; }
        //public DateTime GetOrderDate()
        //{
        //    // Chuyển đổi chuỗi ngày tháng năm sang kiểu DateTime
        //    return DateTime.ParseExact(order_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //}
        public HashSet<OrderItemModel> OrderItems { get; set; }
      
    }
    public class OrderCreateModel
    {
        public string order_id { get; set; }
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
        public DateTime order_date { get; set; }
        public string payment { get; set; }
        public string status { get; set; }
        //public DateTime GetOrderDate()
        //{
        //    // Chuyển đổi chuỗi ngày tháng năm sang kiểu DateTime
        //    return DateTime.ParseExact(order_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //}
        public HashSet<OrderItemCreateModel> OrderItems { get; set; }
       
    }
    public class OrderIdModel
    {
        public string order_id { get; set; }
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
        public double total_price { get; set; }
        public int customer_id { get; set; }
        public DateTime order_date { get; set; }
        public string payment { get; set; }
        public string status { get; set; }
        public DateTime receive_date { get; set; }
        //public DateTime GetOrderDate()
        //{
        //    // Chuyển đổi chuỗi ngày tháng năm sang kiểu DateTime
        //    return DateTime.ParseExact(order_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //}
        public HashSet<OrderItemModel> OrderItems { get; set; }
    }
    public class OrderModelUpdate
    {
        public int customer_id { get; set; }
        public DateTime order_date { get; set; }
        public string payment { get; set; }
        public string status { get; set; }
        //public DateTime GetOrderDate()
        //{
        //    // Chuyển đổi chuỗi ngày tháng năm sang kiểu DateTime
        //    return DateTime.ParseExact(order_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //}
        public HashSet<OrderItemCreateModel> OrderItems { get; set; }
    }
    public class OrderItemModel
    {
        public int book_id { get; set; }
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
    public class OrderStatusModel
    {
        public string status { get; set; }
        public DateTime receive_date { get; set; }
    }
    public class OrderPaymentModel
    {
        public string payment { get; set; }
    }
    public class OrderItemCreateModel
    {
        public int book_id { get; set; }
        public int Quantity { get; set; } = 1; // giá trị mặc định là 1
    }
}
