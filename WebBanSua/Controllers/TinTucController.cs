using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanSua.Models;
using System.Linq;


namespace WebBanSua.Controllers
{
    public class TinTucController : Controller
    {
        private readonly CuaHangBanSuaContext _context;
        public TinTucController(CuaHangBanSuaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsTinTucPage = "TinTuc";
            return View(await _context.TinTucs.ToListAsync());
        }
        public async Task<IActionResult> Search(string searchInput)
        {
            var searchSP = from l in _context.TinTucs
                           select l;
            if (!string.IsNullOrWhiteSpace(searchInput))
            {
                ViewBag.SearchInput = searchInput;
                searchSP = searchSP.Where(s => s.TenTt.Contains(searchInput));
            }
            return View(await searchSP.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTucs
                .FirstOrDefaultAsync(m => m.MaTt == id);
            if (tinTuc == null)
            {
                return NotFound();
            }

            return View(tinTuc);
        }
    }
}
