using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebNoiThat.Extension;
using WebNoiThat.Helper;
using WebNoiThat.Models;

namespace WebNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShippersController : Controller
    {
        private readonly dbBanHangContext _context;
        public INotyfService _notyfService { get; }

        public ShippersController(dbBanHangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/Shippers
        public async Task<IActionResult> Index(int page = 1)
        {
            var taikhoanID = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("DangNhap", "AccountsAdmin");
            }

            var pageNumber = page;
            var pageSize = 10;

            List<Shipper> lsShip = new List<Shipper>();

            lsShip = _context.Shippers
                .AsNoTracking()
                .OrderByDescending(x => x.MaShipper).ToList();
            //
            PagedList<Shipper> models = new PagedList<Shipper>(lsShip.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;

            return View(models);
        }

        // GET: Admin/Shippers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(m => m.MaShipper == id);
            if (shipper == null)
            {
                return NotFound();
            }

            return View(shipper);
        }

        // GET: Admin/Shippers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Shippers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenShipper,Email,Sdt,MatKhau,LoaiXe,BienSo")] Shipper shipper)
        {
            try
            {
                // Loại bỏ validation cho Salt và TenHt
                ModelState.Remove("Salt");
                ModelState.Remove("TenHt");

                if (ModelState.IsValid)
                {
                    // Kiểm tra trùng email và số điện thoại
                    if (await _context.Shippers.AnyAsync(x => x.Email == shipper.Email))
                    {
                        _notyfService.Error("Email đã được sử dụng");
                        return View(shipper);
                    }

                    if (await _context.Shippers.AnyAsync(x => x.Sdt == shipper.Sdt))
                    {
                        _notyfService.Error("Số điện thoại đã được sử dụng");
                        return View(shipper);
                    }

                    // Tạo salt và mã hóa mật khẩu
                    string salt = Utilities.GetRandomKey();
                    shipper.Salt = salt;
                    shipper.MatKhau = (shipper.MatKhau + salt.Trim()).ToMD5();

                    // Tạo tên hiển thị
                    shipper.TenHt = shipper.TenShipper + " - " + shipper.Sdt;

                    // Set trạng thái mặc định
                    shipper.Khoa = false;

                    _context.Add(shipper);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Tạo mới shipper thành công");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _notyfService.Error("Có lỗi xảy ra: " + ex.Message);
            }

            return View(shipper);
        }

        // GET: Admin/Shippers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipper = await _context.Shippers.FindAsync(id);
            if (shipper == null)
            {
                return NotFound();
            }
            return View(shipper);
        }

        // POST: Admin/Shippers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaShipper,TenShipper,Email,Sdt,MatKhau,LoaiXe,BienSo,Salt")] Shipper shipper)
        {
            if (id != shipper.MaShipper)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shipper);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShipperExists(shipper.MaShipper))
                    {
                        _notyfService.Warning("Cập nhật thất bại");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shipper);
        }

        // GET: Admin/Shippers/Delete/5
        // GET: Admin/Shippers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(m => m.MaShipper == id);
            if (shipper == null)
            {
                return NotFound();
            }

            return View(shipper);
        }

        // POST: Admin/Shippers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var shipper = await _context.Shippers.FindAsync(id);
                if (shipper != null)
                {
                    // Đảo ngược trạng thái khóa
                    shipper.Khoa = !(shipper.Khoa ?? false);
                    _context.Update(shipper);
                    await _context.SaveChangesAsync();
                    _notyfService.Success(shipper.Khoa.GetValueOrDefault() ? "Khóa thành công" : "Mở khóa thành công");
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _notyfService.Warning("Thao tác thất bại");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ShipperExists(int id)
        {
            return _context.Shippers.Any(e => e.MaShipper == id);
        }
    }
}
