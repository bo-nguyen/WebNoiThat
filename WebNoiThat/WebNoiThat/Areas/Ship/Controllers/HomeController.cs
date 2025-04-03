using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebNoiThat.Models;

namespace WebNoiThat.Areas.Ship.Controllers
{
    [Area("Ship")]
    public class HomeController : Controller
    {
        private readonly dbBanHangContext _context;

        public INotyfService _notyfService { get; }

        public HomeController(dbBanHangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;

        }
        public async Task<IActionResult> Index(int page = 1, int TrangThai = 0)
        {
            var taikhoanID = HttpContext.Session.GetString("ShipId");
            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("DangNhap", "AccountsShip");
            }

            var pageNumber = page;
            var pageSize = 10;

            List<DonHang> lsDonHang = new List<DonHang>();

            //filter op
            if (TrangThai != 0)
            {
                if (TrangThai == 3 || TrangThai == 4 || TrangThai == 5)
                {
                    lsDonHang = _context.DonHangs
                .AsNoTracking()
                .Include(x => x.MaTtNavigation)
                .Where(x => x.MaTt == TrangThai && x.MaShipper == Convert.ToInt32(taikhoanID))
                .OrderByDescending(x => x.MaDh).ToList();
                }
                else
                {
                    TrangThai = 0;
                    lsDonHang = _context.DonHangs
                .AsNoTracking()
                .Where(x => x.MaTt == 3 || x.MaTt == 4 || x.MaTt == 5)
                .Where(x => x.MaShipper == Convert.ToInt32(taikhoanID))
                .Include(x => x.MaTtNavigation)
                .OrderByDescending(x => x.MaDh).ToList();
                }
            }
            else
            {
                lsDonHang = _context.DonHangs
                .AsNoTracking()
                .Where(x => x.MaTt == 3 || x.MaTt == 4 || x.MaTt == 5)
                .Where(x => x.MaShipper == Convert.ToInt32(taikhoanID))
                .Include(x => x.MaTtNavigation)
                .OrderByDescending(x => x.MaDh).ToList();
            }

            //page
            PagedList<DonHang> models = new PagedList<DonHang>(lsDonHang.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentTrangThai = TrangThai;

            var items = _context.TrangThaiDonHangs.Where(x => x.MaTt == 3 || x.MaTt == 4 || x.MaTt == 5).ToList();
            //lấy slted value
            ViewData["lsTrangThai"] = new SelectList(items, "MaTt", "TenTt", TrangThai);

            return View(models);
        }
        public IActionResult Filter(int TrangThai = 0)
        {
            var url = $"/Ship/Home?TrangThai={TrangThai}";
            if (TrangThai == 0)
            {
                url = $"/Ship/Home";
            }
            else
            {
                //if(maLoai==0) url = $"/Admin/SanPhams?maTh={maTh}&stt={stt}";
            }
            return Json(new { status = "success", RedirectUrl = url });
        }



        public string getLocation(string maxa, string maqh, string matp)
        {
            try
            {
                var xa = _context.XaPhuongThiTrans.AsNoTracking()
                    .SingleOrDefault(x => x.Maxa == maxa);
                var qh = _context.QuanHuyens.AsNoTracking()
                    .SingleOrDefault(x => x.Maqh == maqh);
                var tp = _context.TinhThanhPhos.AsNoTracking()
                    .SingleOrDefault(x => x.Matp == matp);

                if (xa != null && qh != null && tp != null)
                {
                    return $"{xa.Name}, {qh.Name}, {tp.Name}";
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
        //
        public async Task<IActionResult> ChangeStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaShipperNavigation)
                .Include(d => d.MaTtNavigation)
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(m => m.MaDh == id);

            var ctdh = _context.ChiTietDonHangs
                .AsNoTracking()
                .Include(x => x.MaSpNavigation)
                .Where(x => x.MaDh == donHang.MaDh)
                .OrderBy(x => x.MaSp)
                .ToList();

            string fullAddress = $"{donHang.DiaChi}, {getLocation(donHang.Maxa, donHang.Maqh, donHang.Matp)}";
            ViewBag.FullAddress = fullAddress;

            ViewBag.ChiTiet = ctdh;
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string KQ, DonHang donHang)
        {
            if (id != donHang.MaDh)
            {
                return NotFound();
            }

            try
            {
                // Lấy đơn hàng hiện tại từ database với đầy đủ thông tin
                var existingDonHang = await _context.DonHangs
                    .Include(d => d.MaKhNavigation)
                    .Include(d => d.MaTtNavigation)
                    .Include(d => d.ChiTietDonHangs)
                    .Include(d => d.MaShipperNavigation)
                    .FirstOrDefaultAsync(m => m.MaDh == id);

                if (existingDonHang == null)
                {
                    return NotFound();
                }

                // Cập nhật trạng thái
                if (existingDonHang.MaTt == 3)
                {
                    if (KQ == "TC")
                    {
                        existingDonHang.MaTt = 4;
                        existingDonHang.NgayShip = DateTime.Now;
                    }
                    else if (KQ == "TB")
                    {
                        existingDonHang.MaTt = 5;
                    }

                    _context.Update(existingDonHang);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật trạng thái thành công");
                }

                // Load lại chi tiết đơn hàng
                var ctdh = await _context.ChiTietDonHangs
                    .Include(x => x.MaSpNavigation)
                    .Where(x => x.MaDh == id)
                    .OrderBy(x => x.MaSp)
                    .ToListAsync();

                ViewBag.ChiTiet = ctdh;

                // Cập nhật địa chỉ đầy đủ
                string fullAddress = $"{existingDonHang.DiaChi}, {getLocation(existingDonHang.Maxa, existingDonHang.Maqh, existingDonHang.Matp)}";
                ViewBag.FullAddress = fullAddress;

                // Load lại đơn hàng một lần nữa để đảm bảo dữ liệu mới nhất
                var updatedDonHang = await _context.DonHangs
                    .Include(d => d.MaKhNavigation)
                    .Include(d => d.MaTtNavigation)
                    .Include(d => d.ChiTietDonHangs)
                    .Include(d => d.MaShipperNavigation)
                    .FirstOrDefaultAsync(m => m.MaDh == id);

                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DonHangExists(donHang.MaDh))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.MaDh == id);
        }
    }
}
