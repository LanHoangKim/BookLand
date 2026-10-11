using BookLand.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLand.ViewModels;

public class HomeViewModel
{
    public List<Product> FlashSale { get; set; } = [];
    public List<CategoryTile> CategoryTiles { get; set; } = [];
    public List<Product> TrendingDaily { get; set; } = [];
    public List<Product> HotDeals { get; set; } = [];
    public List<Product> ForeignBestsellers { get; set; } = [];
    public List<Product> Featured { get; set; } = [];
    public List<BestSellerGroup> BestSellerGroups { get; set; } = [];
    public List<Product> NewArrivals { get; set; } = [];
    public List<Product> AuthorSpotlight { get; set; } = [];
}

public class CategoryTile
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
}

public class BestSellerGroup
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public List<Product> Products { get; set; } = [];
}

// Trang danh sách sản phẩm: dùng chung cho xem theo danh mục và tìm kiếm
public class ProductListViewModel
{
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public List<string> Prices { get; set; } = [];
    public List<int> Suppliers { get; set; } = [];
    public List<int> Publishers { get; set; } = [];
    public List<int> CoverTypes { get; set; } = [];
    public string Sort { get; set; } = "ban-chay";
    public int PageSize { get; set; } = 12;

    public Category? CurrentCategory { get; set; }
    public List<Category> Breadcrumb { get; set; } = [];
    public List<Category> CategoryTree { get; set; } = [];
    public List<FacetItem> SupplierFacets { get; set; } = [];
    public List<FacetItem> PublisherFacets { get; set; } = [];
    public List<FacetItem> CoverTypeFacets { get; set; } = [];
    public PagedList<Product> Results { get; set; } = PagedList<Product>.Empty();

    public string Title => !string.IsNullOrWhiteSpace(Keyword)
        ? $"Kết quả tìm kiếm: \"{Keyword}\""
        : CurrentCategory?.CategoryName ?? "Tất cả sản phẩm";

    public static readonly (string Value, string Label, decimal Min, decimal Max)[] PriceRanges =
    [
        ("0-150000", "0đ - 150.000đ", 0, 150_000),
        ("150000-300000", "150.000đ - 300.000đ", 150_000, 300_000),
        ("300000-500000", "300.000đ - 500.000đ", 300_000, 500_000),
        ("500000-700000", "500.000đ - 700.000đ", 500_000, 700_000),
        ("700000-max", "700.000đ - Trở Lên", 700_000, decimal.MaxValue),
    ];

    public static readonly (string Value, string Label)[] SortOptions =
    [
        ("ban-chay", "Bán Chạy"),
        ("moi-nhat", "Mới Nhất"),
        ("giam-gia", "Giảm Giá Nhiều"),
        ("gia-tang", "Giá Thấp - Cao"),
        ("gia-giam", "Giá Cao - Thấp"),
    ];

    public static readonly int[] PageSizes = [12, 24, 48];
}

public class FacetItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public class ProductDetailPageViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> Related { get; set; } = [];
    public List<Category> Breadcrumb { get; set; } = [];
}

// Model cho partial _ProductDetailPartial (dùng chung trang khách hàng và trang Admin)
public class ProductDetailPartialModel
{
    public Product Product { get; set; } = null!;
    public bool ShowPurchase { get; set; }
}

// Model cho partial _OrderListPartial (dùng chung trang Admin và Lịch sử giao dịch)
public class OrderListPartialModel
{
    public IEnumerable<Order> Orders { get; set; } = [];
    public bool ForAdmin { get; set; }
    public bool ShowCustomer { get; set; }
    public bool ShowPayment { get; set; } = true;
}

public class PagedList<T>
{
    public List<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public static PagedList<T> Empty() => new() { Page = 1, PageSize = 12 };

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int page, int pageSize)
    {
        int total = await source.CountAsync();
        int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T> { Items = items, Page = page, PageSize = pageSize, TotalItems = total };
    }
}

public class PaginationModel
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
}
