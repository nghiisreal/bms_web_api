namespace bms_web_api.Models
{
    public class CategoryModel
    {
        public string category_name { get; set; }
    }
    public class CategoryIdModel
    {
        public int category_id { get; set; }
        public string category_name { get; set; }
    }
    public class CategoryWithBooksModel
    {
        public int category_id { get; set; }
        public string category_name { get; set; }
        public HashSet<Category_BooksModel> CategoryAndBooks { get; set; }
    }
    public class Category_BooksModel
    {
        public string TitleBooks { get; set; }
        public HashSet<string> CategoryAndBooks { get; set; }
    }
}
