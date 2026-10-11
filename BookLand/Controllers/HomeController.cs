using System.Diagnostics;
using BookLand.Models;
using BookLand.Services;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Controllers;

public class HomeController : Controller
{
    private readonly BookLandContext _db;

    public HomeController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Chỉ lấy sản phẩm đang bán và còn hàng (Quantity > 0)
        var products = _db.Products.AsNoTracking().InStock().Include(p => p.ProductDetail);

        var vm = new HomeViewModel
        {
            // Flash Sale: sản phẩm được đánh dấu IsFlashSale, giảm giá nhiều nhất trước
            FlashSale = await products.Where(p => p.IsFlashSale).OrderByDiscount().Take(10).ToListAsync(),
            // Xu hướng theo ngày: bán chạy nhất (SoldQuantity)
            TrendingDaily = await products.OrderBySold().Take(10).ToListAsync(),
            // Sách HOT - Giảm sốc: % giảm giá cao nhất
            HotDeals = await products.OrderByDiscount().ThenByDescending(p => p.ProductDetail!.SoldQuantity).Take(10).ToListAsync(),
            // Sản phẩm nổi bật: IsFeatured
            Featured = await products.Where(p => p.IsFeatured).OrderBySold().Take(10).ToListAsync(),
            // Sách mới: CreatedAt mới nhất
            NewArrivals = await products.OrderByDescending(p => p.CreatedAt).Take(10).ToListAsync()
        };

        var foreign = await _db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ParentId == null && c.CategoryName.Contains("Foreign"));
        if (foreign != null)
        {
            var foreignIds = await _db.CategoryWithChildrenIdsAsync(foreign.CategoryId);
            vm.ForeignBestsellers = await products.Where(p => foreignIds.Contains(p.CategoryId)).OrderBySold().Take(10).ToListAsync();
        }

        // Danh mục: lấy xen kẽ danh mục con của từng danh mục cha để đa dạng
        var categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryId).ToListAsync();
        var parents = categories.Where(c => c.ParentId == null).ToList();
        var childrenByParent = parents.Select(p => new Queue<Category>(categories.Where(c => c.ParentId == p.CategoryId))).ToList();
        while (vm.CategoryTiles.Count < 10 && childrenByParent.Any(q => q.Count > 0))
        {
            foreach (var queue in childrenByParent.Where(q => q.Count > 0))
            {
                var c = queue.Dequeue();
                vm.CategoryTiles.Add(new CategoryTile { CategoryId = c.CategoryId, Name = c.CategoryName, ImageUrl = c.ImageUrl });
                if (vm.CategoryTiles.Count == 10) break;
            }
        }

        // Best Sellers: top 5 bán chạy của từng danh mục cha
        var bestSellers = await products
            .Include(p => p.Author)
            .Include(p => p.Publisher)
            .Include(p => p.Category)
            .OrderBySold()
            .ToListAsync();
        foreach (var parent in parents)
        {
            var top = bestSellers
                .Where(p => p.CategoryId == parent.CategoryId || p.Category.ParentId == parent.CategoryId)
                .Take(5)
                .ToList();
            if (top.Count > 0)
            {
                vm.BestSellerGroups.Add(new BestSellerGroup { CategoryId = parent.CategoryId, CategoryName = parent.CategoryName, Products = top });
            }
        }

        ViewBag.ParentCategories = parents;
        return View(vm);
    }

    [Route("Home/StatusCode")]
    public IActionResult StatusCodePage(int code)
    {
        Response.StatusCode = code;
        return View("StatusCode", code);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
