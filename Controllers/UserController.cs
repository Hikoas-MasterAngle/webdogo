﻿using Antlr.Runtime.Tree;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Net.Mail;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WEBDOGO.Models;
using System.Net;
using static WEBDOGO.Models.sepay;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using System.Text;

namespace WEBDOGO.Controllers
{
    public class UserController : Controller
    {
        SQLWEB2Entities1 db = new SQLWEB2Entities1();
        // GET: User
        public ActionResult dangnhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult dangnhap(string taikhoan, string matkhau)
        {
            string hashedPassword = HashPasswordWithSHA512(matkhau); // Mã hóa mật khẩu nhập vào

            var khachhang = db.KHACHHANGs.FirstOrDefault(u => u.TENDANGNHAP == taikhoan && u.MATKHAU == hashedPassword);
            var admin = db.ADMINs.FirstOrDefault(u => u.TENDANGNHAP == taikhoan && u.MATKHAU == hashedPassword);
            var taichinh = db.NHANVIENTAICHINHs.FirstOrDefault(u => u.TENDANGNHAP == taikhoan && u.MATKHAU == hashedPassword);
            var sanxuat = db.NHANVIENSANXUATs.FirstOrDefault(u => u.TENDANGNHAP == taikhoan && u.MATKHAU == hashedPassword);
            var vanchuyen = db.NHANVIENVANCHUYENs.FirstOrDefault(u => u.TENDANGNHAP == taikhoan && u.MATKHAU == hashedPassword);

            if (khachhang != null)
            {
                XulyGioHang(khachhang.MAKHACHHANG);
                Session["taikhoan"] = khachhang;
                TempData["dangnhapthanhcong"] = true;
                return RedirectToAction("Trangchu", "TrangChu");
            }

            if (admin != null)
            {
                Session["taikhoan"] = admin;
                TempData["dangnhapthanhcong"] = true;
                return RedirectToAction("showtrangquanly", "ADMIN");
            }

            if (taichinh != null)
            {
                Session["taikhoan"] = taichinh;
                TempData["dangnhapthanhcong"] = true;
                return RedirectToAction("showgiaodienquanly", "Nhanvientaichinh");
            }

            if (sanxuat != null)
            {
                Session["taikhoan"] = sanxuat;
                TempData["dangnhapthanhcong"] = true;
                return RedirectToAction("showyeucausanxuat", "Nhanviensanxuat");
            }

            if (vanchuyen != null)
            {
                Session["taikhoan"] = vanchuyen;
                TempData["dangnhapthanhcong"] = true;
                return RedirectToAction(vanchuyen.VAITRO == "Trưởng VC"
                    ? "showgiaodienquanly"
                    : "showvanchuyencuatoi", "Nhanvienvanchuyen");
            }

            TempData["dangnhapthanhcong"] = false;
            return View();
        }

        public static string HashPasswordWithSHA512(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha512.ComputeHash(bytes);

                // Chuyển hash thành chuỗi Hex
                StringBuilder result = new StringBuilder();
                foreach (byte b in hash)
                {
                    result.Append(b.ToString("x2"));
                }
                return result.ToString();
            }
        }

        private void XulyGioHang(int maKhachHang)
        {
            var listsanpham = Session["sanphamnologin"] as List<giohangsession>;
            if (listsanpham == null) return;

            foreach (var item in listsanpham)
            {
                var sanphamGioHang = db.GIOHANGs.FirstOrDefault(m => m.MASANPHAM == item.ProductId && m.MAKHACHHANG == maKhachHang);
                if (sanphamGioHang != null)
                {
                    sanphamGioHang.SOLUONG += item.Quantity;
                    sanphamGioHang.TONGTIEN = sanphamGioHang.SOLUONG * item.Price;
                    db.Entry(sanphamGioHang).State = EntityState.Modified;
                }
                else
                {
                    db.GIOHANGs.Add(new GIOHANG
                    {
                        MAKHACHHANG = maKhachHang,
                        MASANPHAM = item.ProductId,
                        SOLUONG = item.Quantity,
                        TONGTIEN = item.tongtien,
                        NGAYTHEM = DateTime.Now
                    });
                }
            }
            db.SaveChanges();
        }

        public ActionResult dangky()
        {
            return View();
        }

        [HttpPost]
        public ActionResult dangky(KHACHHANG kh, string matkhaunhaplai)
        {
            KHACHHANG khachang = db.KHACHHANGs.FirstOrDefault(u => u.EMAIL == kh.EMAIL || u.TENDANGNHAP == kh.TENDANGNHAP);
            
                if (khachang == null && kh.MATKHAU == matkhaunhaplai)
                {
                    try
                    {
                        kh.MATKHAU = HashPasswordWithSHA512(kh.MATKHAU);

                        db.KHACHHANGs.Add(kh);
                        db.SaveChanges();

                        TempData["dangkythanhcong"] = true;
                        return RedirectToAction("dangnhap");
                    }
                    catch (DbUpdateException ex)
                    {
                        ViewBag.thongbao = "Đã xảy ra lỗi khi lưu thông tin đăng ký. Vui lòng thử lại sau.";
                        return Content(ViewBag.thongbao);
                    }
                }
                else
                {
                    ViewBag.thongbao = "Email hoặc tài khoản đã được đăng ký trước đó";
                    return View();
                }
        }

        public ActionResult dangxuat()
        {
            Session["taikhoan"] = null;
            return RedirectToAction("Trangchu", "TrangChu");
        }

        public ActionResult showgiohang()
        {
            if (Session["taikhoan"] != null)
            {
                var khachhang = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                var danhsachgiohang = (from gh in db.GIOHANGs
                                       join sp in db.SANPHAMs on gh.MASANPHAM equals sp.MASANPHAM
                                       where gh.MAKHACHHANG == khachhang.MAKHACHHANG
                                       select new thanhtoan_giohang
                                       {
                                           gIOHANG = gh,
                                           sANPHAM = sp,
                                       }).ToList();
                
                double tongsotienbandau = 0;
                foreach (var item in danhsachgiohang)
                {
                    tongsotienbandau += (double)item.gIOHANG.TONGTIEN;
                }
                ViewBag.danhsachgiohang = danhsachgiohang;
                ViewBag.tongsotienbandau = tongsotienbandau;
                ViewBag.Severname = AppConfig.ServerName;
                return View();
            }
            else
            {
                var listsanphamnologin = Session["sanphamnologin"] as List<giohangsession>;
                ViewBag.listsanphamnologin = listsanphamnologin;
                double tongtien = 0;
                foreach(var item in listsanphamnologin)
                {
                    var tongtienmoiitem = item.Price * item.Quantity;
                    tongtien += tongtienmoiitem;
                }
                ViewBag.tongtien = tongtien;
                ViewBag.Severname = AppConfig.ServerName;
                return View();
            }
        }

        [HttpPost]
        public ActionResult themvaogiohang(int idkh, int idsanpham, int giasanpham, DateTime ngaythem, int soluong)
        {
            var kiemtragiohang = db.GIOHANGs.FirstOrDefault(g => g.MASANPHAM == idsanpham && g.MAKHACHHANG == idkh);
            if (kiemtragiohang == null)
            {
                var sp = db.SANPHAMs.FirstOrDefault(m => m.MASANPHAM == idsanpham);
                if (sp.SOLUONG == 0)
                {
                    return RedirectToAction("chitietsanpham", "TrangChu", new { id = idsanpham });
                }
                else
                {
                    int tongtien = giasanpham * soluong;
                    WEBDOGO.Models.GIOHANG giohang = new WEBDOGO.Models.GIOHANG();
                    giohang.MASANPHAM = idsanpham;
                    giohang.MAKHACHHANG = idkh;
                    giohang.TONGTIEN = tongtien;
                    giohang.SOLUONG = soluong;
                    giohang.NGAYTHEM = ngaythem;
                    try
                    {
                        TempData["thongbaothemthanhcong"] = "Thêm sản phẩm vào giỏ hàng thành công";

                        db.GIOHANGs.Add(giohang);
                        db.SaveChanges();
                        return RedirectToAction("chitietsanpham", "TrangChu", new { id = idsanpham });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.thongbao = "Đã xảy ra lỗi khi lưu thông tin đăng ký. Vui lòng thử lại sau.";
                        return Content(ViewBag.thongbao);
                    }
                }
            }
            else
            {
                kiemtragiohang.SOLUONG = kiemtragiohang.SOLUONG + soluong;
                kiemtragiohang.TONGTIEN = kiemtragiohang.SOLUONG * giasanpham;
                try
                {
                    TempData["thongbaothemthanhcong"] = "Thêm sản phẩm vào giỏ hàng thành công";

                    db.Entry(kiemtragiohang).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("chitietsanpham", "TrangChu", new { id = idsanpham });
                }
                catch (Exception ex)
                {
                    ViewBag.thongbao = "Đã xảy ra lỗi khi lưu thông tin đăng ký. Vui lòng thử lại sau.";
                    return Content(ViewBag.thongbao);
                }
            }
        }

        public ActionResult xoakhoigiohang(int idgiohang)
        {
            if (Session["taikhoan"] != null)
            {
                var kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                var sanphamcanxoa = db.GIOHANGs.FirstOrDefault(g => g.MAGIOHANG == idgiohang);
                if (sanphamcanxoa != null)
                {
                    db.GIOHANGs.Remove(sanphamcanxoa);
                    db.SaveChanges();
                    return RedirectToAction("showgiohang", "User");
                }
                else
                {
                    return RedirectToAction("showgiohang", "User");
                }
            }
            else
            {
                var listsanpham = Session["sanphamnologin"] as List<giohangsession>;
                var sanpham = listsanpham.FirstOrDefault(sp => sp.ProductId == idgiohang);
                listsanpham.Remove(sanpham);
                Session["sanphamnologin"] = listsanpham;
                return RedirectToAction("showgiohang", "User");
            }
        }


        public  ActionResult capnhapthongtinnguoidung()
        {
            if (Session["taikhoan"] != null)
            {
                var kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                return View(kh);
            }
            else
            {
                return RedirectToAction("Trangchu","TrangChu");
            }
        }

        [HttpPost]
        public ActionResult capnhapthongtinnguoidung(KHACHHANG khvao)
        {
            // Tìm kiếm khách hàng theo tên đăng nhập
            KHACHHANG tkkh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
            var existingKhachHang = db.KHACHHANGs.FirstOrDefault(kh => kh.MAKHACHHANG == tkkh.MAKHACHHANG);

            if (existingKhachHang != null)
            {
                // Cập nhật thông tin khách hàng
                existingKhachHang.HOVATEN = khvao.HOVATEN;
                existingKhachHang.TUOI = khvao.TUOI;
                existingKhachHang.TINH = khvao.TINH;
                existingKhachHang.DIACHICHITIET = khvao.DIACHICHITIET;
                existingKhachHang.EMAIL = khvao.EMAIL;
                existingKhachHang.SDT = khvao.SDT;

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
                Session["taikhoan"] = existingKhachHang;

                return RedirectToAction("capnhapthongtinnguoidung" , "User");
            }
            else
            {
                ViewBag.thongbao = "Người dùng không tồn tại";
                return Content(ViewBag.thongbao);
            }
        }

        public ActionResult lienhe()
        {
            return View();
        }

        [HttpPost]
        public ActionResult lienhe(LIENHE lh)
        {
            if(lh != null)
            {
                var kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                lh.MAKHACHHANG = kh.MAKHACHHANG;
                db.LIENHEs.Add(lh);
                db.SaveChanges();
                return RedirectToAction("lienhe", "User");
            }
            else
            {
                ViewBag.thongbao = "Không thể lưu";
                return Content(ViewBag.thongbao);
            }
        }

        public ActionResult showsanphamtheoyeucau()
        {
            if (Session["taikhoan"] != null)
            {

                var khachhang = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                var sanphamtheoyeucau = db.SANPHAMTHEOYEUCAUs.Where(m => m.MAKHACHHANG == khachhang.MAKHACHHANG);

                var listsanphamdadat = from sp in db.SANPHAMTHEOYEUCAUs
                                       join vc in db.VANCHUYENs on sp.MASANPHAMTHEOYEUCAU equals vc.MASANPHAMTHEOYEUCAU into vcGroup
                                       from vc in vcGroup.DefaultIfEmpty()
                                       join td in db.TIENDOSANXUATs on sp.MASANPHAMTHEOYEUCAU equals td.MASANPHAMTHEOYEUCAU into tdGroup
                                       from td in tdGroup.DefaultIfEmpty()
                                       where sp.MAKHACHHANG == khachhang.MAKHACHHANG
                                       select new SanPhamTheoYeuCauKhachHangViewModel
                                       {
                                           SanPham = sp,
                                           VANCHUYENs = vc,
                                           TIENDOSANXUATs = td,
                                       };
                ViewBag.danhsachsanphamdadat = listsanphamdadat.ToList();
                return View();
            }
            else
            {
                return RedirectToAction("dangnhap");
            }
        }

        public ActionResult chitietvanchuyen(int mavc)
        {
            if(mavc != null)
            {
                var vc = db.VANCHUYENs.FirstOrDefault(m => m.MAVANCHUYEN == mavc);

                var khachhang = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                var sanphamtheoyeucau = db.SANPHAMTHEOYEUCAUs.Where(m => m.MAKHACHHANG == khachhang.MAKHACHHANG);

                var listsanphamdadat = from sp in db.SANPHAMTHEOYEUCAUs
                                       join vc1 in db.VANCHUYENs on sp.MASANPHAMTHEOYEUCAU equals vc1.MASANPHAMTHEOYEUCAU into vcGroup
                                       from vc1 in vcGroup.DefaultIfEmpty()
                                       join td in db.TIENDOSANXUATs on sp.MASANPHAMTHEOYEUCAU equals td.MASANPHAMTHEOYEUCAU into tdGroup
                                       from td in tdGroup.DefaultIfEmpty()
                                       where sp.MAKHACHHANG == khachhang.MAKHACHHANG
                                       select new SanPhamTheoYeuCauKhachHangViewModel
                                       {
                                           SanPham = sp,
                                           VANCHUYENs = vc1,
                                           TIENDOSANXUATs = td,
                                       };
                ViewBag.danhsachsanphamdadat = listsanphamdadat.ToList();
                var nhanvienvanchuyen = db.NHANVIENVANCHUYENs.FirstOrDefault(m => m.MANHANVIENVANCHUYEN == vc.MANHANVIENVANCHUYEN);
                ViewBag.vanchuyen = vc;
                ViewBag.nhanvienvanchuyen = nhanvienvanchuyen.TENNHANVIENVANCHUYEN;
                return PartialView("_Partialchitietvanchuyen_Khachhang");
            }
            else
            {
                return Content("Không tìm thấy vận chuyển");
            }
        }

        public ActionResult chitiettiendosanxuat(int matd)
        {
            if(matd != null)
            {
                var td = db.TIENDOSANXUATs.FirstOrDefault(m => m.MATIENDOSANXUAT == matd);
                var khachhang = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                var sanphamtheoyeucau = db.SANPHAMTHEOYEUCAUs.Where(m => m.MAKHACHHANG == khachhang.MAKHACHHANG);

                var listsanphamdadat = from sp in db.SANPHAMTHEOYEUCAUs
                                       join vc in db.VANCHUYENs on sp.MASANPHAMTHEOYEUCAU equals vc.MASANPHAMTHEOYEUCAU into vcGroup
                                       from vc in vcGroup.DefaultIfEmpty()
                                       join td1 in db.TIENDOSANXUATs on sp.MASANPHAMTHEOYEUCAU equals td1.MASANPHAMTHEOYEUCAU into tdGroup
                                       from td1 in tdGroup.DefaultIfEmpty()
                                       where sp.MAKHACHHANG == khachhang.MAKHACHHANG
                                       select new SanPhamTheoYeuCauKhachHangViewModel
                                       {
                                           SanPham = sp,
                                           VANCHUYENs = vc,
                                           TIENDOSANXUATs = td1,
                                       };
                ViewBag.danhsachsanphamdadat = listsanphamdadat.ToList();
                ViewBag.tiendosanxuat = td;
                var nhanviensanxuat = db.NHANVIENSANXUATs.FirstOrDefault(m => m.MANHANVIENSANXUAT == td.MANHANVIENSANXUAT);

                if(nhanviensanxuat != null )
                {
                    ViewBag.tennhanviensanxuat = nhanviensanxuat.TENNHANVIENSANXUAT;
                }
                else
                {
                    ViewBag.tennhanviensanxuat = "Chưa có";
                }
                return PartialView("_Partialchitiettiendosanxuat_khachhang");
            }
            else
            {
                return Content("Không tìm thấy vận chuyển");
            }
        }

        [HttpPost]
        public ActionResult themsanphamtheoyeucau(SANPHAMTHEOYEUCAU sptyc, HttpPostedFileBase HinhAnh)
        {
            if (sptyc != null)
            {
                var khachhang = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                sptyc.MAKHACHHANG = khachhang.MAKHACHHANG;
                sptyc.TRANGTHAIDUYET = "Chưa duyệt";
                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var path = Path.Combine(Server.MapPath("~/IMG/sanpham"), fileName);
                    HinhAnh.SaveAs(path); // Lưu ảnh vào thư mục

                    sptyc.ANHMINHHOA = "../IMG/sanpham/" + fileName;
                }
                sptyc.KHACHHANGCHAPNHAN = "Chưa có";
                db.SANPHAMTHEOYEUCAUs.Add(sptyc);
                db.SaveChanges();
                return RedirectToAction("showsanphamtheoyeucau");
            }
            else
            {
                return Content("Không lấy được dữ liệu của sản phẩm theo yêu cầu");
            }
        }

        public ActionResult thanhtoan()
        {
            if (Session["taikhoan"] !=  null)
            {
                KHACHHANG kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
                var danhsachgiohang = from gh in db.GIOHANGs
                                      join kh1 in db.KHACHHANGs on gh.MAKHACHHANG equals kh1.MAKHACHHANG
                                      join sp in db.SANPHAMs on gh.MASANPHAM equals sp.MASANPHAM
                                      where gh.MAKHACHHANG == kh.MAKHACHHANG
                                      select new thanhtoan_giohang
                                      {
                                          sANPHAM = sp,
                                          gIOHANG = gh,
                                      };

                var danhsachsanpham = danhsachgiohang.ToList();
                ViewBag.dssp = danhsachsanpham;
                return View();
            }
            else
            {
                return RedirectToAction("dangnhap");
            }
        }

        public string TaoSoHoaDon()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string soHoaDon;

            do
            {
                soHoaDon = new string(Enumerable.Repeat(chars, 5)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (db.HOADONs.Any(h => h.SOHOADON == soHoaDon));

            return soHoaDon;
        }

        public ActionResult dathang(double tongsotien,string paymentMethod)
        {

            KHACHHANG kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];

            HOADON newhd = new HOADON();
            newhd.MAKHACHHANG = kh.MAKHACHHANG;
            newhd.TONGTIEN = tongsotien;
            newhd.TRANGTHAITHANHTOAN = "Chưa thanh toán";
            if(paymentMethod == "Thanhtoanonline")
            {
                newhd.PHUONGTHUCTHANHTOAN = "Thanh toán online";
            }
            else
            {
                newhd.PHUONGTHUCTHANHTOAN = "Thanh toán khi nhận hàng";
            }
            newhd.SOHOADON = TaoSoHoaDon();
            newhd.NGAYLAP = DateTime.Now;

            db.HOADONs.Add(newhd);
            db.SaveChanges();


            var danhsachgiohang = from gh in db.GIOHANGs
                                  join kh1 in db.KHACHHANGs on gh.MAKHACHHANG equals kh1.MAKHACHHANG
                                  join sp in db.SANPHAMs on gh.MASANPHAM equals sp.MASANPHAM
                                  where gh.MAKHACHHANG == kh.MAKHACHHANG
                                  select new thanhtoan_giohang
                                  {
                                      sANPHAM = sp,
                                      gIOHANG = gh,
                                  };

            var danhsachsanpham = danhsachgiohang.ToList();

            foreach(var item in danhsachsanpham)
            {
                CHITIETHOADON newcthd = new CHITIETHOADON();

                newcthd.MAHOADON = newhd.MAHOADON;
                newcthd.MASANPHAM = item.gIOHANG.MASANPHAM;
                newcthd.TONGTIEN = item.gIOHANG.TONGTIEN;
                newcthd.SOLUONG = item.gIOHANG.SOLUONG;

                var spphaitru = db.SANPHAMs.FirstOrDefault(s => s.MASANPHAM == item.sANPHAM.MASANPHAM);
                spphaitru.SOLUONG-=item.gIOHANG.SOLUONG;
                if(spphaitru.DABAN == null )
                {
                    spphaitru.DABAN = 0;
                    spphaitru.DABAN += item.gIOHANG.SOLUONG;
                }
                else
                {
                    spphaitru.DABAN += item.gIOHANG.SOLUONG;
                }

                db.Entry(spphaitru).State = EntityState.Modified;
                db.CHITIETHOADONs.Add(newcthd);
                db.SaveChanges();
            }

            var danhsachgh = (from gh in db.GIOHANGs
                                  where gh.MAKHACHHANG == kh.MAKHACHHANG
                                  select gh).ToList();
            foreach(var item in danhsachgh)
            {
                db.GIOHANGs.Remove(item);
                db.SaveChanges();
            }

            var danhsachchitietsanphamcuahoadon = (from cthd in db.CHITIETHOADONs
                                                   join sp in db.SANPHAMs on cthd.MASANPHAM equals sp.MASANPHAM
                                                   where cthd.MAHOADON == newhd.MAHOADON
                                                   select new chitiethoadon_sanpham
                                                   {
                                                       sCHITIETHOADON = cthd,
                                                       sSANPHAM = sp,
                                                   }).ToList();

            

            ViewBag.danhsachcthdcuahd = danhsachchitietsanphamcuahoadon;
            ViewBag.sohoadon = newhd;

            if(paymentMethod == "Thanhtoanonline")
            {
                return View();
            }
            else
            {
                TempData["Hoadon"] = newhd;
                return RedirectToAction("dathangoffline");
            }
        }

        public ActionResult replacethanhtoan(int mahd)
        {
            ViewBag.sohoadon = db.HOADONs.FirstOrDefault(m => m.MAHOADON == mahd);
            ViewBag.sanphamtheoyeucau = (from hd in db.HOADONs
                                         join sptyc in db.SANPHAMTHEOYEUCAUs on hd.MAHOADON equals sptyc.MAHOADON
                                         where hd.MAHOADON == mahd
                                         select new chitiethoadon_sanpham
                                         {
                                             hOADON = hd,
                                             sANPHAMTHEOYEUCAU = sptyc,
                                         }).ToList();
            

            if(ViewBag.sanphamtheoyeucau != null)
            {
                return View();
            }
            else
            {
                ViewBag.chitethoadon = (from hd in db.HOADONs
                                        join cthd in db.CHITIETHOADONs on hd.MAHOADON equals cthd.MAHOADON
                                        join sp in db.SANPHAMs on cthd.MASANPHAM equals sp.MASANPHAM
                                        where hd.MAHOADON == mahd
                                        select new chitiethoadon_sanpham
                                        {
                                            hOADON = hd,
                                            sCHITIETHOADON = cthd,
                                            sSANPHAM = sp,
                                        }).ToList();
                return View();
            }
        }

        public ActionResult dathangoffline()
        {
            HOADON hoadon = TempData["Hoadon"] as HOADON;

            var kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];

            var dsgiohang = (from hd in db.HOADONs
                             join cthd in db.CHITIETHOADONs on hd.MAHOADON equals cthd.MAHOADON
                             join sp in db.SANPHAMs on cthd.MASANPHAM equals sp.MASANPHAM
                             where hd.SOHOADON == hoadon.SOHOADON
                             select new chitiethoadon_sanpham
                             {
                                 sCHITIETHOADON = cthd,
                                 sSANPHAM = sp,
                             }).ToList();

            foreach(var item in dsgiohang)
            {
                VANCHUYEN newvc = new VANCHUYEN();
                newvc.NGAYBATDAUGUI = DateTime.Now.AddDays(1);
                newvc.PHUONGTHUCVANCHUYEN = "Vận chuyển nhanh";
                newvc.CHIPHIVANCHUYEN = 0;
                newvc.MOTA = "Đơn hàng đã được tạo hãy vận chuyển đơn hàng đến khách hàng sớm nhất có thể";
                newvc.TRANGTHAIVANCHUYEN = "Đơn hàng đã tạo";
                newvc.MACHITIETHOADON = item.sCHITIETHOADON.MACHITIETHOADON;
                db.VANCHUYENs.Add(newvc);
            }
            db.SaveChanges();
            guimai(dsgiohang, hoadon, kh);
            return View();
        }

        public void guimai(List<chitiethoadon_sanpham> dsgiohang, HOADON hoadon,KHACHHANG kh )
        {
            string username = "2224802010208@student.tdmu.edu.vn";
            string password = "dbmd wckm qknh zazz";

            try
            {
                // Nội dung email với CSS đẹp mắt
                string body = $@"
<div style='font-family:Arial, sans-serif; max-width:600px; margin:auto; border:1px solid #ddd; border-radius:8px; overflow:hidden; box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);'>
    <div style='background-color:#4CAF50; padding:25px; text-align:center; color:white;'>
        <h2 style='font-size:28px; margin-bottom:10px;'>Xác nhận đơn hàng</h2>
        <p style='font-size:16px; margin:0;'>Cảm ơn bạn đã đặt hàng tại nhà sách online!</p>
    </div>
    <div style='padding:30px; background-color:#fafafa;'>
        <h3 style='font-size:22px; color:#333; margin-bottom:15px;'>Chi tiết đơn hàng</h3>
        <table style='width:100%; border-collapse:collapse;'>
            <thead>
                <tr style='background-color:#FFB591; color:#555;'>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Mã Sản Phẩm</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Tên Sản Phẩm</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Số Lượng</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Đơn Giá</th>
                    <th style='padding:12px; border-bottom:2px solid #ddd;'>Thành Tiền</th>
                </tr>
            </thead>
            <tbody>";

                foreach (var item1 in dsgiohang)
                {
                    if(item1.sCHITIETHOADON != null)
                    {
                        body += $@"
                <tr style='background-color:#fff; border-bottom:1px solid #eee;'>
                    <td style='padding:10px; text-align:center; color:#333;'>{item1.sCHITIETHOADON.MASANPHAM}</td>
                    <td style='padding:10px; color:#333;'>{item1.sSANPHAM.TENSANPHAM}</td>
                    <td style='padding:10px; text-align:center; color:#333;'>{item1.sCHITIETHOADON.SOLUONG}</td>
                    <td style='padding:10px; text-align:right; color:#333;'>{item1.sSANPHAM.GIASANPHAM:#,##0} VNĐ</td>
                    <td style='padding:10px; text-align:right; color:#333;'>{item1.sCHITIETHOADON.TONGTIEN:#,##0} VNĐ</td>
                </tr>";
                    }
                    else
                    {
                        body += $@"
                <tr style='background-color:#fff; border-bottom:1px solid #eee;'>
                    <td style='padding:10px; text-align:center; color:#333;'>{item1.sANPHAMTHEOYEUCAU.MASANPHAMTHEOYEUCAU}</td>
                    <td style='padding:10px; color:#333;'>{item1.sANPHAMTHEOYEUCAU.LOAISANPHAM}</td>
                    <td style='padding:10px; text-align:center; color:#333;'>{item1.sANPHAMTHEOYEUCAU.SOLUONG}</td>
                    <td style='padding:10px; text-align:right; color:#333;'>{item1.sANPHAMTHEOYEUCAU.TONGSOTIENSANXUAT:#,##0} VNĐ</td>
                    <td style='padding:10px; text-align:right; color:#333;'>{item1.sANPHAMTHEOYEUCAU.TONGSOTIENSANXUAT:#,##0} VNĐ</td>
                </tr>";
                    }
                    
                }

                body += $@"
            </tbody>
        </table>
        <div style='text-align:right; margin-top:20px; font-size:16px; color:#333;'>
            <p><b>Tổng tiền:</b> {hoadon.TONGTIEN:#,##0} VNĐ</p>
        </div>
</div>";
                // Cấu hình email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(username);
                mail.To.Add(kh.EMAIL);
                mail.Subject = "Xác Nhận Đơn Hàng";
                mail.Body = body;
                mail.IsBodyHtml = true;
                // Cấu hình SMTP
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = true;

                // Gửi email
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gửi email thất bại: " + ex.Message);
            }
        }

        public ActionResult hoadoncuauser()
        {
            if (Session["taikhoan"] != null)
            {
                KHACHHANG kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];

                var danhsachgh = db.HOADONs.Where(s => s.MAKHACHHANG == kh.MAKHACHHANG).ToList();
                ViewBag.danhsachhd = danhsachgh;

                return View();
            }
            else
            {
                return RedirectToAction("dangnhap");
            }
        }

        public ActionResult chitiethoadoncuauser(int mahd)
        {
            HOADON hd1 = db.HOADONs.FirstOrDefault(m => m.MAHOADON == mahd);
            if (hd1 == null)
            {
                return HttpNotFound("Hóa đơn không tồn tại.");
            }

            var chitiethoadonSanphamList = (from hd in db.HOADONs
                                            join cthd1 in db.CHITIETHOADONs on hd.MAHOADON equals cthd1.MAHOADON into cthd1group
                                            from cthd1 in cthd1group.DefaultIfEmpty()
                                            join sp in db.SANPHAMs on cthd1.MASANPHAM equals sp.MASANPHAM into spgroup
                                            from sp in spgroup.DefaultIfEmpty()
                                            join vc in db.VANCHUYENs on cthd1.MACHITIETHOADON equals vc.MACHITIETHOADON into vcgroup
                                            from vc in vcgroup.DefaultIfEmpty()
                                            where hd.MAHOADON == mahd
                                            select new chitiethoadon_sanpham
                                            {
                                                sCHITIETHOADON = cthd1,
                                                sSANPHAM = sp,
                                                vANCHUYEN = vc,
                                            }).ToList();

            if (chitiethoadonSanphamList.ElementAt(0).sCHITIETHOADON != null)
            {
                ViewBag.hoadoncuauser = hd1;
                ViewBag.chitiethoadoncantim = chitiethoadonSanphamList;
                return View();
            }
            else
            {
                var chitietsanphamtheoyeucauList = (from hd in db.HOADONs
                                                    join sptyc in db.SANPHAMTHEOYEUCAUs on hd.MAHOADON equals sptyc.MAHOADON into sptycGroup
                                                    from sptyc in sptycGroup.DefaultIfEmpty()
                                                    join vc in db.VANCHUYENs on sptyc.MASANPHAMTHEOYEUCAU equals vc.MASANPHAMTHEOYEUCAU into vcGroup
                                                    from vc in vcGroup.DefaultIfEmpty()
                                                    where hd.MAHOADON == mahd
                                                    select new chitiethoadon_sanpham
                                                    {
                                                        sANPHAMTHEOYEUCAU = sptyc,
                                                        hOADON = hd,
                                                        vANCHUYEN = vc,
                                                    }).ToList();

                ViewBag.hoadoncuauser = hd1;
                ViewBag.chitietsanphamtheoyeucau = chitietsanphamtheoyeucauList;
                return View();
            }

        }

        public ActionResult xemchitietvanchuyencuachitiethoadonuser(int mahd, int mavc)
        {

            HOADON hd1 = db.HOADONs.FirstOrDefault(m => m.MAHOADON == mahd);
            ViewBag.hoadoncuauser = hd1;

            if (hd1 == null)
            {
                return HttpNotFound("Hóa đơn không tồn tại.");
            }

            var cthd = from hd in db.HOADONs
                       join cthd1 in db.CHITIETHOADONs on hd.MAHOADON equals cthd1.MAHOADON
                       join sp in db.SANPHAMs on cthd1.MASANPHAM equals sp.MASANPHAM
                       join vc in db.VANCHUYENs on cthd1.MACHITIETHOADON equals vc.MACHITIETHOADON
                       join nvvc in db.NHANVIENVANCHUYENs on vc.MANHANVIENVANCHUYEN equals nvvc.MANHANVIENVANCHUYEN
                       where hd.MAHOADON == mahd
                       select new chitiethoadon_sanpham
                       {
                           sCHITIETHOADON = cthd1,
                           sSANPHAM = sp,
                           vANCHUYEN = vc,
                           hANVIENVANCHUYEN = nvvc,
                       };

            ViewBag.chitietsanphamtheoyeucau = (from hd in db.HOADONs
                                                join sptyc in db.SANPHAMTHEOYEUCAUs on hd.MAHOADON equals sptyc.MAHOADON
                                                join vc in db.VANCHUYENs on sptyc.MASANPHAMTHEOYEUCAU equals vc.MASANPHAMTHEOYEUCAU
                                                where hd.MAHOADON == mahd
                                                select new chitiethoadon_sanpham
                                                {
                                                    sANPHAMTHEOYEUCAU = sptyc,
                                                    hOADON = hd,
                                                    vANCHUYEN = vc,
                                                }).ToList();

            ViewBag.chitiethoadoncantim = cthd.ToList();

            ViewBag.vanchuyen = db.VANCHUYENs.FirstOrDefault(m => m.MAVANCHUYEN == mavc);

            ViewBag.nvvc = (from vc in db.VANCHUYENs
                           join nvvc in db.NHANVIENVANCHUYENs on vc.MANHANVIENVANCHUYEN equals nvvc.MANHANVIENVANCHUYEN
                           where vc.MAVANCHUYEN == mavc
                           select new vanchuyen_nvvanchuyen
                           {
                               nHANVIENVANCHUYEN = nvvc,
                               vANCHUYEN = vc,
                           }).FirstOrDefault();


            return View();
        }



        public ActionResult chapnhandonhang(int masp)
        {
            var sptyc = db.SANPHAMTHEOYEUCAUs.FirstOrDefault(m => m.MASANPHAMTHEOYEUCAU == masp);
            sptyc.KHACHHANGCHAPNHAN = "Chấp nhận";
            var kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];
            HOADON newhd = new HOADON()
            {
                MAKHACHHANG = kh.MAKHACHHANG,
                TONGTIEN = sptyc.TONGSOTIENSANXUAT,
                TRANGTHAITHANHTOAN = "Chưa thanh toán",
                PHUONGTHUCTHANHTOAN = "Thanh toán online",
                MANHANVIENTAICHINH = 1,
                NGAYLAP = DateTime.Now,
                SOHOADON = TaoSoHoaDon(),
            };
            db.HOADONs.Add(newhd);
            db.SaveChanges();

            sptyc.MAHOADON = newhd.MAHOADON;
            db.Entry(sptyc).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.hoadon = newhd;
            ViewBag.sptyc1 = sptyc;
            return View();
        }

        public ActionResult khongchapnhandonhang(int masp)
        {
            var sptyc = db.SANPHAMTHEOYEUCAUs.FirstOrDefault(m => m.MASANPHAMTHEOYEUCAU == masp);
            sptyc.KHACHHANGCHAPNHAN = "Không chấp nhận";
            sptyc.TRANGTHAIDUYET = "Hủy đơn hàng";
            db.Entry(sptyc).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("showsanphamtheoyeucau");
        }

        private readonly sepay _model = new sepay();

        public async Task<ActionResult> sepay1(int mahd)
        {
            var data = await _model.FetchTransactionsAsync();
            var kh = (WEBDOGO.Models.KHACHHANG)Session["taikhoan"];

            if (data.error != null)
            {
                ViewBag.Error = data.error;
            }
            else
            {
                var danhsachgiaodich = data.transactions;
                var hoadon = db.HOADONs.FirstOrDefault(m => m.MAHOADON == mahd);

                foreach(var item in danhsachgiaodich)
                {
                    double hoadonTongTienValue = hoadon.TONGTIEN ?? 0.0;
                    string transactionContentFirst5 = item.transaction_content.ToString().Substring(0, 5);
                    if (double.TryParse(item.amount_in.ToString(), out double amountInValue))
                    {
                        if (hoadon.SOHOADON == transactionContentFirst5 && Math.Abs(hoadonTongTienValue - amountInValue) < 0.001)
                        {
                            hoadon.TRANGTHAITHANHTOAN = "đã thanh toán";
                            db.Entry(hoadon).State = EntityState.Modified;
                            db.SaveChanges();

                            var sptyc = db.SANPHAMTHEOYEUCAUs.FirstOrDefault(m => m.MAHOADON == mahd);
                            if(sptyc != null)
                            {
                                var daylasptyc = (from hd in db.HOADONs
                                                  join sptyc1 in db.SANPHAMTHEOYEUCAUs on hd.MAHOADON equals sptyc1.MAHOADON
                                                  where hd.MAHOADON == mahd
                                                  select new chitiethoadon_sanpham
                                                  {
                                                      sANPHAMTHEOYEUCAU = sptyc1
                                                  }).ToList();

                                foreach(var i in daylasptyc)
                                {
                                    DOANHTHU dt = new DOANHTHU
                                    {
                                        SOLUONG = i.sANPHAMTHEOYEUCAU.SOLUONG,
                                        GIADABAN = i.sANPHAMTHEOYEUCAU.TONGSOTIENSANXUAT,
                                        NGAYTAO = DateTime.Now,
                                        MASANPHAMTHEOYEUCAU = i.sANPHAMTHEOYEUCAU.MASANPHAMTHEOYEUCAU,
                                    };
                                    db.DOANHTHUs.Add(dt);

                                    TIENDOSANXUAT td1 = new TIENDOSANXUAT
                                    {
                                       GIAIDOANSANXUAT = "Lập kế hoạch và thiết kế",
                                       NGAYSANXUAT = DateTime.Now,
                                       MANHANVIENSANXUAT = 1,
                                        MOTA = "Đơn hàng đã được thanh toán hãy làm việc năng nỗ nào !!!",
                                        SOLUONG = i.sANPHAMTHEOYEUCAU.SOLUONG,
                                        MASANPHAMTHEOYEUCAU = i.sANPHAMTHEOYEUCAU.MASANPHAMTHEOYEUCAU,
                                        NGHIEMTHU = "Phân công",
                                    };
                                    db.TIENDOSANXUATs.Add(td1);
                                    db.SaveChanges();
                                }
                                guimai(daylasptyc, hoadon, kh);
                                ViewBag.thongbao = 0;
                                return View();
                            }
                            else
                            {
                                var dsgiohang = (from hd in db.HOADONs
                                                 join cthd in db.CHITIETHOADONs on hd.MAHOADON equals cthd.MAHOADON
                                                 join sp in db.SANPHAMs on cthd.MASANPHAM equals sp.MASANPHAM
                                                 where hd.MAHOADON == mahd
                                                 select new chitiethoadon_sanpham
                                                 {
                                                     sCHITIETHOADON = cthd,
                                                     sSANPHAM = sp,
                                                 }).ToList();

                                foreach (var i in dsgiohang)
                                {
                                    DOANHTHU dt = new DOANHTHU
                                    {
                                        SOLUONG = i.sCHITIETHOADON.SOLUONG,
                                        GIADABAN = i.sCHITIETHOADON.TONGTIEN,
                                        NGAYTAO = DateTime.Now,
                                        MACHITIETHOADON = i.sCHITIETHOADON.MACHITIETHOADON,
                                    };
                                    db.DOANHTHUs.Add(dt);
                                    db.SaveChanges();

                                    db.VANCHUYENs.Add(new VANCHUYEN
                                    {
                                        NGAYBATDAUGUI = DateTime.Now.AddDays(1),
                                        NGAYDUKIENDUOCGIAO = DateTime.Now.AddDays(5),
                                        PHUONGTHUCVANCHUYEN = "Vận chuyển nhanh",
                                        MOTA = "Đơn hàng đã được thanh toán, hãy làm việc năng nỗ nào!",
                                        CHIPHIVANCHUYEN = 0,
                                        MACHITIETHOADON = i.sCHITIETHOADON.MACHITIETHOADON,
                                        TRANGTHAIVANCHUYEN = "Đơn hàng đã tạo",
                                        MANHANVIENVANCHUYEN = 1
                                    });
                                    db.SaveChanges();
                                }
                                guimai(dsgiohang, hoadon, kh);
                                ViewBag.thongbao = 0;
                                return View();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Lỗi: Không thể chuyển đổi amount_in thành kiểu double.");
                    }
                }
            }
            ViewBag.mahd = mahd;
            ViewBag.thongbao = 1;
            return View();
        }

        public ActionResult quenmatkhau()
        {
            return View();
        }

        public string GenerateRandomString(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void guimk(string mk, string email)
        {
            var kh = db.KHACHHANGs.FirstOrDefault(m => m.EMAIL == email);
            string username = "2224802010208@student.tdmu.edu.vn";
            string password = "dbmd wckm qknh zazz";

            try
            {
                string body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Thông báo đặt lại mật khẩu</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; color: #333; margin: 0; padding: 0; }}
        .email-container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; padding-bottom: 20px; border-bottom: 1px solid #dddddd; }}
        .header h1 {{ font-size: 24px; color: #333; }}
        .content {{ margin-top: 20px; }}
        .content p {{ font-size: 16px; line-height: 1.5; }}
        .new-password {{ font-size: 18px; font-weight: bold; color: #d9534f; }}
        .footer {{ margin-top: 30px; text-align: center; font-size: 12px; color: #888888; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>Thông báo đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <p>Chào "+ kh.HOVATEN + " </p> "+
            "<p>Chúng tôi đã tạo một mật khẩu mới cho tài khoản của bạn theo yêu cầu. Vui lòng sử dụng mật khẩu dưới đây để đăng nhập và thay đổi thành mật khẩu dễ nhớ hơn nếu bạn muốn.</p>"+
            "<p>Mật khẩu mới của bạn là:</p>"+
            "<p class='new-password'>"+mk+"</p>"+
            "<p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ với bộ phận hỗ trợ của chúng tôi ngay lập tức.</p>"+
            "<p>Trân trọng,<br>Đội ngũ Hỗ trợ của Công ty</p>" +
        "</div>" +
   "</div>"+
"</body>"+
"</html>";

                // Cấu hình email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(username);
                mail.To.Add(email);
                mail.Subject = "Thông báo đặt lại mật khẩu";
                mail.Body = body;
                mail.IsBodyHtml = true;
                // Cấu hình SMTP
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = true;

                // Gửi email
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gửi email thất bại: " + ex.Message);
            }
        }

        [HttpPost]
        public ActionResult quenmatkhau(KHACHHANG kh)
        {
            var khtim = db.KHACHHANGs.FirstOrDefault(m => m.EMAIL == kh.EMAIL);
            if(khtim != null)
            {
                var newmk = GenerateRandomString();
                khtim.MATKHAU = newmk;
                db.Entry(khtim).State = EntityState.Modified;
                db.SaveChanges();
                guimk(newmk, kh.EMAIL);
                ViewBag.thongbao = "Đã gửi mật khẩu mới vào email của bạn !!!";
            }
            else
            {
                ViewBag.thongbao = "Đã gửi mật khẩu mới vào email của bạn !!!";
            }
            return RedirectToAction("quenmatkhau");
        }

        public ActionResult huydonhang(int id)
        {
            bool canCancel = false;
            var hd = db.HOADONs.FirstOrDefault(m => m.MAHOADON == id);

            if (hd != null)
            {
                if (hd.PHUONGTHUCTHANHTOAN.Equals("Thanh toán khi nhận hàng", StringComparison.OrdinalIgnoreCase))
                {
                    // Lấy danh sách chi tiết hóa đơn
                    var cthd = hd.CHITIETHOADONs.ToList();
                    if (cthd != null)
                    {
                        canCancel = true;
                        foreach (var item in cthd)
                        {
                            var vc = item.VANCHUYENs.FirstOrDefault();
                            if (vc != null &&
                                (vc.TRANGTHAIVANCHUYEN == "Đơn hàng đã tạo" || vc.TRANGTHAIVANCHUYEN == "Đang chuẩn bị"))
                            {
                                db.VANCHUYENs.Remove(vc);
                                db.CHITIETHOADONs.Remove(item);
                            }
                            else
                            {
                                canCancel = false;
                                break;
                            }
                        }

                        if (canCancel && !hd.CHITIETHOADONs.Any())
                        {
                            db.HOADONs.Remove(hd);
                            db.SaveChanges();
                        }
                    }
                }
                else if (hd.PHUONGTHUCTHANHTOAN.Equals("Thanh toán online", StringComparison.OrdinalIgnoreCase))
                {
                    var cthd = hd.CHITIETHOADONs.ToList();
                    if (cthd != null && cthd.Any())
                    {
                        foreach (var item in cthd)
                        {
                            var vc = db.VANCHUYENs.FirstOrDefault(n => n.MACHITIETHOADON == item.MACHITIETHOADON);
                            var dt = db.DOANHTHUs.FirstOrDefault(n => n.MACHITIETHOADON == item.MACHITIETHOADON);
                            var sp = db.SANPHAMs.FirstOrDefault(n => n.MASANPHAM == item.MASANPHAM);

                            if(vc == null && dt == null)
                            {
                                sp.SOLUONG += item.SOLUONG;
                                db.Entry(sp).State = EntityState.Modified;
                                db.CHITIETHOADONs.Remove(item);
                                canCancel = true;

                            }
                            else if (vc != null && (vc.TRANGTHAIVANCHUYEN == "Đơn hàng đã tạo" || vc.TRANGTHAIVANCHUYEN == "Đang chuẩn bị"))
                            {
                                db.DOANHTHUs.Remove(dt);
                                db.VANCHUYENs.Remove(vc);
                                sp.SOLUONG += item.SOLUONG;
                                db.Entry(sp).State = EntityState.Modified;
                                db.CHITIETHOADONs.Remove(item);
                                canCancel = true;
                            }
                            else
                            {
                                canCancel = false;
                                break;
                            }
                        }

                        if (canCancel)
                        {
                            db.HOADONs.Remove(hd);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var sptyc = db.SANPHAMTHEOYEUCAUs.FirstOrDefault(m => m.MAHOADON == id);
                        if (sptyc != null)
                        {
                            var dt = db.DOANHTHUs.FirstOrDefault(m => m.MASANPHAMTHEOYEUCAU == sptyc.MASANPHAMTHEOYEUCAU);
                            var tdsx = db.TIENDOSANXUATs.FirstOrDefault(m => m.MASANPHAMTHEOYEUCAU == sptyc.MASANPHAMTHEOYEUCAU);

                            if (tdsx != null && tdsx.GIAIDOANSANXUAT == "Lập kế hoạch và thiết kế")
                            {
                                db.TIENDOSANXUATs.Remove(tdsx);
                                db.DOANHTHUs.Remove(dt);
                                db.SANPHAMTHEOYEUCAUs.Remove(sptyc);
                                db.HOADONs.Remove(hd);
                                db.SaveChanges();
                                canCancel = true;
                            }
                            else
                            {
                                canCancel = false;
                            }
                        }
                    }
                }
            }

            // Cập nhật trạng thái hủy đơn cho ViewBag
            ViewBag.huydon = canCancel;

            // Trả về View
            return View();
        }

        public ActionResult gioithieu()
        {
            return View();
        }

        public ActionResult nutthemsoluongsanphamnologin(int id, int loai)
        {
            if(loai == 0)
            {
                var listsanpham = Session["sanphamnologin"] as List<giohangsession>;
                var sanpham = listsanpham.FirstOrDefault(sp => sp.ProductId == id);
                var spcantim = db.SANPHAMs.SingleOrDefault(m => m.MASANPHAM == id);
                if(spcantim.SOLUONG > sanpham.Quantity)
                {
                    sanpham.Quantity++;
                    sanpham.tongtien = sanpham.Quantity*sanpham.Price;
                }
                Session["sanphamnologin"] = listsanpham;
                return RedirectToAction("showgiohang", "User");
            }
            else
            {
                var listsanpham = Session["sanphamnologin"] as List<giohangsession>;
                var sanpham = listsanpham.FirstOrDefault(sp => sp.ProductId == id);

                if(sanpham.Quantity > 1)
                {
                    sanpham.Quantity--;
                    sanpham.tongtien =  sanpham.Price*sanpham.Quantity;
                }
                Session["sanphamnologin"] = listsanpham;
                return RedirectToAction("showgiohang", "User");
            }
        }

        public ActionResult nutthemsoluongsanphamlogin(int id, int loai)
        {
            if(loai == 0)
            {
                var giohang = db.GIOHANGs.SingleOrDefault(m => m.MAGIOHANG == id);
                var sp = db.SANPHAMs.SingleOrDefault(m => m.MASANPHAM == giohang.MASANPHAM);
                if(sp.SOLUONG > giohang.SOLUONG)
                {
                    giohang.SOLUONG++;
                    giohang.TONGTIEN = giohang.SOLUONG * double.Parse(sp.GIASANPHAM);
                }
                db.Entry(giohang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("showgiohang", "User");
            }
            else
            {
                var giohang = db.GIOHANGs.SingleOrDefault(m => m.MAGIOHANG == id);
                var sp = db.SANPHAMs.SingleOrDefault(m => m.MASANPHAM == giohang.MASANPHAM);
                if (giohang.SOLUONG > 1)
                {
                    giohang.SOLUONG--;
                    giohang.TONGTIEN = giohang.SOLUONG * double.Parse(sp.GIASANPHAM);
                }
                db.Entry(giohang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("showgiohang", "User");
            }
        }
    }
}