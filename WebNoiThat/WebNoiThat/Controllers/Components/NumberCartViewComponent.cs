using Microsoft.AspNetCore.Mvc;
using WebNoiThat.Extension;
using WebNoiThat.ModelViews;

namespace WebNoiThat.Controllers.Components
{
    public class NumberCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            return View(cart);
        }
    }
}
