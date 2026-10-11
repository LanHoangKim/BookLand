using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BookLand.Helpers;

// Hai cookie đăng nhập riêng: khách hàng (mặc định) và quản trị viên
public static class AuthSchemes
{
    public const string Customer = CookieAuthenticationDefaults.AuthenticationScheme;
    public const string Admin = "AdminAuth";
}

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;
}
