namespace bms_web_api.Models
{
    public class PublisherModel
    {
        public string publisher_name { get; set; }

        public string publisher_address { get; set; }

        public string publisher_phone { get; set; }

    }
    public class PublisherIdModel
    {
        public int publisher_id { get; set; }
        public string publisher_name { get; set; }

        public string publisher_address { get; set; }

        public string publisher_phone { get; set; }
        // Navigation Properties
    }
    // Để dữ liệu của nhà xuất bản get dữ liệu với tên sách
    public class PublisherWithBook
    {
        public int publisher_id { get; set; }
        public string publisher_name { get; set; }

        public string publisher_address { get; set; }

        public string publisher_phone { get; set; }
        public HashSet<Publisher_Book> PublisherBook { get; set; }
    }


    public class Publisher_Book
    {
        public string Title_book { get; set; }
    }
}
