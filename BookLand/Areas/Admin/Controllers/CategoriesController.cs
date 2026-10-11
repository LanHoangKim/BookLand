using BookLand.Areas.Admin.Models;
using BookLand.Helpers;
using BookLand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

public class CategoriesController : AdminBaseController
{
    private readonly BookLandContext _db;
    private readonly IWebHostEnvironment _env;

    public CategoriesController(BookLandContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Index: hiển thị tổng số hàng hóa thuộc danh mục (gồm cả danh mục con)
    public async Task<IActionResult> Index()
    {
        var all = await _db.Categories.AsNoTracking()
            .Include(c => c.Parent)
            .Select(c => new CategoryIndexItem
            {
                Category = c,
                ProductCount = c.Products.Count() + c.InverseParent.SelectMany(ch => ch.Products).Count()
            })
            .ToListAsync();

        // Sắp xếp: danh mục cha, ngay sau là các danh mục con
        var ordered = new List<CategoryIndexItem>();
        foreach (var parent in all.Where(x => x.Category.ParentId == null).OrderBy(x => x.Category.SortOrder))
        {
            ordered.Add(parent);
            ordered.AddRange(all.Where(x => x.Category.ParentId == parent.Category.CategoryId).OrderBy(x => x.Category.SortOrder));
        }
        return View(ordered);
    }

    // Details: thông tin danh mục + danh sách sản phẩm liên quan
    public async Task<IActionResult> Details(int id)
    {
        var category = await _db.Categories.AsNoTracking()
            .Include(c => c.Parent)
            .Include(c => c.InverseParent)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null) return NotFound();

        ViewBag.Products = await _db.Products.AsNoTracking()
            .Include(p => p.ProductDetail)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == id || p.Category.ParentId == id)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
        return View(category);
    }

    public async Task<IActionResult> Create()
    {
        await LoadParentsAsync(null);
        return View(new Category { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryName,ParentId,Description,SortOrder,IsActive")] Category category, IFormFile? imageFile)
    {
        await ValidateAsync(category, imageFile);
        if (!ModelState.IsValid)
        {
            await LoadParentsAsync(category.ParentId);
            return View(category);
        }

        if (imageFile != null) category.ImageUrl = await ImageHelper.SaveAsync(imageFile, _env, "categories");
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        Success($"Đã thêm danh mục \"{category.CategoryName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        await LoadParentsAsync(category.ParentId, id);
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,ParentId,Description,SortOrder,IsActive")] Category input, IFormFile? imageFile)
    {
        if (id != input.CategoryId) return NotFound();
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        await ValidateAsync(input, imageFile);
        if (input.ParentId == id) ModelState.AddModelError(nameof(input.ParentId), "Danh mục cha không thể là chính nó.");
        if (input.ParentId != null && await _db.Categories.AnyAsync(c => c.ParentId == id))
            ModelState.AddModelError(nameof(input.ParentId), "Danh mục đang có danh mục con nên không thể chuyển thành danh mục con.");

        if (!ModelState.IsValid)
        {
            input.ImageUrl = category.ImageUrl;
            await LoadParentsAsync(input.ParentId, id);
            return View(input);
        }

        category.CategoryName = input.CategoryName.Trim();
        category.ParentId = input.ParentId;
        category.Description = input.Description;
        category.SortOrder = input.SortOrder;
        category.IsActive = input.IsActive;
        if (imageFile != null)
        {
            ImageHelper.DeleteUploaded(category.ImageUrl, _env);
            category.ImageUrl = await ImageHelper.SaveAsync(imageFile, _env, "categories");
        }
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật danh mục \"{category.CategoryName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.AsNoTracking().Include(c => c.Parent).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(id);
        return View(category);
    }

    // Delete: kiểm tra ràng buộc với bảng Product và danh mục con trước khi xóa
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _db.Categories.Include(c => c.Parent).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null) return NotFound();

        var blockers = await GetBlockersAsync(id);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa danh mục này vì còn dữ liệu liên quan.";
            return View(category);
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        ImageHelper.DeleteUploaded(category.ImageUrl, _env);
        Success($"Đã xóa danh mục \"{category.CategoryName}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(int id)
    {
        var blockers = new List<string>();
        int products = await _db.Products.CountAsync(p => p.CategoryId == id);
        int children = await _db.Categories.CountAsync(c => c.ParentId == id);
        if (products > 0) blockers.Add($"Có {products} sản phẩm thuộc danh mục này.");
        if (children > 0) blockers.Add($"Có {children} danh mục con thuộc danh mục này.");
        return blockers;
    }

    private async Task ValidateAsync(Category category, IFormFile? imageFile)
    {
        if (imageFile != null && ImageHelper.Validate(imageFile) is string error)
            ModelState.AddModelError("imageFile", error);

        string name = (category.CategoryName ?? "").Trim();
        bool duplicate = await _db.Categories.AnyAsync(c => c.CategoryName == name
                                                            && c.ParentId == category.ParentId
                                                            && c.CategoryId != category.CategoryId);
        if (duplicate) ModelState.AddModelError(nameof(category.CategoryName), "Đã có danh mục cùng tên ở cấp này.");

        if (category.ParentId != null && !await _db.Categories.AnyAsync(c => c.CategoryId == category.ParentId && c.ParentId == null))
            ModelState.AddModelError(nameof(category.ParentId), "Danh mục cha không hợp lệ.");
    }

    private async Task LoadParentsAsync(int? selected, int? excludeId = null)
    {
        var parents = await _db.Categories.AsNoTracking()
            .Where(c => c.ParentId == null && c.CategoryId != excludeId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        ViewBag.ParentId = new SelectList(parents, nameof(Category.CategoryId), nameof(Category.CategoryName), selected);
    }
}
