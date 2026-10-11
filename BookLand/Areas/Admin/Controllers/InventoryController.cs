using BookLand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Quản lý kho hàng: thống kê theo số lượng tồn trong bảng ProductDetail
public class InventoryController : AdminBaseController
{
    private readonly BookLandContext _db;

    public InventoryController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, string tab = "ton-kho")
    {
        var query = _db.ProductDetails.AsNoTracking()
            .Include(d => d.Product).ThenInclude(p => p.Category)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            string k = search.Trim();
            query = query.Where(d => d.Product.ProductName.Contains(k) || (d.Sku != null && d.Sku.Contains(k)));
        }

        // Danh sách hàng tồn: số lượng tồn > 0; Danh sách hàng cần nhập: số lượng tồn = 0
        ViewBag.InStock = await query.Where(d => d.Quantity > 0).OrderBy(d => d.Quantity).ToListAsync();
        ViewBag.NeedImport = await query.Where(d => d.Quantity == 0).OrderByDescending(d => d.SoldQuantity).ToListAsync();
        ViewBag.Search = search;
        ViewBag.Tab = tab;
        return View();
    }
}
