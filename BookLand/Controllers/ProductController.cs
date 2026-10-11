using System.Linq.Expressions;
using BookLand.Models;
using BookLand.Services;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Controllers;

public class ProductController : Controller
{
    private readonly BookLandContext _db;

    public ProductController(BookLandContext db)
    {
        _db = db;
    }

    // Danh sách sản phẩm theo danh mục / tìm kiếm, có bộ lọc và phân trang
    public async Task<IActionResult> Index(int? categoryId, string? keyword, List<string>? prices,
        List<int>? suppliers, List<int>? publishers, List<int>? coverTypes,
        string sort = "ban-chay", int page = 1, int pageSize = 12)
    {
        if (!ProductListViewModel.PageSizes.Contains(pageSize)) pageSize = 12;
        if (!ProductListViewModel.SortOptions.Any(s => s.Value == sort)) sort = "ban-chay";

        var vm = new ProductListViewModel
        {
            CategoryId = categoryId,
            Keyword = keyword?.Trim(),
            Prices = prices ?? [],
            Suppliers = suppliers ?? [],
            Publishers = publishers ?? [],
            CoverTypes = coverTypes ?? [],
            Sort = sort,
            PageSize = pageSize
        };

        var categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryId).ToListAsync();
        vm.CategoryTree = categories;

        IQueryable<Product> query = _db.Products.AsNoTracking().InStock();

        if (categoryId.HasValue)
        {
            vm.CurrentCategory = categories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (vm.CurrentCategory == null) return NotFound();

            var ids = categories.Where(c => c.CategoryId == categoryId || c.ParentId == categoryId).Select(c => c.CategoryId).ToList();
            query = query.Where(p => ids.Contains(p.CategoryId));

            var parent = categories.FirstOrDefault(c => c.CategoryId == vm.CurrentCategory.ParentId);
            if (parent != null) vm.Breadcrumb.Add(parent);
            vm.Breadcrumb.Add(vm.CurrentCategory);
        }

        if (!string.IsNullOrWhiteSpace(vm.Keyword))
        {
            string k = vm.Keyword;
            query = query.Where(p => p.ProductName.Contains(k)
                                     || (p.Author != null && p.Author.AuthorName.Contains(k))
                                     || (p.Publisher != null && p.Publisher.PublisherName.Contains(k)));
        }

        // Bộ lọc hiển thị theo kết quả hiện tại (trước khi áp dụng chính các bộ lọc này)
        vm.SupplierFacets = await query.Where(p => p.SupplierId != null)
            .GroupBy(p => new { p.SupplierId, p.Supplier!.SupplierName })
            .Select(g => new FacetItem { Id = g.Key.SupplierId!.Value, Name = g.Key.SupplierName, Count = g.Count() })
            .OrderByDescending(f => f.Count).ThenBy(f => f.Name).ToListAsync();
        vm.PublisherFacets = await query.Where(p => p.PublisherId != null)
            .GroupBy(p => new { p.PublisherId, p.Publisher!.PublisherName })
            .Select(g => new FacetItem { Id = g.Key.PublisherId!.Value, Name = g.Key.PublisherName, Count = g.Count() })
            .OrderByDescending(f => f.Count).ThenBy(f => f.Name).ToListAsync();
        vm.CoverTypeFacets = await query.Where(p => p.ProductDetail!.CoverTypeId != null)
            .GroupBy(p => new { p.ProductDetail!.CoverTypeId, p.ProductDetail.CoverType!.CoverTypeName })
            .Select(g => new FacetItem { Id = g.Key.CoverTypeId!.Value, Name = g.Key.CoverTypeName, Count = g.Count() })
            .OrderByDescending(f => f.Count).ToListAsync();

        var pricePredicate = BuildPricePredicate(vm.Prices);
        if (pricePredicate != null) query = query.Where(pricePredicate);
        if (vm.Suppliers.Count > 0) query = query.Where(p => p.SupplierId != null && vm.Suppliers.Contains(p.SupplierId.Value));
        if (vm.Publishers.Count > 0) query = query.Where(p => p.PublisherId != null && vm.Publishers.Contains(p.PublisherId.Value));
        if (vm.CoverTypes.Count > 0)
            query = query.Where(p => p.ProductDetail!.CoverTypeId != null && vm.CoverTypes.Contains(p.ProductDetail.CoverTypeId.Value));

        query = query.Include(p => p.ProductDetail);
        query = sort switch
        {
            "moi-nhat" => query.OrderByDescending(p => p.CreatedAt),
            "giam-gia" => query.OrderByDiscount().ThenByDescending(p => p.ProductDetail!.SoldQuantity),
            "gia-tang" => query.OrderBy(p => p.Price),
            "gia-giam" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBySold().ThenBy(p => p.ProductId)
        };

        vm.Results = await PagedList<Product>.CreateAsync(query, page, pageSize);
        return View(vm);
    }

    // Trang chi tiết sản phẩm
    public async Task<IActionResult> Detail(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Category).ThenInclude(c => c.Parent)
            .Include(p => p.Author)
            .Include(p => p.Publisher)
            .Include(p => p.Supplier)
            .Include(p => p.ProductDetail).ThenInclude(d => d!.CoverType)
            .Include(p => p.ProductImages.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);
        if (product == null) return NotFound();

        var vm = new ProductDetailPageViewModel { Product = product };
        if (product.Category.Parent != null) vm.Breadcrumb.Add(product.Category.Parent);
        vm.Breadcrumb.Add(product.Category);

        vm.Related = await _db.Products.AsNoTracking().InStock()
            .Include(p => p.ProductDetail)
            .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
            .OrderBySold()
            .Take(10)
            .ToListAsync();

        // Nếu cùng danh mục có ít sản phẩm, lấy thêm sản phẩm cùng danh mục cha
        if (vm.Related.Count < 5 && product.Category.ParentId.HasValue)
        {
            var siblingIds = await _db.CategoryWithChildrenIdsAsync(product.Category.ParentId.Value);
            var existing = vm.Related.Select(p => p.ProductId).Append(id).ToList();
            vm.Related.AddRange(await _db.Products.AsNoTracking().InStock()
                .Include(p => p.ProductDetail)
                .Where(p => siblingIds.Contains(p.CategoryId) && !existing.Contains(p.ProductId))
                .OrderBySold()
                .Take(10 - vm.Related.Count)
                .ToListAsync());
        }

        return View(vm);
    }

    // Ghép điều kiện OR cho nhiều khoảng giá được chọn
    private static Expression<Func<Product, bool>>? BuildPricePredicate(IEnumerable<string> values)
    {
        var ranges = ProductListViewModel.PriceRanges.Where(r => values.Contains(r.Value)).ToList();
        if (ranges.Count == 0) return null;

        var p = Expression.Parameter(typeof(Product), "p");
        var price = Expression.Property(p, nameof(Product.Price));
        Expression? body = null;
        foreach (var r in ranges)
        {
            Expression condition = Expression.GreaterThanOrEqual(price, Expression.Constant(r.Min));
            if (r.Max != decimal.MaxValue)
            {
                condition = Expression.AndAlso(condition, Expression.LessThan(price, Expression.Constant(r.Max)));
            }
            body = body == null ? condition : Expression.OrElse(body, condition);
        }
        return Expression.Lambda<Func<Product, bool>>(body!, p);
    }
}
