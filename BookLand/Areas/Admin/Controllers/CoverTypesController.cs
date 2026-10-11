using BookLand.Areas.Admin.Models;
using BookLand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Bảng CoverType chỉ có 2 cột (< 5 cột) nên không làm chức năng Details
public class CoverTypesController : AdminBaseController
{
    private readonly BookLandContext _db;

    public CoverTypesController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.CoverTypes.AsNoTracking()
            .OrderBy(c => c.CoverTypeId)
            .Select(c => new NamedCountItem<CoverType> { Item = c, ProductCount = c.ProductDetails.Count })
            .ToListAsync();
        return View(items);
    }

    public IActionResult Create() => View(new CoverType());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CoverTypeName")] CoverType coverType)
    {
        await CheckDuplicateAsync(coverType);
        if (!ModelState.IsValid) return View(coverType);

        coverType.CoverTypeName = coverType.CoverTypeName.Trim();
        _db.CoverTypes.Add(coverType);
        await _db.SaveChangesAsync();
        Success($"Đã thêm hình thức bìa \"{coverType.CoverTypeName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var coverType = await _db.CoverTypes.FindAsync(id);
        return coverType == null ? NotFound() : View(coverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CoverTypeId,CoverTypeName")] CoverType coverType)
    {
        if (id != coverType.CoverTypeId || !await _db.CoverTypes.AnyAsync(c => c.CoverTypeId == id)) return NotFound();

        await CheckDuplicateAsync(coverType);
        if (!ModelState.IsValid) return View(coverType);

        coverType.CoverTypeName = coverType.CoverTypeName.Trim();
        _db.Update(coverType);
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật hình thức bìa \"{coverType.CoverTypeName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var coverType = await _db.CoverTypes.AsNoTracking().FirstOrDefaultAsync(c => c.CoverTypeId == id);
        if (coverType == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(id);
        return View(coverType);
    }

    // Kiểm tra ràng buộc với bảng ProductDetail trước khi xóa
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var coverType = await _db.CoverTypes.FindAsync(id);
        if (coverType == null) return NotFound();

        var blockers = await GetBlockersAsync(id);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa hình thức bìa đang được sử dụng.";
            return View(coverType);
        }

        _db.CoverTypes.Remove(coverType);
        await _db.SaveChangesAsync();
        Success($"Đã xóa hình thức bìa \"{coverType.CoverTypeName}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(int id)
    {
        int count = await _db.ProductDetails.CountAsync(d => d.CoverTypeId == id);
        return count > 0 ? [$"Có {count} sản phẩm đang dùng hình thức bìa này."] : [];
    }

    private async Task CheckDuplicateAsync(CoverType coverType)
    {
        string name = (coverType.CoverTypeName ?? "").Trim();
        if (await _db.CoverTypes.AnyAsync(c => c.CoverTypeName == name && c.CoverTypeId != coverType.CoverTypeId))
            ModelState.AddModelError(nameof(coverType.CoverTypeName), "Hình thức bìa này đã tồn tại.");
    }
}
