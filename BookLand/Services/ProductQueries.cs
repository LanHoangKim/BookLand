using BookLand.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Services;

public static class ProductQueries
{
    // Sản phẩm hiển thị cho khách hàng: đang bán và còn hàng (số lượng tồn > 0)
    public static IQueryable<Product> InStock(this IQueryable<Product> query) =>
        query.Where(p => p.IsActive && p.ProductDetail != null && p.ProductDetail.Quantity > 0);

    // Sắp xếp theo % giảm giá (giá bìa - giá bán) / giá bìa
    public static IOrderedQueryable<Product> OrderByDiscount(this IQueryable<Product> query) =>
        query.OrderByDescending(p => p.OriginalPrice == null || p.OriginalPrice == 0
            ? 0
            : (p.OriginalPrice.Value - p.Price) / p.OriginalPrice.Value);

    public static IOrderedQueryable<Product> OrderBySold(this IQueryable<Product> query) =>
        query.OrderByDescending(p => p.ProductDetail!.SoldQuantity);

    // Danh sách id của danh mục và các danh mục con
    public static async Task<List<int>> CategoryWithChildrenIdsAsync(this BookLandContext db, int categoryId) =>
        await db.Categories
            .Where(c => c.CategoryId == categoryId || c.ParentId == categoryId)
            .Select(c => c.CategoryId)
            .ToListAsync();

    public static string CategoryIcon(string? name)
    {
        string n = (name ?? "").ToLowerInvariant();
        if (n.Contains("văn học")) return "fa-book-open";
        if (n.Contains("kinh tế")) return "fa-chart-line";
        if (n.Contains("tâm lý") || n.Contains("kỹ năng")) return "fa-brain";
        if (n.Contains("thiếu nhi")) return "fa-child-reaching";
        if (n.Contains("ngoại ngữ")) return "fa-language";
        if (n.Contains("lịch sử") || n.Contains("khoa học")) return "fa-landmark";
        if (n.Contains("foreign")) return "fa-earth-asia";
        return "fa-book";
    }
}
