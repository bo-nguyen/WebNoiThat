using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNoiThat.Models;
using WebNoiThat.ModelViews;

namespace WebNoiThat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly dbBanHangContext _context;
        public HomeController(ILogger<HomeController> logger, dbBanHangContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            HomeViewVM model = new HomeViewVM();

            var lsProducts = _context.SanPhams.AsNoTracking()
                .OrderByDescending(x => x.MaSp)
                .ToList();

            List<ProductHomeVM> lsProductViews = new List<ProductHomeVM>();

            var lsCats = _context.LoaiSanPhams
                .AsNoTracking()
                .OrderBy(x => x.MaLoai)
                .Take(5)
                .ToList();

            foreach (var item in lsCats)
            {
                ProductHomeVM productHome = new ProductHomeVM();
                productHome.category = item;
                productHome.lsProducts = lsProducts.Where(x => x.MaLoai == item.MaLoai).Take(8).ToList();
                lsProductViews.Add(productHome);
            }

            model.Products = lsProductViews;
            ViewBag.AllProducts = lsProducts.Take(8).ToList();

            return View(model);
        }

        [Route("huong-dan-mua-hang")]
        public IActionResult ShoppingGuide()
        {
            return View();
        }

        [Route("ve-chung-toi")]
        public IActionResult About()
        {
            return View();
        }

        [Route("lien-he")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("chinh-sach-giao-hang")]
        public IActionResult ShippingPolicy()
        {
            return View();
        }

        [Route("chinh-sach-doi-tra")]
        public IActionResult ReturnPolicy()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
