namespace bms_web_api.Models
{
    public class AuthorModel
    {
        public int author_id { get; set; }
        public string author_name { get; set; }
    };
    public class AuthorNoIdModel
    {
        public string author_name { get; set; }

    };
    // Để tác giả get dữ liệu với tên tác giả và tên sách
    public class AuthorWithBooksModel
    {
        public int author_id { get; set; }
        public string author_name { get; set; }

        public HashSet<Author_Book> AuthorAndBooks { get; set; }
    }


    public class Author_Book
    {
        public string Title_book { get; set; }
    }
}
