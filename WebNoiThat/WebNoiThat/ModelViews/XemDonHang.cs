using WebNoiThat.Models;

namespace WebNoiThat.ModelViews
{
    public class XemDonHang
    {
        public DonHang DonHang { get; set; }
        public List<ChiTietDonHang> ChiTietDonHang { get; set; }
    }
}
