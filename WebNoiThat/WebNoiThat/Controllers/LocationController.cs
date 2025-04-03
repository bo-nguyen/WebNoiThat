using Microsoft.AspNetCore.Mvc;
using WebNoiThat.Models;

namespace WebNoiThat.Controllers
{
    public class LocationController : Controller
    {
        private readonly dbBanHangContext _context;

        public LocationController(dbBanHangContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult QuanHuyenList(string TinhThanhId)
        {
            var QuanHuyens = _context.QuanHuyens
                .Where(x => x.Matp == TinhThanhId)
                .OrderBy(x => x.Maqh)
                .ToList();
            return Json(QuanHuyens);
        }
        public IActionResult PhuongXaList(string QuanHuyenId)
        {
            var PhuongXas = _context.XaPhuongThiTrans
                .Where(x => x.Maqh == QuanHuyenId)
                .OrderBy(x => x.Maxa)
                .ToList();
            return Json(PhuongXas);
        }
    }
}
