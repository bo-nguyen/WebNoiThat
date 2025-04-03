using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebNoiThat.Helper;
using WebNoiThat.Models;
using WebNoiThat.ModelViews;
using Microsoft.EntityFrameworkCore;
using WebNoiThat.Extension;
using Microsoft.Extensions.Logging;

namespace WebNoiThat.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly dbBanHangContext _context;
        public INotyfService _notyfService { get; }
        private readonly ILogger<AccountsController> _logger;  // Thêm dòng này

        public AccountsController(dbBanHangContext context, INotyfService notyfService, ILogger<AccountsController> logger) // Thêm tham số logger
        {
            _context = context;
            _notyfService = notyfService;
            _logger = logger;  // Thêm dòng này
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult ValidatePhone(string Phone)
        {
            try
            {
                var kh = _context.KhachHangs.AsNoTracking()
                    .SingleOrDefault(x => x.Sdt.ToLower() == Phone);
                if (kh != null)
                {
                    return Json(false);
                }
                else return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult ValidateEmail(string Email)
        {
            try
            {
                var kh = _context.KhachHangs.AsNoTracking()
                    .SingleOrDefault(x => x.Email.ToLower() == Email);
                if (kh != null)
                {
                    return Json(false);
                }
                else return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [Route("taikhoan", Name = "TaiKhoanCuaToi")]
        public IActionResult Dashboard()
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID))
                {
                    return RedirectToAction("DangNhap");
                }

                if (!int.TryParse(taikhoanID, out int maKh))
                {
                    HttpContext.Session.Remove("CustomerId");
                    return RedirectToAction("DangNhap");
                }

                var khachhang = _context.KhachHangs.AsNoTracking()
                    .FirstOrDefault(x => x.MaKh == maKh);

                if (khachhang == null)
                {
                    HttpContext.Session.Remove("CustomerId");
                    return RedirectToAction("DangNhap");
                }

                var lsDonHang = _context.DonHangs
                    .Include(x => x.ChiTietDonHangs)
                    .Include(x => x.MaTtNavigation)
                    .AsNoTracking()
                    .Where(x => x.MaKh == khachhang.MaKh)
                    .OrderByDescending(x => x.NgayDat)
                    .ToList();

                ViewBag.DonHang = lsDonHang;
                return View(khachhang);
            }
            catch
            {
                HttpContext.Session.Remove("CustomerId");
                return RedirectToAction("DangNhap");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dangky", Name = "DangKy")]
        public IActionResult DangKyTaiKhoan()
        {
            return View();
        }

        /* [HttpPost]
[AllowAnonymous]
[Route("dangky", Name = "DangKy")]
public async Task<IActionResult> DangKyTaiKhoan(RegisterVM taikhoan)
{
    try
    {
        if (ModelState.IsValid)
        {
            // ... code kiểm tra và tạo tài khoản ...
            
            _context.Add(kh);
            await _context.SaveChangesAsync();
            
            // Lưu session
            HttpContext.Session.SetString("CustomerId", kh.MaKh.ToString());
            
            // Identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, kh.TenKh),
                new Claim("CustomerId", kh.MaKh.ToString())
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);

            _notyfService.Success("Đăng ký thành công");
            return RedirectToAction("Dashboard", "Accounts");  // Đảm bảo điều hướng đúng
        }
        return View(taikhoan);
    }
    catch
    {
        _notyfService.Error("Đăng ký không thành công");
        return View(taikhoan);
    }
}*/
        [HttpPost]
        [AllowAnonymous]
        [Route("dangky", Name = "DangKy")]
        public async Task<IActionResult> DangKyTaiKhoan(RegisterVM taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    KhachHang kh = new KhachHang
                    {
                        TenKh = taikhoan.FullName,
                        Sdt = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        MatKhau = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Khoa = false,
                        Salt = salt,
                        DiaChi = "",
                        Matp = "",
                        Maqh = "",
                        Maxa = ""
                    };

                    _context.Add(kh);
                    await _context.SaveChangesAsync();
                    //lưu session
                    HttpContext.Session.SetString("CustomerId", kh.MaKh.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");
                    //Identity
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, kh.TenKh),
                            new Claim("CustomerId", kh.MaKh.ToString())
                        };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);

                    _notyfService.Success("Đăng ký thành công");

                    return RedirectToAction("Dashboard", "Accounts");

                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch
            {
                _notyfService.Error("Đăng ký không thành công");
                return View(taikhoan);

            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dangnhap", Name = "DangNhap")]
        public IActionResult DangNhap(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {

                return RedirectToAction("Dashboard", "Accounts");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dangnhap", Name = "DangNhap")]
        public async Task<IActionResult> DangNhap(LoginViewModel customer, string returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.KhachHangs.AsNoTracking()
                        .SingleOrDefault(x => x.Email.Trim() == customer.UserName);
                    if (khachhang == null)
                    {
                        _notyfService.Warning("Thông tin đăng nhập không chính xác");
                        return View(customer);
                    }


                    string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();
                    if (khachhang.MatKhau != pass)
                    {
                        _notyfService.Warning("Thông tin đăng nhập không chính xác");
                        return View(customer);
                    }
                    if (khachhang.Khoa == true)
                    {
                        _notyfService.Error("Tài khoản bị khóa");
                        return View(customer);
                    }

                    // lưu session
                    HttpContext.Session.SetString("CustomerId", khachhang.MaKh.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");

                    //Identity
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, khachhang.TenKh),
                            new Claim("CustomerId", khachhang.MaKh.ToString())
                        };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);

                    _notyfService.Success("Đăng nhập thành công");
                    return RedirectToAction("Dashboard", "Accounts");

                }
            }
            catch
            {
                return RedirectToAction("DangKyTaiKhoan", "Accounts");

            }
            return View(customer);
        }
        [HttpGet]
        [Route("dangxuat", Name = "DangXuat")]
        public IActionResult DangXuat()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeInfo(ChangeInfoVM model)
        {
            try
            {
                // Lấy thông tin khách hàng hiện tại
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID))
                {
                    return RedirectToAction("DangNhap");
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(x => x.MaKh == Convert.ToInt32(taikhoanID));

                if (khachHang == null)
                {
                    return RedirectToAction("DangNhap");
                }

                // Kiểm tra mật khẩu hiện tại (bắt buộc)
                if (string.IsNullOrEmpty(model.PasswordNow))
                {
                    _notyfService.Error("Vui lòng nhập mật khẩu hiện tại");
                    return RedirectToAction("Dashboard");
                }

                string passwordHash = (model.PasswordNow + khachHang.Salt.Trim()).ToMD5();
                if (passwordHash != khachHang.MatKhau)
                {
                    _notyfService.Error("Mật khẩu hiện tại không đúng");
                    return RedirectToAction("Dashboard");
                }

                // Cập nhật thông tin cơ bản
                if (!string.IsNullOrEmpty(model.FullName))
                {
                    khachHang.TenKh = model.FullName.Trim();
                }

                if (!string.IsNullOrEmpty(model.Address))
                {
                    khachHang.DiaChi = model.Address.Trim();
                }

                // Kiểm tra và cập nhật mật khẩu mới (nếu có)
                if (!string.IsNullOrEmpty(model.Password))
                {
                    // Kiểm tra độ dài mật khẩu mới
                    if (model.Password.Length < 6)
                    {
                        _notyfService.Error("Mật khẩu mới cần tối thiểu 6 ký tự");
                        return RedirectToAction("Dashboard");
                    }

                    // Kiểm tra mật khẩu nhập lại
                    if (model.Password != model.ConfirmPassword)
                    {
                        _notyfService.Error("Mật khẩu mới không khớp");
                        return RedirectToAction("Dashboard");
                    }

                    // Cập nhật mật khẩu mới
                    khachHang.MatKhau = (model.Password + khachHang.Salt.Trim()).ToMD5();
                }

                // Lưu thay đổi
                _context.Update(khachHang);
                await _context.SaveChangesAsync();

                // Cập nhật lại session và claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, khachHang.TenKh),
            new Claim("CustomerId", khachHang.MaKh.ToString())
        };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);

                _notyfService.Success("Cập nhật thông tin thành công");
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _notyfService.Error("Có lỗi xảy ra: " + ex.Message);
                return RedirectToAction("Dashboard");
            }
        }
    }
}