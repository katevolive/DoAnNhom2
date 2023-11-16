using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanSua.Models;

namespace WebBanSua.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly CuaHangBanSuaContext _context;

        public SanPhamController(CuaHangBanSuaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsTinTucPage = "SanPham"; 
            var cuaHangBanSuaContext = _context.SanPhams.Include(s => s.MaDmNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }
        [Route("/SanPham/Search")]
        public async Task<IActionResult> Search(string searchInput)
        {
            ViewBag.IsTinTucPage = "SanPham";
            var searchSP = from l in _context.SanPhams.Include(s => s.MaDmNavigation)
                           select l;
            if (!string.IsNullOrWhiteSpace(searchInput))
            {
                ViewBag.SearchInput = searchInput;
                searchSP = searchSP.Where(s => s.TenSp.Contains(searchInput));
            }
            return View(await searchSP.ToListAsync());
        }
            //if (!string.IsNullOrWhiteSpace(searchInput))
            //{
            //    ViewBag.SearchInput = searchInput;
            //    var searchSanPham = _context.SanPhams
            //                .Where(result => result.TenSp.Contains(searchInput))
            //                .Include(s => s.MaDmNavigation);
            //    return View(await searchSanPham.ToListAsync());
            //}
            //return View();
        public async Task<IActionResult> Search2(string id)
        {
            var searchSanPham = _context.SanPhams.Include(s => s.MaDmNavigation);
            var searchSP = from l in _context.SanPhams.Include(s => s.MaDmNavigation)
                           select l;
            if (!string.IsNullOrWhiteSpace(id))
            {
                ViewBag.SearchInput = id;
                searchSP = searchSP.Where(s => s.TenSp.Contains(id));
            }
                return View(await searchSP.ToListAsync());
        }

           public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

       public async Task<IActionResult> SuaTT()
        {
            var cuaHangBanSuaContext = _context.SanPhams.Include(s => s.MaDmNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }

        public async Task<IActionResult> SuaChua()
        {
            var cuaHangBanSuaContext = _context.SanPhams.Include(s => s.MaDmNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }

        public async Task<IActionResult> BoPhomat()
        {
            var cuaHangBanSuaContext = _context.SanPhams.Include(s => s.MaDmNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }

        public async Task<IActionResult> Intro()
        {
            return View();
        }


        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSp == id);
        }
    }
}
