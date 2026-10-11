using System.Security.Claims;
using BookLand.Helpers;
using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Controllers;

public class AccountController : Controller
{
    private readonly BookLandContext _db;

    public AccountController(BookLandContext db)
    {
        _db = db;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string userName = model.UserName.Trim();
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == userName || c.Phone == userName);
        if (customer == null || !PasswordHelper.Verify(model.Password, customer.PasswordHash))
        {
            ModelState.AddModelError("", "Đăng nhập thất bại: số điện thoại/email hoặc mật khẩu không đúng.");
            return View(model);
        }

        await SignInCustomerAsync(customer, model.RememberMe);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        string email = model.Email.Trim().ToLowerInvariant();
        string phone = model.Phone.Trim();

        // Kiểm tra trùng với tài khoản đã có
        if (ModelState.IsValid)
        {
            if (await _db.Customers.AnyAsync(c => c.Email == email))
                ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký.");
            if (await _db.Customers.AnyAsync(c => c.Phone == phone))
                ModelState.AddModelError(nameof(model.Phone), "Số điện thoại này đã được đăng ký.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.RegisterFailed = true;
            return View(model);
        }

        _db.Customers.Add(new Customer
        {
            FullName = model.FullName.Trim(),
            Email = email,
            Phone = phone,
            PasswordHash = PasswordHelper.Hash(model.Password),
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục mua sắm.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Customer);
        return RedirectToAction("Index", "Home");
    }

    // Hồ sơ khách hàng
    [Authorize(AuthenticationSchemes = AuthSchemes.Customer)]
    public async Task<IActionResult> Profile()
    {
        var customer = await _db.Customers.FindAsync(User.GetUserId());
        if (customer == null) return await ForceLogoutAsync();

        ViewBag.Customer = customer;
        return View(new ProfileViewModel
        {
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Gender = customer.Gender,
            BirthDate = customer.BirthDate,
            Address = customer.Address
        });
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = AuthSchemes.Customer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var customer = await _db.Customers.FindAsync(User.GetUserId());
        if (customer == null) return await ForceLogoutAsync();

        string phone = model.Phone.Trim();
        if (ModelState.IsValid && await _db.Customers.AnyAsync(c => c.Phone == phone && c.CustomerId != customer.CustomerId))
        {
            ModelState.AddModelError(nameof(model.Phone), "Số điện thoại này đã được tài khoản khác sử dụng.");
        }
        if (model.BirthDate.HasValue && (model.BirthDate > DateOnly.FromDateTime(DateTime.Today) || model.BirthDate.Value.Year < 1900))
        {
            ModelState.AddModelError(nameof(model.BirthDate), "Ngày sinh không hợp lệ.");
        }

        ViewBag.Customer = customer;
        model.Email = customer.Email;
        if (!ModelState.IsValid) return View(model);

        customer.FullName = model.FullName.Trim();
        customer.Phone = phone;
        customer.Gender = model.Gender;
        customer.BirthDate = model.BirthDate;
        customer.Address = model.Address?.Trim();
        await _db.SaveChangesAsync();

        await SignInCustomerAsync(customer, true);   // cập nhật tên hiển thị trên header
        TempData["Success"] = "Đã cập nhật hồ sơ.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = AuthSchemes.Customer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var customer = await _db.Customers.FindAsync(User.GetUserId());
        if (customer == null) return await ForceLogoutAsync();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
        else if (!PasswordHelper.Verify(model.CurrentPassword, customer.PasswordHash))
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
        }
        else
        {
            customer.PasswordHash = PasswordHelper.Hash(model.NewPassword);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đổi mật khẩu thành công.";
        }
        return RedirectToAction(nameof(Profile));
    }

    // Lịch sử giao dịch: chỉ đơn hàng của khách hàng đang đăng nhập
    [Authorize(AuthenticationSchemes = AuthSchemes.Customer)]
    public async Task<IActionResult> Orders()
    {
        int customerId = User.GetUserId() ?? 0;
        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.OrderDetails)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        return View(orders);
    }

    [Authorize(AuthenticationSchemes = AuthSchemes.Customer)]
    public async Task<IActionResult> OrderDetail(int id)
    {
        int customerId = User.GetUserId() ?? 0;
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product).ThenInclude(p => p.ProductDetail)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customerId);
        if (order == null) return NotFound();
        return View(order);
    }

    private async Task SignInCustomerAsync(Customer customer, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
            new(ClaimTypes.Name, customer.FullName),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.MobilePhone, customer.Phone)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthSchemes.Customer));
        await HttpContext.SignInAsync(AuthSchemes.Customer, principal,
            new AuthenticationProperties { IsPersistent = persistent });
    }

    private async Task<IActionResult> ForceLogoutAsync()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Customer);
        return RedirectToAction(nameof(Login));
    }
}
