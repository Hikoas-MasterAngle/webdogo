﻿using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEBDOGO.Models;

namespace WEBDOGO.Controllers
{
    public class TrangChuController : Controller
    {
        SQLWEB2Entities1 db = new SQLWEB2Entities1();
        // GET: TrangChu
        public ActionResult Trangchu()
        {
            var danhsanpham1 = db.SANPHAMs.AsEnumerable();
            return View(danhsanpham1);
        }

        public ActionResult showsanpham(int? page)
        {
            string serverName1 = HttpContext.Application["ServerName"] as string;
            ViewBag.ServerName = serverName1;

            int size = 12;
            int ipagenumber = (page ?? 1);

            var danhsachsanpham = db.SANPHAMs.ToList();

            return View(danhsachsanpham.ToPagedList(ipagenumber, size));
        }

        public ActionResult chitietsanpham(int id)
        {
            
            var sanpham = db.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == id);
            if (sanpham != null) {
                var allsanpham = db.SANPHAMs.ToList();
                var viewmodel = new allclass_and_oneclass
                {
                    onesanpham = sanpham,
                    sanphamlist = allsanpham
                };

                string serverName = AppConfig.ServerName;
                ViewBag.ServerName = serverName;

                return View(viewmodel);
            }
            else
            {
                return RedirectToAction("Error404");
            }
           
        }

        public ActionResult timkiem()
        {
            return PartialView("_Partialtimkiem");
        }

        public ActionResult timkiemsanpham(int? page,SANPHAM spct)
        {
            string serverName1 = HttpContext.Application["ServerName"] as string;
            ViewBag.ServerName = serverName1;

            int size = 12;
            int ipagenumber = (page ?? 1);

            var danhsach = new List<SANPHAM>();
            if (spct.TENSANPHAM != null)
            {
                danhsach = db.SANPHAMs.Where(sp => sp.TENSANPHAM.Contains(spct.TENSANPHAM)).ToList();
            }
            else
            {
                if (spct.LOAISANPHAM == "ALL")
                {
                    danhsach = db.SANPHAMs.ToList();
                }
                else
                {
                    danhsach = db.SANPHAMs.Where(m => m.LOAISANPHAM == spct.LOAISANPHAM).ToList();
                }
            }
            return View(danhsach.ToPagedList(ipagenumber,size));
        }

        public ActionResult locthapdencao(int? page)
        {
            string serverName1 = HttpContext.Application["ServerName"] as string;
            ViewBag.ServerName = serverName1;

            int size = 12;
            int ipagenumber = (page ?? 1);

            var danhsach = db.SANPHAMs.ToList();

            danhsach = danhsach.OrderBy(p => Decimal.TryParse(p.GIASANPHAM, out decimal price) ? price : 0).ToList();

            return View(danhsach.ToPagedList(ipagenumber, size));
        }

        public ActionResult loccaodenthap(int? page)
        {
            string serverName1 = HttpContext.Application["ServerName"] as string;
            ViewBag.ServerName = serverName1;

            int size = 12;
            int ipagenumber = (page ?? 1);

            var danhSach = db.SANPHAMs
            .AsEnumerable()
            .OrderByDescending(sp =>
                decimal.TryParse(sp.GIASANPHAM, out decimal gia) ? gia : decimal.MinValue)
            .ToList();

            return View(danhSach.ToPagedList(ipagenumber, size));
        }

        [HttpPost]
        public ActionResult themsanphamvaogiohangnologin()
        {
            var soluong = int.Parse(Request.Form["soluong"]);
            var masp = int.Parse(Request.Form["masanpham"]);
            var sp = db.SANPHAMs.FirstOrDefault(m => m.MASANPHAM == masp);

            if (sp != null)
            {
                var listsanpham = Session["sanphamnologin"] as List<giohangsession>;
                if (listsanpham == null)
                {
                    listsanpham = new List<giohangsession>();
                }

                var existingProduct = listsanpham.FirstOrDefault(item => item.ProductId == sp.MASANPHAM);

                if (existingProduct != null)
                {
                    existingProduct.Quantity += soluong;
                    existingProduct.tongtien = existingProduct.Quantity*existingProduct.Price;
                }
                else
                {
                    listsanpham.Add(new giohangsession
                    {
                        ProductId = sp.MASANPHAM,
                        ProductName = sp.TENSANPHAM,
                        Quantity = soluong,
                        Price = double.Parse(sp.GIASANPHAM),
                        anhsanpham = sp.ANHSANPHAM,
                        tongtien = soluong* double.Parse(sp.GIASANPHAM)
                    });
                }

                Session["sanphamnologin"] = listsanpham;
            }

            return RedirectToAction("chitietsanpham", new { id = sp.MASANPHAM });
        }

        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}