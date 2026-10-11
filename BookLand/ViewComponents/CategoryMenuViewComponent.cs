using BookLand.Models;
using BookLand.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.ViewComponents;

// Menu danh mục trên header: lấy danh mục cha/con từ bảng Category
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly BookLandContext _db;

    public CategoryMenuViewComponent(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryId)
            .ToListAsync();

        // 4 sản phẩm bán chạy cho mỗi danh mục con, hiển thị trong menu xổ xuống
        var topProducts = await _db.Products.AsNoTracking().InStock()
            .OrderBySold()
            .Select(p => new MenuProduct { ProductId = p.ProductId, ProductName = p.ProductName, CategoryId = p.CategoryId })
            .ToListAsync();

        var menu = categories.Where(c => c.ParentId == null).Select(parent => new MenuCategory
        {
            Category = parent,
            Icon = ProductQueries.CategoryIcon(parent.CategoryName),
            Children = categories.Where(c => c.ParentId == parent.CategoryId).Select(child => new MenuChild
            {
                Category = child,
                Products = topProducts.Where(p => p.CategoryId == child.CategoryId).Take(4).ToList()
            }).ToList()
        }).ToList();

        return View(menu);
    }
}

public class MenuCategory
{
    public Category Category { get; set; } = null!;
    public string Icon { get; set; } = "";
    public List<MenuChild> Children { get; set; } = [];
}

public class MenuChild
{
    public Category Category { get; set; } = null!;
    public List<MenuProduct> Products { get; set; } = [];
}

public class MenuProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int CategoryId { get; set; }
}
