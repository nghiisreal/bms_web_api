using System.ComponentModel.DataAnnotations;

namespace bms_web_api.Data
{
    public class PublisherData
    {
        // [Key]
        public int Id { get; set; }
        //[Required(ErrorMessage = "Vui lòng nhập tên nhà xuất bản !")]
        //[MaxLength(120)]
        public string publisher_name { get; set; }
        //[Required(ErrorMessage = "Vui lòng nhập địa chỉ nhà xuất bản !")]
        //[MaxLength(100)]
        public string publisher_address { get; set; }
        //[Required(ErrorMessage = "Vui lòng nhập số điện thoại nhà xuất bản !")]
        //[MaxLength(20)]
        public string publisher_phone { get; set; }
        // Navigation Properties
        // Một nhà xuất bản có thể xuất bản nhiều quyển sách
        // Quan hệ một - nhiều
        public  HashSet<BookData> BookDatas { get; set; }
    }
}
