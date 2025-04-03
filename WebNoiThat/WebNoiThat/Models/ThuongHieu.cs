using System.ComponentModel.DataAnnotations;

namespace WebNoiThat.Models
{
    public partial class ThuongHieu
    {
        public ThuongHieu()
        {
            SanPhams = new HashSet<SanPham>();
        }

        public int MaTh { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string TenTh { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string MoTa { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
