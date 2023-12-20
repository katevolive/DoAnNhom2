using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanSua.Models;
using WebBanSua.ModelViews;

namespace WebBanSua.Controllers
{
    public class DonHangsController : Controller
    {
        private readonly CuaHangBanSuaContext _context;

        public DonHangsController(CuaHangBanSuaContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.ShowAdminDiv = HomeController.roleId;
            var cuaHangBanSuaContext = _context.DonHangs.Include(d => d.MaKhNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }
        public async Task<IActionResult> TongDon()
        {
            var cuaHangBanSuaContext = _context.DonHangs.Include(d => d.MaKhNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);
            if (donHang == null)
            {
                return NotFound();
            }
            ViewBag.DiscountAmount = GioHangController.DiscountAmount;
            ViewBag.IsDiscountApplied = GioHangController.isDiscountApplied;

            return View(donHang);
        }
        public IActionResult Create()
        {
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDh,MaKh,NgayTao,TrangThaiHuyDon,ThanhToan,NgayThanhToan,Note")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "Email", donHang.MaKh);
            return View(donHang);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "Email", donHang.MaKh);
            return View(donHang);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDh,MaKh,NgayTao,TrangThaiHuyDon,ThanhToan,NgayThanhToan,Note")] DonHang donHang)
        {
            if (id != donHang.MaDh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "Email", donHang.MaKh);
            return View(donHang);
        }
        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.MaDh == id);
        }
        [HttpPost]
        public async Task<IActionResult> HuyDonHang(int id, [FromBody] ConfirmationModel model)
        {
            var donHang = await _context.DonHangs.FindAsync(id);

            if (donHang == null)
            {
                return NotFound();
            }

            if (!donHang.TrangThaiHuyDon && model != null && model.Confirm)
            {
                try
                {
                    donHang.TrangThaiHuyDon = true;
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Đã xảy ra lỗi khi hủy đơn hàng: {ex.Message}" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Đơn hàng đã được hủy trước đó hoặc không xác nhận." });
            }
        }

    }
}
