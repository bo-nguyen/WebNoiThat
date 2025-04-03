using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebNoiThat.Models;

namespace WebNoiThat.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly dbBanHangContext _context;
        public INotyfService _notyfService { get; }
        public SanPhamController(dbBanHangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        [Route("sanpham")]
        public IActionResult Index(int? page, string? searchKey, int? sort)
        {
            try
            {
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 9;

                var lsSanPham = _context.SanPhams
                    .AsNoTracking()
                    .Include(s => s.MaLoaiNavigation)
                    .Include(s => s.MaThNavigation)
                    .OrderByDescending(x => x.MaSp).ToList();

                if (searchKey != null)
                {
                    lsSanPham = lsSanPham.Where(x => x.TenSp.ToLower().Contains(searchKey.ToLower())).ToList();
                }
                if (sort != null)
                {
                    if (sort == 1)
                    {
                        lsSanPham = lsSanPham.OrderBy(x => x.GiaGiam).ToList();
                    }
                    else if (sort == 2)
                    {
                        lsSanPham = lsSanPham.OrderByDescending(x => x.GiaGiam).ToList();
                    }
                }

                PagedList<SanPham> models = new PagedList<SanPham>(lsSanPham.AsQueryable(), pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;

                //
                var dmsp = _context.LoaiSanPhams
                    .AsNoTracking()
                    .OrderBy(x => x.MaLoai).ToList();
                ViewBag.DMSP = dmsp;
                //
                var spmoi = _context.SanPhams
                    .AsNoTracking()
                    .Include(s => s.MaLoaiNavigation)
                    .Include(s => s.MaThNavigation)
                    .OrderByDescending(x => x.MaSp)
                    .Take(3)
                    .ToList();
                ViewBag.SPM = spmoi;
                //
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [Route("sanpham/sort")]
        public IActionResult Sort(int sort)
        {
            var url = $"/sanpham?sort={sort}";
            if (sort == 0)
            {
                url = $"/sanpham";
            }
            else
            {
                //if(maLoai==0) url = $"/Admin/SanPhams?maTh={maTh}&stt={stt}";
            }
            return Json(new { status = "success", RedirectUrl = url });
        }

        [Route("sanphams/{MaLoai}")]
        public IActionResult List(int MaLoai, int page = 1, int? sort =  null)
        {
            try
            {
                //
                var dmsp = _context.LoaiSanPhams
                    .AsNoTracking()
                    .OrderBy(x => x.MaLoai).ToList();
                ViewBag.DMSP = dmsp;
                //

                var pageNumber = page;
                var pageSize = 9;
                var danhmuc = _context.LoaiSanPhams
                    .AsNoTracking()
                    .SingleOrDefault(x => x.MaLoai == MaLoai);

                var lsSanPham = _context.SanPhams
                    .AsNoTracking()
                    .Where(x => x.MaLoai == MaLoai)
                    .Include(s => s.MaLoaiNavigation)
                    .Include(s => s.MaThNavigation)
                    .OrderByDescending(x => x.MaSp).ToList();

                // Xử lý sắp xếp
                if (sort.HasValue)
                {
                    if (sort == 1)
                    {
                        lsSanPham = lsSanPham.OrderBy(x => x.GiaGiam).ToList();
                    }
                    else if (sort == 2)
                    {
                        lsSanPham = lsSanPham.OrderByDescending(x => x.GiaGiam).ToList();
                    }
                }

                PagedList<SanPham> models = new PagedList<SanPham>(lsSanPham.AsQueryable(), pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                ViewBag.CurrentLoaiSP = danhmuc;

                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

        }
        [Route("sanpham/{id}")]
        public IActionResult Detail(int id)
        {
            try
            {
                var product = _context.SanPhams
                .Include(x => x.MaLoaiNavigation)
                .Include(x => x.MaThNavigation)
                .FirstOrDefault(x => x.MaSp == id);

                if (product == null)
                {
                    return RedirectToAction("Index");
                }

                var lsProduct = _context.SanPhams
                    .AsNoTracking()
                    .Where(x => x.MaLoai == product.MaLoai && x.MaSp != id)
                    .Take(4)
                    .OrderByDescending(x => x.MaSp)
                    .ToList();
                ViewBag.SanPham = lsProduct;

                var lsDG = _context.DanhGiaSanPhams
                    .Include(x => x.MaKhNavigation)
                    .AsNoTracking()
                    .Where(x => x.MaSp == id)
                    .OrderByDescending(x => x.MaDg)
                    .Take(5)
                    .ToList();
                ViewBag.DanhGia = lsDG;

                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", "Home");

            }

        }

        [HttpPost]
        [Route("sanpham/danhgia")]
        public async Task<IActionResult> DanhGia(int? id, byte diem, string noiDung)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "SanPham");
            }
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID))
                {
                    return RedirectToAction("DangNhap", "Accounts");
                }
                var khachhang = _context.KhachHangs.AsNoTracking()
                    .SingleOrDefault(x => x.MaKh == Convert.ToInt32(taikhoanID));
                if (khachhang == null)
                {
                    return NotFound();
                }
                //

                //
                DanhGiaSanPham dgsp = new DanhGiaSanPham();
                dgsp.MaKh = khachhang.MaKh;
                dgsp.MaSp = id;
                dgsp.Diem = diem;
                dgsp.NoiDung = noiDung;
                dgsp.ThoiGian = DateTime.Now;

                _context.Add(dgsp);
                _context.SaveChanges();

                _notyfService.Success("Đánh giá thành công");

                return RedirectToAction("Detail", "SanPham", new { id = id });

            }
            catch
            {
                return RedirectToAction("Index", "SanPham", new { id = id });
            }

        }

    }
}
