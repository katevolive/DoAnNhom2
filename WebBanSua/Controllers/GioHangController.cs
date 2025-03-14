﻿using Microsoft.AspNetCore.Http;
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
        public static bool isDiscountApplied = false;
        public static decimal DiscountAmount = 0m;

        public GioHangController(CuaHangBanSuaContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var listGio = GioHang;
            ViewBag.IsDiscountApplied = isDiscountApplied;
            if (isDiscountApplied)
            {
                ViewBag.DiscountPerCartItem = DiscountAmount.ToString("#,##0");
            }
            else
            {
                ViewBag.DiscountPerCartItem = 0m.ToString("#,##0");
            }
            return View(GioHang);
        }
        private decimal CalculateDiscountPerCartItem(List<CartItem> gioHang)
        {
            decimal DiscountCart = gioHang.Sum(item => item.sanPham.GiaSp * item.soLuong);
            if (DiscountCart > 0)
            {
                decimal totalD = DiscountCart * 0.1m;
                return totalD;
            }

            return 0m;
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
        [Route("/giohang/apply-discount")]
        public IActionResult ApplyDiscount(string maGiamGia)
        {
            try
            {
                var gioHang = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (isDiscountApplied)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Bạn đã áp dụng mã giảm giá rồi."
                    });
                }
                if (gioHang != null && gioHang.Count > 0)
                {
                    if (maGiamGia == "GIAMGIA")
                    {
                        decimal totalDiscount100 = gioHang.Sum(item => item.sanPham.GiaSp * item.soLuong); // 10% giảm giá
                        decimal totalDiscount = totalDiscount100 * 0.1m;

                        DiscountAmount = totalDiscount;//test

                        int totalAmount = gioHang.Sum(item => item.sanPham.GiaSp * item.soLuong);
                        decimal discountPerCartItem = totalDiscount / totalAmount;
                        foreach (var item in gioHang)
                        {
                            item.sanPham.GiaSp -= (int)(item.sanPham.GiaSp * discountPerCartItem);
                        }
                        isDiscountApplied = true;
                        ViewBag.IsDiscountApplied= isDiscountApplied;

                        //ViewBag.DiscountPerCartItem = totalDiscount.ToString("#,##0") + " VNĐ";
                        HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                        return Json(new
                        {
                            success = true,
                            message = "Đã áp dụng giảm giá thành công."
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Mã giảm giá không hợp lệ hoặc bị sai."
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Giỏ hàng đang trống hoặc không tồn tại sản phẩm."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi. " + ex.Message
                });
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
                            decimal finalPrice = CalculateFinalPrice(sp);

                            item = new CartItem
                            {
                                soLuong = soLuong,
                                sanPham = sp,
                                finalPrice = finalPrice,
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
                ApplyDiscountIfNeeded(gioHang);
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi." +ex });
            }
        }
        private void ApplyDiscountIfNeeded(List<CartItem> gioHang)
        {
            if (isDiscountApplied)
            {
                decimal totalAmount = gioHang.Sum(item => item.sanPham.GiaSp * item.soLuong);
                if (totalAmount > 0)
                {
                    decimal totalDiscount = totalAmount * 0.1m; // 10% giảm giá
                    decimal discountPerCartItem = totalDiscount / totalAmount;

                    foreach (var item in gioHang)
                    {
                        item.sanPham.GiaSp -= (int)(item.sanPham.GiaSp * discountPerCartItem);
                    }
                    isDiscountApplied = false;
                }
            }
        }

        private decimal CalculateFinalPrice(SanPham sp)
        {
            decimal giaGiamGia = sp.BestSeller ? sp.GiaSp * 0.20m : 0;
            return giaGiamGia;
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
                            return Json(new
                            {
                                success = false,
                                message = "Giá trị không hợp lệ."
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
                    message = "Đã xảy ra lỗi. "+ex
                });
            }
        }

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
                    HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                }
                return Json(new { success = true,
                                  message = "Xóa thành công." });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
        public ActionResult CleanCart()
        {
            HttpContext.Session.Remove("GioHang");
            return RedirectToAction("Index");
        }
    }
}
