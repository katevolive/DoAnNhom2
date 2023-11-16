using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebBanSua.Extension;
using WebBanSua.Models;
using WebBanSua.ModelViews;

namespace WebBanSua.Controllers
{
    public class GioHangController : Controller
    {
        private readonly CuaHangBanSuaContext _context;

        public GioHangController(CuaHangBanSuaContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var listGio = GioHang;
            return View(GioHang);
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

        [HttpPost]
        [Route("/giohang/add-cart")]
        public IActionResult AddToCart(int maSP, int soLuong)
        {
            try
            {
                List<CartItem> gioHang = HttpContext.Session.Get<List<CartItem>>("GioHang") ?? new List<CartItem>();

                // Kiểm tra sp có trong giỏ hàng k
                CartItem item = gioHang.SingleOrDefault(p => p.sanPham.MaSp == maSP);

                if (item != null)
                {
                    int newQuantity = item.soLuong + soLuong;

                    if (newQuantity <= item.sanPham.SoLuong)
                    {
                        item.soLuong = newQuantity;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Số lượng vượt quá tồn kho." });
                    }
                }
                else
                {
                    // Nếu sp chưa có trong giỏ hàng, thêm mới
                    SanPham sp = _context.SanPhams.SingleOrDefault(p => p.MaSp == maSP);

                    if (sp != null)
                    {
                        if (soLuong <= sp.SoLuong)
                        {
                            item = new CartItem
                            {
                                soLuong = soLuong,
                                sanPham = sp
                            };
                            gioHang.Add(item);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Số lượng vượt quá tồn kho." });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                    }
                }
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi." });
            }
            //List<CartItem> gioHang = GioHang;
            //try
            //{
            //    //Thêm vào giỏ
            //    CartItem item = gioHang.SingleOrDefault(p => p.sanPham.MaSp == maSP);
            //    if (item != null)
            //    {
            //        item.soLuong = item.soLuong + soLuong;

            //        HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
            //    }
            //    else
            //    {
            //        SanPham sp = _context.SanPhams.SingleOrDefault(p => p.MaSp == maSP);
            //        item = new CartItem
            //        {
            //            soLuong = soLuong,
            //            sanPham = sp
            //        };
            //        gioHang.Add(item);
            //    }
            //    HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);

            //    return Json(new { succeess = true });

            //}
            //catch (Exception ex)
            //{
            //    return Json(new { succeess = false });
            //}

        }
        [HttpPost]
        [Route("/giohang/update-cart")]
        public IActionResult UpdateCart(int maSP, int soLuong)
        {
            try
            {
                var gioHang = HttpContext.Session.Get<List<CartItem>>("GioHang");

                if (gioHang != null)
                {
                    CartItem item = gioHang.SingleOrDefault(p => p.sanPham.MaSp == maSP);

                    if (item != null)
                    {
                        if (soLuong > 0)
                        {
                            if (soLuong <= item.sanPham.SoLuong)
                            {
                                item.soLuong = soLuong;
                                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                                return Json(new
                                {
                                    success = true,
                                    soLuong = gioHang.Sum(p => p.soLuong)
                                });
                            }
                            else
                            {
                                return Json(new
                                {
                                    success = false,
                                    message = "Số lượng vượt quá tồn kho < " + item.sanPham.SoLuong
                                });
                            }
                        }
                        else
                        {
                            gioHang.Remove(item);
                            HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);

                            return Json(new
                            {
                                success = true,
                                soLuong = gioHang.Sum(p => p.soLuong)
                            });
                        }
                    }
                }

                return Json(new
                {
                    success = false,
                    message = "Sản phẩm không tồn tại trong giỏ hàng."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi."
                });
            }
        }

        //[HttpPost]
        //[Route("/giohang/update-cart")]
        //public IActionResult UpdateCart(int maSP, int soLuong)
        //{
        //   var gioHang = HttpContext.Session.Get<List<CartItem>>("GioHang");
        //    try
        //    {

        //       if(gioHang != null)
        //        {
        //            CartItem item = gioHang.SingleOrDefault(p => p.sanPham.MaSp == maSP);
        //            if (item != null)
        //            {
        //                item.soLuong = soLuong;
        //            }
        //            HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
        //        }
        //        return Json(new
        //        {
        //            soLuong = GioHang.Sum(p => p.soLuong)
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { succeess = false });
        //    }

        //}

        [HttpPost]
        [Route("/giohang/remove")]
        public ActionResult Remove(int maSP)
        {
            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.sanPham.MaSp == maSP);
                if (item != null)
                {
                    gioHang.Remove(item);
                }
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                return Json(new { succeess = true });
            }
            catch
            {
                return Json(new { succeess = false });
            }

        }

        public ActionResult CleanCart()
        {
            HttpContext.Session.Remove("GioHang");
            return RedirectToAction("Index");

        }

       
    }
}
