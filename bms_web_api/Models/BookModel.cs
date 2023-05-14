namespace bms_web_api.Models
{
    public class BookModel
    {
        public string ISBN { get; set; }
        public string book_title { get; set; }
        public double book_price { get; set; }
        public int num_pages { get; set; }
        public string book_des { get; set; }
        public string book_image { get; set; }
        public string user_book { get; set; }
        public string public_date { get; set; }
        public string category_name { get; set; }
        public string publisher_name { get; set; }
        public string author_name { get; set; }
        public DateTime update_date { get; set; }
    }
    public class BookModelId
    {
        public int book_id { get; set; }
        public string ISBN { get; set; }
        public string book_title { get; set; }
        public double book_price { get; set; }
        public int book_quantity { get; set; }
        public int num_pages { get; set; }
        public string book_des { get; set; }
        public string book_image { get; set; }
        public string user_book { get; set; }
        public string public_date { get; set; }
        public string category_name { get; set; }
        public string publisher_name { get; set; }
        public string author_name { get; set; }
        public int category_id { get; set; }
        public int publisher_id { get; set; }
        public int author_id { get; set; }
        public DateTime update_date { get; set; }
    }
    public class BookModelUpdate
    {
        public string ISBN { get; set; }
        public string book_title { get; set; }
        public double book_price { get; set; }
        public int num_pages { get; set; }
        public string book_des { get; set; }
        public string book_image { get; set; }
        public string user_book { get; set; }
        public string public_date { get; set; }
        public int category_id { get; set; }
        public int publisher_id { get; set; }
        public int author_id { get; set; }
        public DateTime update_date { get; set; }
    }
}
