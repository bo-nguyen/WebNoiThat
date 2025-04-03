using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebNoiThat.Helper;
using WebNoiThat.Models;

namespace WebNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamsController : Controller
    {
        private readonly dbBanHangContext _context;
        public INotyfService _notyfService { get; }
        private readonly IWebHostEnvironment _environment; // Thêm dòng này


        public SanPhamsController(dbBanHangContext context, INotyfService notyfService, IWebHostEnvironment environment)
        {
            _context = context;
            _notyfService = notyfService;
            _environment = environment; // Thêm dòng này
        }

        // GET: Admin/SanPhams
        public async Task<IActionResult> Index(int page = 1, int MaLoai = 0)
        {
            var taikhoanID = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("DangNhap", "AccountsAdmin");
            }

            var pageNumber = page;
            var pageSize = 10;

            List<SanPham> lsSanPham = new List<SanPham>();

            //filter op
            if (MaLoai != 0)
            {
                lsSanPham = _context.SanPhams
                .AsNoTracking()
                .Where(x => x.MaLoai == MaLoai)
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.MaThNavigation)
                .OrderByDescending(x => x.MaSp).ToList();
            }
            else
            {
                lsSanPham = _context.SanPhams
                .AsNoTracking()
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.MaThNavigation)
                .OrderByDescending(x => x.MaSp).ToList();
            }

            //page
            PagedList<SanPham> models = new PagedList<SanPham>(lsSanPham.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentMaLoai = MaLoai;

            //filter select
            List<SelectListItem> lsQuantityStt = new List<SelectListItem>();
            lsQuantityStt.Add(new SelectListItem() { Text = "Còn hàng", Value = "1" });
            lsQuantityStt.Add(new SelectListItem() { Text = "Hết hàng", Value = "0" });
            ViewData["lsQuantityStt"] = lsQuantityStt;

            //lấy slted value
            ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", MaLoai);
            ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh");

            return View(models);
        }

        //
        // Filter(int maLoai=0, int maTh=0, int stt=-1)
        public IActionResult Filter(int MaLoai = 0, int Stt = -1)
        {
            var url = $"/Admin/SanPhams?MaLoai={MaLoai}";
            if (MaLoai == 0)
            {
                url = $"/Admin/SanPhams";
            }
            else
            {
                //if(maLoai==0) url = $"/Admin/SanPhams?maTh={maTh}&stt={stt}";
            }
            return Json(new { status = "success", RedirectUrl = url });
        }

        // GET: Admin/SanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.MaThNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // GET: Admin/SanPhams/Create
        public IActionResult Create()
        {
            ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai");
            ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh");
            return View();
        }

        // POST: Admin/SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("MaSp,TenSp,GiaBan,GiaGiam,SoLuongCo,Anh,CongSuat,KhoiLuong,MoTa,BaoHanh,MaLoai,MaTh")] SanPham sanPham, IFormFile fAnh)
        //{
        //    try
        //    {
        //        // Debug để xem giá trị của ModelState
        //        foreach (var modelState in ModelState.Values)
        //        {
        //            foreach (var error in modelState.Errors)
        //            {
        //                _notyfService.Error(error.ErrorMessage);
        //            }
        //        }

        //        // Kiểm tra giá giảm
        //        if (sanPham.GiaGiam >= sanPham.GiaBan)
        //        {
        //            _notyfService.Warning("Giá giảm phải nhỏ hơn giá bán");
        //            ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
        //            ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
        //            return View(sanPham);
        //        }

        //        // Xử lý tên sản phẩm
        //        sanPham.TenSp = Utilities.ToTitleCase(sanPham.TenSp);

        //        // Xử lý upload ảnh
        //        if (fAnh != null)
        //        {
        //            string extension = Path.GetExtension(fAnh.FileName);
        //            string img = Utilities.SEOUrl(sanPham.TenSp) + "-" + Utilities.RandomGuid() + extension;
        //            sanPham.Anh = await Utilities.UploadFile(fAnh, @"sanpham", img.ToLower());
        //        }
        //        else
        //        {
        //            sanPham.Anh = "default.jpg";
        //        }

        //        // Thêm sản phẩm vào database
        //        _context.Add(sanPham);
        //        await _context.SaveChangesAsync();
        //        _notyfService.Success("Thêm sản phẩm thành công");
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        _notyfService.Error($"Lỗi: {ex.Message}");
        //        ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
        //        ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
        //        return View(sanPham);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("MaSp,TenSp,GiaBan,GiaGiam,SoLuongCo,Anh,CongSuat,KhoiLuong,MoTa,BaoHanh,MaLoai,MaTh")] SanPham sanPham, IFormFile fAnh)
        //{
        //    try
        //    {
        //        // Kiểm tra giá giảm
        //        if (sanPham.GiaGiam >= sanPham.GiaBan)
        //        {
        //            _notyfService.Warning("Giá giảm phải nhỏ hơn giá bán");
        //            ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
        //            ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
        //            return View(sanPham);
        //        }

        //        // Xử lý tên sản phẩm
        //        sanPham.TenSp = Utilities.ToTitleCase(sanPham.TenSp);

        //        // Xử lý upload ảnh
        //        if (fAnh != null)
        //        {
        //            string extension = Path.GetExtension(fAnh.FileName);
        //            string img = Utilities.SEOUrl(sanPham.TenSp) + "-" + Utilities.RandomGuid() + extension;
        //            sanPham.Anh = await Utilities.UploadFile(fAnh, @"sanpham", img.ToLower());
        //        }
        //        else
        //        {
        //            sanPham.Anh = "default.jpg";
        //        }

        //        // Thêm sản phẩm vào database
        //        _context.Add(sanPham);
        //        await _context.SaveChangesAsync();
        //        _notyfService.Success("Thêm sản phẩm thành công");
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        _notyfService.Error($"Lỗi: {ex.Message}");
        //        ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
        //        ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
        //        return View(sanPham);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSp,TenSp,GiaBan,GiaGiam,SoLuongCo,Anh,CongSuat,KhoiLuong,MoTa,BaoHanh,MaLoai,MaTh")] SanPham sanPham, IFormFile fAnh)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrEmpty(sanPham.TenSp))
                {
                    //_notyfService.Error("Vui lòng nhập tên sản phẩm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.GiaBan.HasValue || sanPham.GiaBan <= 0)
                {
                    //_notyfService.Error("Vui lòng nhập giá bán hợp lệ");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.GiaGiam.HasValue)
                {
                    //_notyfService.Error("Vui lòng nhập giá giảm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.SoLuongCo.HasValue || sanPham.SoLuongCo < 0)
                {
                    //_notyfService.Error("Vui lòng nhập số lượng hợp lệ");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (string.IsNullOrEmpty(sanPham.MoTa))
                {
                    //_notyfService.Error("Vui lòng nhập mô tả sản phẩm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.MaLoai.HasValue)
                {
                    //_notyfService.Error("Vui lòng chọn loại sản phẩm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.MaTh.HasValue)
                {
                    //_notyfService.Error("Vui lòng chọn thương hiệu");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                // Kiểm tra giá giảm
                if (sanPham.GiaGiam >= sanPham.GiaBan)
                {
                    //_notyfService.Warning("Giá giảm phải nhỏ hơn giá bán");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                // Xử lý tên sản phẩm
                sanPham.TenSp = Utilities.ToTitleCase(sanPham.TenSp);

                // Xử lý upload ảnh
                if (fAnh != null)
                {
                    string extension = Path.GetExtension(fAnh.FileName);
                    string img = Utilities.SEOUrl(sanPham.TenSp) + "-" + Utilities.RandomGuid() + extension;
                    sanPham.Anh = await Utilities.UploadFile(fAnh, @"sanpham", img.ToLower());
                }
                else
                {
                    sanPham.Anh = "default.jpg";
                }

                // Thêm sản phẩm vào database
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm sản phẩm thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyfService.Error($"Lỗi: {ex.Message}");
                ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                return View(sanPham);
            }
        }

        // GET: Admin/SanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
            return View(sanPham);
        }

        // POST: Admin/SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSp,TenSp,GiaBan,GiaGiam,SoLuongCo,Anh,CongSuat,KhoiLuong,MoTa,BaoHanh,MaLoai,MaTh")] SanPham sanPham, IFormFile fAnh)
        {
            try
            {
                if (id != sanPham.MaSp)
                {
                    return NotFound();
                }

                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrEmpty(sanPham.TenSp))
                {
                    //_notyfService.Error("Vui lòng nhập tên sản phẩm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.GiaBan.HasValue || sanPham.GiaBan <= 0)
                {
                    //_notyfService.Error("Vui lòng nhập giá bán hợp lệ");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.GiaGiam.HasValue)
                {
                    //_notyfService.Error("Vui lòng nhập giá giảm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.SoLuongCo.HasValue || sanPham.SoLuongCo < 0)
                {
                    //_notyfService.Error("Vui lòng nhập số lượng hợp lệ");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (string.IsNullOrEmpty(sanPham.MoTa))
                {
                    //_notyfService.Error("Vui lòng nhập mô tả sản phẩm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.MaLoai.HasValue)
                {
                    //_notyfService.Error("Vui lòng chọn loại sản phẩm");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                if (!sanPham.MaTh.HasValue)
                {
                    //_notyfService.Error("Vui lòng chọn thương hiệu");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                // Kiểm tra giá giảm
                if (sanPham.GiaGiam >= sanPham.GiaBan)
                {
                    //_notyfService.Warning("Giá giảm phải nhỏ hơn giá bán");
                    ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                    ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                    return View(sanPham);
                }

                // Lấy sản phẩm hiện tại từ database
                var existingProduct = await _context.SanPhams.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Xử lý tên sản phẩm
                sanPham.TenSp = Utilities.ToTitleCase(sanPham.TenSp);

                // Xử lý upload ảnh
                if (fAnh != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingProduct.Anh) && existingProduct.Anh != "default.jpg")
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, "images", "sanpham", existingProduct.Anh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string extension = Path.GetExtension(fAnh.FileName);
                    string img = Utilities.SEOUrl(sanPham.TenSp) + "-" + Utilities.RandomGuid() + extension;
                    sanPham.Anh = await Utilities.UploadFile(fAnh, @"sanpham", img.ToLower());
                }
                else
                {
                    // Giữ nguyên ảnh cũ nếu không upload ảnh mới
                    sanPham.Anh = existingProduct.Anh;
                }

                // Cập nhật thông tin sản phẩm
                _context.Entry(existingProduct).CurrentValues.SetValues(sanPham);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyfService.Error($"Lỗi: {ex.Message}");
                ViewData["LoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
                ViewData["ThuongHieu"] = new SelectList(_context.ThuongHieus, "MaTh", "TenTh", sanPham.MaTh);
                return View(sanPham);
            }
        }

        // GET: Admin/SanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.MaThNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: Admin/SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(id);
                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();
                _notyfService.Success("Xóa thành công");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _notyfService.Warning("Xóa thất bại");
                return RedirectToAction(nameof(Index));
            }

        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSp == id);
        }
    }
}
