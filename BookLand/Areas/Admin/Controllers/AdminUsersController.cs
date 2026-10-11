using BookLand.Areas.Admin.Models;
using BookLand.Helpers;
using BookLand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Quản lý người dùng: chỉ lưu danh sách người quản trị
public class AdminUsersController : AdminBaseController
{
    private readonly BookLandContext _db;

    public AdminUsersController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.CurrentId = User.GetUserId();
        return View(await _db.AdminUsers.AsNoTracking().OrderBy(a => a.AdminId).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var admin = await _db.AdminUsers.AsNoTracking().FirstOrDefaultAsync(a => a.AdminId == id);
        return admin == null ? NotFound() : View(admin);
    }

    public IActionResult Create() => View(new AdminUserFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password)) ModelState.AddModelError(nameof(model.Password), "Vui lòng nhập mật khẩu");
        await ValidateAsync(model);
        if (!ModelState.IsValid) return View(model);

        _db.AdminUsers.Add(new AdminUser
        {
            Username = model.Username.Trim(),
            FullName = model.FullName.Trim(),
            Email = model.Email?.Trim(),
            Role = model.Role,
            PasswordHash = PasswordHelper.Hash(model.Password!),
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();
        Success($"Đã thêm tài khoản quản trị \"{model.Username}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var admin = await _db.AdminUsers.FindAsync(id);
        if (admin == null) return NotFound();
        return View(new AdminUserFormViewModel
        {
            AdminId = admin.AdminId,
            Username = admin.Username,
            FullName = admin.FullName,
            Email = admin.Email,
            Role = admin.Role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminUserFormViewModel model)
    {
        if (id != model.AdminId) return NotFound();
        var admin = await _db.AdminUsers.FindAsync(id);
        if (admin == null) return NotFound();

        await ValidateAsync(model);
        if (admin.Role == AdminRoles.Admin && model.Role != AdminRoles.Admin
            && await _db.AdminUsers.CountAsync(a => a.Role == AdminRoles.Admin) <= 1)
        {
            ModelState.AddModelError(nameof(model.Role), "Phải còn ít nhất một tài khoản Quản trị viên.");
        }
        if (!ModelState.IsValid) return View(model);

        admin.Username = model.Username.Trim();
        admin.FullName = model.FullName.Trim();
        admin.Email = model.Email?.Trim();
        admin.Role = model.Role;
        if (!string.IsNullOrWhiteSpace(model.Password)) admin.PasswordHash = PasswordHelper.Hash(model.Password);
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật tài khoản \"{admin.Username}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var admin = await _db.AdminUsers.AsNoTracking().FirstOrDefaultAsync(a => a.AdminId == id);
        if (admin == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(admin);
        return View(admin);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var admin = await _db.AdminUsers.FindAsync(id);
        if (admin == null) return NotFound();

        var blockers = await GetBlockersAsync(admin);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa tài khoản này.";
            return View(admin);
        }

        _db.AdminUsers.Remove(admin);
        await _db.SaveChangesAsync();
        Success($"Đã xóa tài khoản \"{admin.Username}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(AdminUser admin)
    {
        var blockers = new List<string>();
        if (admin.AdminId == User.GetUserId()) blockers.Add("Bạn không thể xóa tài khoản đang đăng nhập.");
        if (admin.Role == AdminRoles.Admin && await _db.AdminUsers.CountAsync(a => a.Role == AdminRoles.Admin) <= 1)
            blockers.Add("Đây là tài khoản Quản trị viên cuối cùng.");
        return blockers;
    }

    private async Task ValidateAsync(AdminUserFormViewModel model)
    {
        if (!AdminRoles.All.Contains(model.Role)) ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ");
        string username = (model.Username ?? "").Trim();
        if (await _db.AdminUsers.AnyAsync(a => a.Username == username && a.AdminId != model.AdminId))
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại.");
    }
}
