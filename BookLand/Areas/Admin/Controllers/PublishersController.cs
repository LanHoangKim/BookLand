using BookLand.Areas.Admin.Models;
using BookLand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

public class PublishersController : AdminBaseController
{
    private readonly BookLandContext _db;

    public PublishersController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.Publishers.AsNoTracking()
            .OrderBy(p => p.PublisherName)
            .Select(p => new NamedCountItem<Publisher> { Item = p, ProductCount = p.Products.Count })
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var publisher = await _db.Publishers.AsNoTracking().FirstOrDefaultAsync(p => p.PublisherId == id);
        if (publisher == null) return NotFound();
        ViewBag.Products = await _db.Products.AsNoTracking().Include(p => p.ProductDetail).Include(p => p.Category)
            .Where(p => p.PublisherId == id).OrderBy(p => p.ProductName).ToListAsync();
        return View(publisher);
    }

    public IActionResult Create() => View(new Publisher());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PublisherName,Address,Phone,Email,Website")] Publisher publisher)
    {
        await CheckDuplicateAsync(publisher);
        if (!ModelState.IsValid) return View(publisher);

        publisher.PublisherName = publisher.PublisherName.Trim();
        _db.Publishers.Add(publisher);
        await _db.SaveChangesAsync();
        Success($"Đã thêm nhà xuất bản \"{publisher.PublisherName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var publisher = await _db.Publishers.FindAsync(id);
        return publisher == null ? NotFound() : View(publisher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PublisherId,PublisherName,Address,Phone,Email,Website")] Publisher publisher)
    {
        if (id != publisher.PublisherId || !await _db.Publishers.AnyAsync(p => p.PublisherId == id)) return NotFound();

        await CheckDuplicateAsync(publisher);
        if (!ModelState.IsValid) return View(publisher);

        publisher.PublisherName = publisher.PublisherName.Trim();
        _db.Update(publisher);
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật nhà xuất bản \"{publisher.PublisherName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var publisher = await _db.Publishers.AsNoTracking().FirstOrDefaultAsync(p => p.PublisherId == id);
        if (publisher == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(id);
        return View(publisher);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var publisher = await _db.Publishers.FindAsync(id);
        if (publisher == null) return NotFound();

        var blockers = await GetBlockersAsync(id);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa nhà xuất bản này vì còn sản phẩm liên quan.";
            return View(publisher);
        }

        _db.Publishers.Remove(publisher);
        await _db.SaveChangesAsync();
        Success($"Đã xóa nhà xuất bản \"{publisher.PublisherName}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(int id)
    {
        int count = await _db.Products.CountAsync(p => p.PublisherId == id);
        return count > 0 ? [$"Có {count} sản phẩm thuộc nhà xuất bản này."] : [];
    }

    private async Task CheckDuplicateAsync(Publisher publisher)
    {
        string name = (publisher.PublisherName ?? "").Trim();
        if (await _db.Publishers.AnyAsync(p => p.PublisherName == name && p.PublisherId != publisher.PublisherId))
            ModelState.AddModelError(nameof(publisher.PublisherName), "Nhà xuất bản này đã tồn tại.");
    }
}
