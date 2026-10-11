using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Chi tiết sản phẩm: thông số kỹ thuật + số lượng tồn kho
public class ProductDetailsController : AdminBaseController
{
    private readonly BookLandContext _db;

    public ProductDetailsController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _db.ProductDetails.AsNoTracking().Include(d => d.Product).Include(d => d.CoverType).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            string k = search.Trim();
            query = query.Where(d => d.Product.ProductName.Contains(k) || (d.Sku != null && d.Sku.Contains(k)));
        }
        ViewBag.Search = search;
        return View(await PagedList<ProductDetail>.CreateAsync(query.OrderBy(d => d.ProductId), page, PageSize));
    }

    public async Task<IActionResult> Details(int id)
    {
        var detail = await _db.ProductDetails.AsNoTracking()
            .Include(d => d.Product).ThenInclude(p => p.Category)
            .Include(d => d.CoverType)
            .FirstOrDefaultAsync(d => d.ProductDetailId == id);
        return detail == null ? NotFound() : View(detail);
    }

    public async Task<IActionResult> Create(int? productId)
    {
        await LoadLookupsAsync(productId, null, createMode: true);
        return View(new ProductDetail { ProductId = productId ?? 0, Language = "Tiếng Việt", PublishYear = DateTime.Today.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductId,Sku,Translator,PublishYear,Language,Weight,Dimensions,PageCount,CoverTypeId,Quantity,SoldQuantity")] ProductDetail detail)
    {
        if (await _db.ProductDetails.AnyAsync(d => d.ProductId == detail.ProductId))
            ModelState.AddModelError(nameof(detail.ProductId), "Sản phẩm này đã có chi tiết, hãy chọn chức năng Sửa.");
        await CheckSkuAsync(detail);

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(detail.ProductId, detail.CoverTypeId, createMode: true);
            return View(detail);
        }

        _db.ProductDetails.Add(detail);
        await _db.SaveChangesAsync();
        Success("Đã thêm chi tiết sản phẩm.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var detail = await _db.ProductDetails.Include(d => d.Product).FirstOrDefaultAsync(d => d.ProductDetailId == id);
        if (detail == null) return NotFound();
        await LoadLookupsAsync(detail.ProductId, detail.CoverTypeId, createMode: false);
        return View(detail);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductDetailId,ProductId,Sku,Translator,PublishYear,Language,Weight,Dimensions,PageCount,CoverTypeId,Quantity,SoldQuantity")] ProductDetail input)
    {
        if (id != input.ProductDetailId) return NotFound();
        var detail = await _db.ProductDetails.Include(d => d.Product).FirstOrDefaultAsync(d => d.ProductDetailId == id);
        if (detail == null) return NotFound();

        await CheckSkuAsync(input);
        if (!ModelState.IsValid)
        {
            input.Product = detail.Product;
            await LoadLookupsAsync(detail.ProductId, input.CoverTypeId, createMode: false);
            return View(input);
        }

        detail.Sku = input.Sku?.Trim();
        detail.Translator = input.Translator;
        detail.PublishYear = input.PublishYear;
        detail.Language = input.Language;
        detail.Weight = input.Weight;
        detail.Dimensions = input.Dimensions;
        detail.PageCount = input.PageCount;
        detail.CoverTypeId = input.CoverTypeId;
        detail.Quantity = input.Quantity;
        detail.SoldQuantity = input.SoldQuantity;
        await _db.SaveChangesAsync();
        Success($"Đã cập nhật chi tiết \"{detail.Product.ProductName}\" (tồn kho: {detail.Quantity}).");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var detail = await _db.ProductDetails.AsNoTracking().Include(d => d.Product).Include(d => d.CoverType)
            .FirstOrDefaultAsync(d => d.ProductDetailId == id);
        if (detail == null) return NotFound();
        ViewBag.Blockers = GetBlockers(detail);
        return View(detail);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var detail = await _db.ProductDetails.Include(d => d.Product).Include(d => d.CoverType)
            .FirstOrDefaultAsync(d => d.ProductDetailId == id);
        if (detail == null) return NotFound();

        var blockers = GetBlockers(detail);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa chi tiết sản phẩm khi vẫn còn hàng tồn kho.";
            return View(detail);
        }

        _db.ProductDetails.Remove(detail);
        await _db.SaveChangesAsync();
        Success("Đã xóa chi tiết sản phẩm.");
        return RedirectToAction(nameof(Index));
    }

    private static List<string> GetBlockers(ProductDetail detail) =>
        detail.Quantity > 0 ? [$"Sản phẩm còn {detail.Quantity} cuốn trong kho."] : [];

    private async Task CheckSkuAsync(ProductDetail detail)
    {
        if (string.IsNullOrWhiteSpace(detail.Sku)) return;
        string sku = detail.Sku.Trim();
        if (await _db.ProductDetails.AnyAsync(d => d.Sku == sku && d.ProductDetailId != detail.ProductDetailId))
            ModelState.AddModelError(nameof(detail.Sku), "Mã hàng đã được dùng cho sản phẩm khác.");
    }

    private async Task LoadLookupsAsync(int? productId, int? coverTypeId, bool createMode)
    {
        var products = _db.Products.AsNoTracking();
        if (createMode) products = products.Where(p => p.ProductDetail == null);
        ViewBag.ProductId = new SelectList(await products.OrderBy(p => p.ProductName).ToListAsync(), "ProductId", "ProductName", productId);
        ViewBag.CoverTypeId = new SelectList(await _db.CoverTypes.AsNoTracking().ToListAsync(), "CoverTypeId", "CoverTypeName", coverTypeId);
    }
}
