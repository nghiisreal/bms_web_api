using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bms_web_api.Data
{
    public class CategoryData
    {
        //[Key]
        public int Id { get; set; }
        //[Required]
        //[MaxLength(50)]
        public string category_name { get; set; }

        // Một thể loại có nhiều quyển sách
        // Quan hệ một - nhiều
        public HashSet<BookData> BookDatas { get; set; }
    }
}
