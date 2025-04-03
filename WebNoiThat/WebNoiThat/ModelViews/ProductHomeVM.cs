using WebNoiThat.Models;

namespace WebNoiThat.ModelViews
{
    public class ProductHomeVM
    {
        public LoaiSanPham category { get; set; }
        public List<SanPham> lsProducts { get; set; }
    }
}
