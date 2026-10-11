using BookLand.Areas.Admin.Models;
using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

public class AuthorsController : AdminBaseController
{
    private readonly BookLandContext _db;

    public AuthorsController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _db.Authors.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(a => a.AuthorName.Contains(search.Trim()));

        var items = query.OrderBy(a => a.AuthorName)
            .Select(a => new NamedCountItem<Author> { Item = a, ProductCount = a.Products.Count });
        ViewBag.Search = search;
        return View(await PagedList<NamedCountItem<Author>>.CreateAsync(items, page, PageSize));
    }

    public async Task<IActionResult> Details(int id)
    {
        var author = await _db.Authors.AsNoTracking().FirstOrDefaultAsync(a => a.AuthorId == id);
        if (author == null) return NotFound();
        ViewBag.Products = await _db.Products.AsNoTracking().Include(p => p.ProductDetail).Include(p => p.Category)
            .Where(p => p.AuthorId == id).OrderBy(p => p.ProductName).ToListAsync();
        return View(author);
    }

    public IActionResult Create() => View(new Author());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AuthorName,Nationality,BirthYear,Biography")] Author author)
    {
        await CheckDuplicateAsync(author);
        if (!ModelState.IsValid) return View(author);

        author.AuthorName = author.AuthorName.Trim();
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        Success($"Đã thêm tác giả \"{author.AuthorName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var author = await _db.Authors.FindAsync(id);
        return author == null ? NotFound() : View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AuthorId,AuthorName,Nationality,BirthYear,Biography")] Author author)
    {
        if (id != author.AuthorId) return NotFound();
        if (!await _db.Authors.AnyAsync(a => a.AuthorId == id)) return NotFound();

        await CheckDuplicateAsync(author);
        if (!ModelState.IsValid) return View(author);

        author.AuthorName = author.AuthorName.Trim();
        _db.Update(author);
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật tác giả \"{author.AuthorName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var author = await _db.Authors.AsNoTracking().FirstOrDefaultAsync(a => a.AuthorId == id);
        if (author == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(id);
        return View(author);
    }

    // Kiểm tra ràng buộc với bảng Product trước khi xóa
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var author = await _db.Authors.FindAsync(id);
        if (author == null) return NotFound();

        var blockers = await GetBlockersAsync(id);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa tác giả này vì còn sản phẩm liên quan.";
            return View(author);
        }

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
        Success($"Đã xóa tác giả \"{author.AuthorName}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(int id)
    {
        int count = await _db.Products.CountAsync(p => p.AuthorId == id);
        return count > 0 ? [$"Có {count} sản phẩm của tác giả này."] : [];
    }

    private async Task CheckDuplicateAsync(Author author)
    {
        string name = (author.AuthorName ?? "").Trim();
        if (await _db.Authors.AnyAsync(a => a.AuthorName == name && a.AuthorId != author.AuthorId))
            ModelState.AddModelError(nameof(author.AuthorName), "Tác giả này đã tồn tại.");
    }
}
