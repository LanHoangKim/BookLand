using System.Security.Claims;
using BookLand.Areas.Admin.Models;
using BookLand.Helpers;
using BookLand.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

public class AccountController : AdminBaseController
{
    private readonly BookLandContext _db;

    public AccountController(BookLandContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync(AuthSchemes.Admin);
        if (result.Succeeded) return RedirectToAction("Index", "Home");
        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == model.Username.Trim());
        if (admin == null || !PasswordHelper.Verify(model.Password, admin.PasswordHash))
        {
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
            new(ClaimTypes.Name, admin.FullName),
            new("Username", admin.Username),
            new(ClaimTypes.Role, admin.Role)
        };
        await HttpContext.SignInAsync(AuthSchemes.Admin,
            new ClaimsPrincipal(new ClaimsIdentity(claims, AuthSchemes.Admin)),
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Admin);
        return RedirectToAction(nameof(Login));
    }
}
