using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebNoiThat.Extension;
using WebNoiThat.Models;
using WebNoiThat.ModelViews;

namespace WebNoiThat.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly dbBanHangContext _context;
        public INotyfService _notyfService { get; }

        public ShoppingCartController(dbBanHangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int productID, int? amount)
        {
            List<CartItem> cart = GioHang;
            try
            {
                // Tìm xem sản phẩm đã có trong giỏ hàng chưa
                CartItem item = cart.SingleOrDefault(p => p.product.MaSp == productID);
                if (item != null) // Đã có
                {
                    SanPham product = _context.SanPhams.SingleOrDefault(p => p.MaSp == productID);
                    if (amount.Value + item.amount >= product.SoLuongCo)
                    {
                        item.amount = (int)product.SoLuongCo; // Giới hạn theo số lượng có
                    }
                    else
                    {
                        item.amount = item.amount + amount.Value;
                    }

                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                else
                {
                    SanPham hh = _context.SanPhams.SingleOrDefault(p => p.MaSp == productID);
                    item = new CartItem
                    {
                        amount = amount.HasValue ? amount.Value : 1,
                        product = hh
                    };
                    cart.Add(item);//thêm vào giỏ
                }
                // Lưu session
                HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                _notyfService.Success("Thêm sản phẩm vào giỏ hàng thành công");
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCart(int productID, int? amount)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            try
            {
                if (cart != null)
                {
                    CartItem item = cart.SingleOrDefault(p => p.product.MaSp == productID);
                    SanPham product = _context.SanPhams.SingleOrDefault(p => p.MaSp == productID);

                    if (item != null && amount.HasValue)
                    {
                        if (amount.Value >= product.SoLuongCo)
                        {
                            item.amount = (int)product.SoLuongCo;
                        }
                        else if (amount.Value <= 0)
                        {
                            item.amount = 1;
                        }
                        else
                        {
                            item.amount = amount.Value;
                        }
                    }
                }
                // Lưu session
                HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                _notyfService.Success("Cập nhật giỏ hàng thành công");
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("api/cart/remove")]
        public IActionResult Remove(int productID)
        {
            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.product.MaSp == productID);
                if (item != null)
                {
                    gioHang.Remove(item);
                }
                // Lưu session
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                _notyfService.Success("Xóa sản phẩm khỏi giỏ hàng thành công");
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [Route("cart", Name = "Cart")]
        public IActionResult Index()
        {
            return View(GioHang);
        }
    }
}
