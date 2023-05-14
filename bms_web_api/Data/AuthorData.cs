using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bms_web_api.Data
{
    public class AuthorData
    {
        // [Key]
        public int Id { get; set; }
        // [Required(ErrorMessage = "Vui lòng nhập tên tác giả !")]
        // [MaxLength(100)]
        public string author_name { get; set; }

        // Vì một tác giả có thể viết nhiều quyển sách
        public HashSet<BookData> BookDatas { get; set; }
        public AuthorData()
        {
            BookDatas = new HashSet<BookData>();
        }
    }
}
