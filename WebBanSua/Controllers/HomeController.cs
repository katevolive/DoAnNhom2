using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebBanSua.Models;
namespace WebBanSua.Controllers
{
    public class HomeController : Controller
    {
      
        private readonly CuaHangBanSuaContext _context;

        public HomeController(CuaHangBanSuaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsTinTucPage = "SanPham";
            var cuaHangBanSuaContext = _context.SanPhams.Include(s => s.MaDmNavigation);
            return View(await cuaHangBanSuaContext.ToListAsync());
        }

        public async Task<IActionResult> ChangePassword(int id)
        {
            var maKH = HttpContext.Session.GetString("MaKh");
            id = int.Parse(maKH);
            var customerUser = await _context.KhachHangs.FindAsync(id);

            return View(customerUser);
        }

        [HttpPost]
        public IActionResult ChangePassword(KhachHang customer,string inputPasswordNew)
        {
            if (string.IsNullOrEmpty(inputPasswordNew))
            {
                ModelState.AddModelError("", "Mật khẩu không được để trống");
                return View(customer);
            }
            if (customer.Password == inputPasswordNew)
            {
                ModelState.AddModelError("", "Mật khẩu mới không được trùng với mật khẩu cũ");
                return View(customer);
            }

            if (ModelState.IsValid)
            {
                var check = _context.SaveChanges();
                if (check > 0)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu");
                    return View(customer);
                }
            }
            return View(customer);

        }

        public async Task<IActionResult> UserDashboard(int id)
        {
            var maKH = HttpContext.Session.GetString("MaKh");
            id = int.Parse(maKH);
            var customerUser = await _context.KhachHangs.FindAsync(id);
            return View(customerUser);
        }

        [HttpPost]
        public  IActionResult UserDashboard(KhachHang customer, Account user)
        {
            if (ModelState.IsValid)
            {
                    _context.Update(customer);
                    var check = _context.SaveChanges();
                    if (check > 0)
                    {
                        return RedirectToAction("UserDashboard");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Lỗi lưu dữ liệu");
                        return View(customer);
                    }
                }
            return View(customer);
        }
       
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("MaKh,TenKh,GioiTinh,AvatarKh,Diachi,Ngaysinh,Phone,Email,Password,CreateDate")] KhachHang khachhang, Account user)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(khachhang.TenKh) == true || string.IsNullOrEmpty(khachhang.GioiTinh) == true|| string.IsNullOrEmpty(khachhang.Email) == true || khachhang.Phone == null || khachhang.Ngaysinh == null)
                {
                    ModelState.AddModelError("TenKh", "Thông tin không được để trống");
                    return View(khachhang);
                }
                if (string.IsNullOrEmpty(khachhang.TenKh) == true || (khachhang.Ngaysinh) == null)
                {
                    ModelState.AddModelError("Ngaysinh", "Thông tin không được để trống");
                    return View(khachhang);
                }
                var checkEmail = _context.KhachHangs.SingleOrDefault(x => x.Email.Trim().ToLower() == khachhang.Email.Trim().ToLower());
                if (checkEmail != null )
                {
                    ModelState.AddModelError("Email", "Địa chỉ Email đã tồn tại");
                    return View(khachhang);
                }
                var checkPhone = _context.KhachHangs.SingleOrDefault(x => x.Phone == khachhang.Phone);
                if (!IsPhoneNumberValid(checkPhone.Phone))
                {
                    ModelState.AddModelError("1", "Số điện thoại phải chứa đúng 9 số.");
                    return View(khachhang);
                }
                if (checkPhone != null)
                {
                    ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại");
                    return View(khachhang);
                }
                    user.TaiKhoan = khachhang.Email;
                    user.RoleId = 3;
                    user.CreateDate = DateTime.Now;
                    khachhang.CreateDate = DateTime.Now;
                    HttpContext.Session.SetString("TenKh", khachhang.TenKh.ToString());
                    HttpContext.Session.SetString("Email", khachhang.Email.Trim().ToLower());
                    _context.Add(khachhang);
                    _context.Add(user);
                   await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
            }
            return View();
        }
        private bool IsPhoneNumberValid(int phoneNumber)
        {
            int count = 0;
            int tempNumber = phoneNumber;
            while (tempNumber > 0)
            {
                tempNumber /= 10;
                count++;
            }
            return count == 9;
        }
        
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var khachhang = _context.KhachHangs.SingleOrDefault(x => x.Email.Trim().ToLower() == email.Trim().ToLower() && x.Password == password);
                if (khachhang!=null)
                {
                    HttpContext.Session.SetString("MaKh",khachhang.MaKh.ToString());
                    HttpContext.Session.SetString("TenKh", khachhang.TenKh.ToString());
                    HttpContext.Session.SetString("Email", khachhang.Email.Trim().ToLower());
                    var checkAcount = _context.Accounts.SingleOrDefault(x => x.TaiKhoan.Trim().ToLower() ==email.Trim().ToLower());
                    if (checkAcount != null)
                    {
                        HttpContext.Session.SetString("AccountID", checkAcount.AccountId.ToString());
                        HttpContext.Session.SetInt32("RoleID", checkAcount.RoleId);
                        var roleID = HttpContext.Session.GetInt32("RoleID");
                        var checkRole = _context.RoleAccounts.SingleOrDefault(x => x.RoleId == roleID);
                        if (checkRole != null)
                        {
                            if(checkRole.RoleId == 1 || checkRole.RoleId == 2)
                            {
                                return RedirectToAction("Index", "Home", new { area = "Admin" });
                            } else return RedirectToAction("Index");
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("1", "Đăng Nhập Thất Bại!"); 
                    ModelState.AddModelError("1", "Kiểm Lại Thông Tin Đăng Nhập!");
                    return View();
                }
            }
            return View();
        }
        //Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();//remove session
            return RedirectToAction("Index");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
