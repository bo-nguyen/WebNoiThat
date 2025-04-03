using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebNoiThat.Models.Payments;
using WebNoiThat.Models;
using WebNoiThat.ModelViews;
using WebNoiThat.Services;
using WebNoiThat.Extension;
using Microsoft.EntityFrameworkCore;

namespace WebNoiThat.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly dbBanHangContext _context;
        public INotyfService _notyfService { get; }
        private readonly IVnPayService _vnPayService;

        public CheckoutController(dbBanHangContext context, INotyfService notyfService, IVnPayService vnPayService)
        {
            _context = context;
            _notyfService = notyfService;
            _vnPayService = vnPayService;
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

        [Route("checkout")]
        public IActionResult Index(string returnUrl = null)
        {

            //lấy giỏ
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("DangNhap", "Accounts");
            }
            if (cart == null)
            {
                return RedirectToAction("Index", "ShoppingCart");
            }
            MuaHangVM model = new MuaHangVM();

            if (taikhoanID != null)
            {
                var khachhang = _context.KhachHangs.AsNoTracking()
                    .SingleOrDefault(x => x.MaKh == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.MaKh;
                model.FullName = khachhang.TenKh;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Sdt;

                if (khachhang.DiaChi != null)
                {
                    model.Address = khachhang.DiaChi;
                }
                //tp-qh-xp
                if (khachhang.Matp != null)
                {
                    ViewData["lsTinhThanh"] = new SelectList(_context.TinhThanhPhos.OrderBy(x => x.Matp).ToList(), "Matp", "Name", khachhang.Matp);
                    if (khachhang.Maqh != null)
                    {
                        ViewData["lsQuanHuyen"] = new SelectList(_context.QuanHuyens.Where(x => x.Matp == khachhang.Matp).OrderBy(x => x.Maqh).ToList(), "Maqh", "Name", khachhang.Maqh);
                        if (khachhang.Maxa != null)
                        {
                            ViewData["lsPhuongXa"] = new SelectList(_context.XaPhuongThiTrans.Where(x => x.Maqh == khachhang.Maqh).OrderBy(x => x.Maxa).ToList(), "Maxa", "Name", khachhang.Maxa);
                        }
                        else
                        {
                            ViewData["lsPhuongXa"] = new SelectList(_context.XaPhuongThiTrans.Where(x => x.Maqh == khachhang.Maqh).OrderBy(x => x.Maxa).ToList(), "Maxa", "Name");
                        }
                    }
                    else
                    {
                        ViewData["lsQuanHuyen"] = new SelectList(_context.QuanHuyens.Where(x => x.Matp == khachhang.Matp).OrderBy(x => x.Maqh).ToList(), "Maqh", "Name");
                    }
                }
                else
                {
                    ViewData["lsTinhThanh"] = new SelectList(_context.TinhThanhPhos.OrderBy(x => x.Matp).ToList(), "Matp", "Name");
                }
            }

            var dsgg = _context.KhuyenMais
                .AsNoTracking()
                .Where(x => DateTime.Now >= x.NgayBatDau && DateTime.Now < x.NgayKetThuc)
                .ToList();
            ViewBag.DSGG = dsgg;
            ViewBag.GioHang = cart;
            return View(model);
        }

        [HttpPost]
        [Route("checkout")]
        public IActionResult Index(MuaHangVM muahang, int soTienGiamInput, int phiGiaoHangInput, string macode)
        {
            //lấy giỏ
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("DangNhap", "Accounts");
            }
            if (cart == null)
            {
                return RedirectToAction("Index", "ShoppingCart");
            }
            if (string.IsNullOrEmpty(muahang.TinhThanh) ||
                string.IsNullOrEmpty(muahang.QuanHuyen) ||
                string.IsNullOrEmpty(muahang.PhuongXa))
            {
                // Xử lý lỗi và load lại dữ liệu cần thiết
                ViewBag.GioHang = cart;
                var dsgg = _context.KhuyenMais
                    .AsNoTracking()
                    .Where(x => DateTime.Now >= x.NgayBatDau && DateTime.Now < x.NgayKetThuc)
                    .ToList();
                ViewBag.DSGG = dsgg;

                // Load lại danh sách địa chỉ
                ViewData["lsTinhThanh"] = new SelectList(_context.TinhThanhPhos.OrderBy(x => x.Name), "Matp", "Name", muahang.TinhThanh);
                if (!string.IsNullOrEmpty(muahang.TinhThanh))
                {
                    ViewData["lsQuanHuyen"] = new SelectList(_context.QuanHuyens.Where(x => x.Matp == muahang.TinhThanh).OrderBy(x => x.Name), "Maqh", "Name", muahang.QuanHuyen);
                    if (!string.IsNullOrEmpty(muahang.QuanHuyen))
                    {
                        ViewData["lsPhuongXa"] = new SelectList(_context.XaPhuongThiTrans.Where(x => x.Maqh == muahang.QuanHuyen).OrderBy(x => x.Name), "Maxa", "Name", muahang.PhuongXa);
                    }
                }
                return View(muahang);

            }
            try
            {
                //khoi tao don hang
                DonHang donhang = new DonHang();
                donhang.MaKh = Convert.ToInt32(taikhoanID);
                donhang.NgayDat = DateTime.Now;
                donhang.MaTt = 1; //trạng thái đơn hàng mới
                donhang.HoTen = muahang.FullName;
                donhang.Sdt = muahang.Phone;
                donhang.DiaChi = muahang.Address;
                donhang.Matp = muahang.TinhThanh;
                donhang.Maqh = muahang.QuanHuyen;
                donhang.Maxa = muahang.PhuongXa;
                donhang.PhuongThucThanhToan = "Thanh toán khi nhận hàng";
                donhang.TienShip = 50000;
                donhang.GiamGiaShip = donhang.TienShip - phiGiaoHangInput;
                donhang.GiamGia = soTienGiamInput;

                // Xử lý mã khuyến mãi nếu có
                if (soTienGiamInput > 0 && !string.IsNullOrEmpty(macode))
                {
                    var khuyenMai = _context.KhuyenMais.FirstOrDefault(x => x.MaNhap == macode);
                    if (khuyenMai != null)
                    {
                        donhang.MaKm = khuyenMai.MaKm;
                    }
                }

                donhang.TongTien = cart.Sum(x => x.TotalMoney) + donhang.TienShip - donhang.GiamGiaShip - donhang.GiamGia;

                _context.Add(donhang);
                _context.SaveChanges();

                // Tạo chi tiết đơn hàng
                foreach (var item in cart)
                {
                    ChiTietDonHang ctdh = new ChiTietDonHang();
                    ctdh.MaDh = donhang.MaDh;
                    ctdh.MaSp = item.product.MaSp;
                    ctdh.SoLuong = item.amount;
                    ctdh.GiaBan = item.product.GiaBan;
                    ctdh.GiaGiam = item.product.GiaGiam;
                    ctdh.TongTien = item.TotalMoney;

                    _context.Add(ctdh);

                    // Cập nhật số lượng sản phẩm
                    var sanpham = _context.SanPhams.Find(item.product.MaSp);
                    if (sanpham != null)
                    {
                        sanpham.SoLuongCo -= item.amount;
                        _context.Update(sanpham);
                    }
                }

                _context.SaveChanges();

                // Xóa giỏ hàng
                HttpContext.Session.Remove("GioHang");
                _notyfService.Success("Đặt hàng thành công");

                return RedirectToAction("Dashboard", "Accounts");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và load lại dữ liệu cần thiết
                ViewBag.GioHang = cart;
                var dsgg = _context.KhuyenMais
                    .AsNoTracking()
                    .Where(x => DateTime.Now >= x.NgayBatDau && DateTime.Now < x.NgayKetThuc)
                    .ToList();
                ViewBag.DSGG = dsgg;

                // Load lại danh sách địa chỉ
                ViewData["lsTinhThanh"] = new SelectList(_context.TinhThanhPhos.OrderBy(x => x.Name), "Matp", "Name", muahang.TinhThanh);
                if (!string.IsNullOrEmpty(muahang.TinhThanh))
                {
                    ViewData["lsQuanHuyen"] = new SelectList(_context.QuanHuyens.Where(x => x.Matp == muahang.TinhThanh).OrderBy(x => x.Name), "Maqh", "Name", muahang.QuanHuyen);
                    if (!string.IsNullOrEmpty(muahang.QuanHuyen))
                    {
                        ViewData["lsPhuongXa"] = new SelectList(_context.XaPhuongThiTrans.Where(x => x.Maqh == muahang.QuanHuyen).OrderBy(x => x.Name), "Maxa", "Name", muahang.PhuongXa);
                    }
                }

                _notyfService.Error("Đặt hàng không thành công: " + ex.Message);
                return View(muahang);
            }
        }

        //[Route("dat-hang-thanh-cong")]
        //public IActionResult Success()
        //{

        //    try
        //    {
        //        var taikhoanID = HttpContext.Session.GetString("CustomerId");
        //        if(string.IsNullOrEmpty(taikhoanID))
        //        {
        //            return RedirectToAction("Login", "Accounts", new { returnUrl = "/dat-hang-thanh-cong" });
        //        }
        //        var khachhang = _context.KhachHangs.AsNoTracking()
        //            .SingleOrDefault(x => x.MaKh == Convert.ToInt32(taikhoanID));
        //        var donhang = _context.DonHangs.Where(x=>x.MaKh == Convert.ToInt32(taikhoanID))
        //            .OrderByDescending(x=>x.MaDh)
        //            .FirstOrDefault();


        //    }
        //    catch
        //    {

        //    }
        //}

        public IActionResult CheckMaGG(string MaCode, int TongTien)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("DangNhap", "Accounts");
            }

            if (taikhoanID != null)
            {
                try
                {
                    var ma = _context.KhuyenMais.AsNoTracking()
                    .SingleOrDefault(x => x.MaNhap.ToLower() == MaCode.ToLower());
                    if (ma != null)
                    {
                        if (DateTime.Now < ma.NgayBatDau)
                        {
                            return Json(new { success = false, error = "Chưa đến ngày áp dụng mã" });
                        }
                        else if (DateTime.Now > ma.NgayKetThuc)
                        {
                            return Json(new { success = false, error = " Đã hết hạn sử dụng" });
                        }
                        else if (ma.SoLuong <= 0)
                        {
                            return Json(new { success = false, error = " Đã hết lượt sử dụng" });
                        }
                        else if (TongTien < ma.GiaTriToiThieu)
                        {
                            return Json(new { success = false, error = "Chưa đạt giá trị tối thiểu" });
                        }
                        else
                        {
                            var tienGiam = ma.GiaTriGiam;
                            return Json(new { success = true, tienGiam });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, error = "Mã giảm giá không tồn tại" });
                    }
                }
                catch
                {
                    return Json(new { success = false, error = "Vui lòng nhập mã" });

                }
            }
            return Json(new { success = false, error = "Lỗi" });
        }


        //vnpay
        public IActionResult CreatePaymentUrl(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            //Json(response);

            var thongTinDonHang = response.OrderDescription;
            string[] thongTinDonHangArr = thongTinDonHang.Split('-');

            string FullName = thongTinDonHangArr[0];
            string Phone = thongTinDonHangArr[1];
            string Address = thongTinDonHangArr[2];
            string TinhThanh = thongTinDonHangArr[3];
            string QuanHuyen = thongTinDonHangArr[4];
            string PhuongXa = thongTinDonHangArr[5];
            int soTienGiamInput = Convert.ToInt32(thongTinDonHangArr[6]);
            int phiGiaoHangInput = Convert.ToInt32(thongTinDonHangArr[7]);
            int tongDonHangInput = Convert.ToInt32(thongTinDonHangArr[8]);

            //
            //lấy giỏ
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();

            if (taikhoanID != null)
            {
                var khachhang = _context.KhachHangs.AsNoTracking()
                    .SingleOrDefault(x => x.MaKh == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.MaKh;
                model.FullName = khachhang.TenKh;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Sdt;
                model.Address = khachhang.DiaChi;
                model.TinhThanh = khachhang.Matp;
                model.QuanHuyen = khachhang.Maqh;
                model.PhuongXa = khachhang.Maxa;

                if (khachhang.DiaChi == null) khachhang.DiaChi = Address;
                if (khachhang.Matp == null) khachhang.Matp = TinhThanh;
                if (khachhang.Maqh == null) khachhang.Maqh = QuanHuyen;
                if (khachhang.Maxa == null) khachhang.Maxa = PhuongXa;


                _context.Update(khachhang);
                _context.SaveChanges();
            }
            ViewBag.GioHang = cart;
            try
            {
                if (ModelState.IsValid)
                {
                    //khoi tao
                    DonHang donhang = new DonHang();

                    donhang.MaKh = model.CustomerId;
                    donhang.Sdt = model.Phone;
                    donhang.HoTen = model.FullName;
                    donhang.DiaChi = model.Address;
                    donhang.Matp = model.TinhThanh;
                    donhang.Maqh = model.QuanHuyen;
                    donhang.Maxa = model.PhuongXa;

                    donhang.NgayDat = DateTime.Now;
                    donhang.MaTt = 1;
                    donhang.PhuongThucThanhToan = "VNPAY";

                    donhang.TienShip = 50000;
                    donhang.GiamGiaShip = donhang.TienShip - phiGiaoHangInput;
                    donhang.GiamGia = soTienGiamInput;


                    donhang.TongTien = Convert.ToInt32(cart.Sum(x => x.TotalMoney)) + donhang.TienShip - donhang.GiamGiaShip - donhang.GiamGia;

                    _context.Add(donhang);
                    _context.SaveChanges();

                    //ds sp
                    foreach (var item in cart)
                    {
                        ChiTietDonHang ctdh = new ChiTietDonHang();
                        ctdh.MaDh = donhang.MaDh;
                        ctdh.MaSp = item.product.MaSp;
                        ctdh.GiaBan = item.product.GiaBan;
                        ctdh.GiaGiam = item.product.GiaGiam;
                        ctdh.SoLuong = item.amount;
                        ctdh.TongTien = item.TotalMoney;
                        //
                        SanPham hh = _context.SanPhams.SingleOrDefault(p => p.MaSp == item.product.MaSp);
                        hh.SoLuongCo -= 1;

                        _context.Add(ctdh);
                        _context.Update(hh);
                    }

                    _context.SaveChanges();

                    //

                    //clear
                    HttpContext.Session.Remove("GioHang");
                    _notyfService.Success("Đặt hàng thành công");

                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch
            {

                ViewBag.GioHang = cart;
                return RedirectToAction("Index", "Checkout");
            }

            ViewBag.GioHang = cart;
            return RedirectToAction("Index", "Checkout");
        }
    }
}
