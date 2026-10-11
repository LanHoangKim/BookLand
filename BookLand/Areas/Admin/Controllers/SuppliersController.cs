using BookLand.Areas.Admin.Models;
using BookLand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

public class SuppliersController : AdminBaseController
{
    private readonly BookLandContext _db;

    public SuppliersController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.Suppliers.AsNoTracking()
            .OrderBy(s => s.SupplierName)
            .Select(s => new NamedCountItem<Supplier> { Item = s, ProductCount = s.Products.Count })
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierId == id);
        if (supplier == null) return NotFound();
        ViewBag.Products = await _db.Products.AsNoTracking().Include(p => p.ProductDetail).Include(p => p.Category)
            .Where(p => p.SupplierId == id).OrderBy(p => p.ProductName).ToListAsync();
        return View(supplier);
    }

    public IActionResult Create() => View(new Supplier());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SupplierName,Address,Phone,Email,Website")] Supplier supplier)
    {
        await CheckDuplicateAsync(supplier);
        if (!ModelState.IsValid) return View(supplier);

        supplier.SupplierName = supplier.SupplierName.Trim();
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        Success($"Đã thêm nhà cung cấp \"{supplier.SupplierName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        return supplier == null ? NotFound() : View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SupplierId,SupplierName,Address,Phone,Email,Website")] Supplier supplier)
    {
        if (id != supplier.SupplierId || !await _db.Suppliers.AnyAsync(s => s.SupplierId == id)) return NotFound();

        await CheckDuplicateAsync(supplier);
        if (!ModelState.IsValid) return View(supplier);

        supplier.SupplierName = supplier.SupplierName.Trim();
        _db.Update(supplier);
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật nhà cung cấp \"{supplier.SupplierName}\".");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierId == id);
        if (supplier == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(id);
        return View(supplier);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        var blockers = await GetBlockersAsync(id);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa nhà cung cấp này vì còn sản phẩm liên quan.";
            return View(supplier);
        }

        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync();
        Success($"Đã xóa nhà cung cấp \"{supplier.SupplierName}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(int id)
    {
        int count = await _db.Products.CountAsync(p => p.SupplierId == id);
        return count > 0 ? [$"Có {count} sản phẩm của nhà cung cấp này."] : [];
    }

    private async Task CheckDuplicateAsync(Supplier supplier)
    {
        string name = (supplier.SupplierName ?? "").Trim();
        if (await _db.Suppliers.AnyAsync(s => s.SupplierName == name && s.SupplierId != supplier.SupplierId))
            ModelState.AddModelError(nameof(supplier.SupplierName), "Nhà cung cấp này đã tồn tại.");
    }
}
