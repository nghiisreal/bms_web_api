using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bms_web_api.Data
{
    public class BookData
    {
        // [Key]
        public int Id { get; set; }
        public string ISBN { get; set; }

        //[Required(ErrorMessage = "Vui lòng nhập tiêu đề quyển sách !")]
        //[MaxLength(100)]
        public string book_title { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập giá tiền !")]
        public double book_price { get; set; }
        //Số lượng tồn kho
        public int book_quantity { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập số trang !")]
        public int num_pages { get; set; }
     
        // [Required(ErrorMessage = "Vui lòng nhập mô tả !")]
        public string book_des { get; set; }

        // [Required(ErrorMessage = "Vui lòng nhập ảnh bìa sách !")]
        public string book_image { get; set; }
       
        public string user_book { get; set; }
        //[Required(ErrorMessage = "Vui lòng nhập ngày xuất bản !")]
        //[MaxLength(10)]
        public DateTime public_date { get; set; }
        // Ngày cập nhật
        public DateTime update_date { get; set; } = DateTime.Now;

        

        // --------------------------------------------------
        // Khóa ngoại
        public int publisher_id { get; set; }
        // relationship
        public PublisherData Publisher { get; set; }

        // --------------------------------------------------
        public int category_id { get; set; }
        // relationship
        public CategoryData Category { get; set; }

        public int author_id { get; set; }

        public AuthorData Author { get; set; }

        // Danh sách các quyển sách khi Customer order
        public HashSet<OrderItemData> OrderItems { get; set; }
        // Một quyển sách có nhiều phiếu nhập kho
        public HashSet<InventoryReceiptData> InventoryReceipts { get; set; }
    }
}
