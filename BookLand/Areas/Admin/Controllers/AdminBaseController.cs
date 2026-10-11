using BookLand.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLand.Areas.Admin.Controllers;

// Mọi trang Admin đều yêu cầu đăng nhập bằng tài khoản quản trị (cookie riêng với khách hàng)
[Area("Admin")]
[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public abstract class AdminBaseController : Controller
{
    protected const int PageSize = 10;

    protected void Success(string message) => TempData["Success"] = message;

    protected void Error(string message) => TempData["Error"] = message;
}
