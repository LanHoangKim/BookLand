using BookLand.Helpers;
using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Services;

// Giỏ hàng lưu trong Session; mọi thao tác đều kiểm tra lại số lượng tồn trong database
public class CartService
{
    private const string SessionKey = "BOOKLAND_CART";
    private readonly IHttpContextAccessor _http;
    private readonly BookLandContext _db;

    public CartService(IHttpContextAccessor http, BookLandContext db)
    {
        _http = http;
        _db = db;
    }

    private ISession Session => _http.HttpContext!.Session;

    public List<CartLine> GetLines() => Session.GetObject<List<CartLine>>(SessionKey) ?? [];

    private void SaveLines(List<CartLine> lines) => Session.SetObject(SessionKey, lines);

    public int Count() => GetLines().Sum(l => l.Quantity);

    public void Clear() => Session.Remove(SessionKey);

    private Task<Product?> LoadProductAsync(int productId) =>
        _db.Products.AsNoTracking()
            .Include(p => p.ProductDetail)
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

    public async Task<CartActionResult> AddAsync(int productId, int quantity)
    {
        var product = await LoadProductAsync(productId);
        if (product == null) return await ResultAsync(productId, false, "Sản phẩm không tồn tại hoặc đã ngừng bán.");

        int stock = product.StockQuantity();
        if (stock <= 0) return await ResultAsync(productId, false, $"\"{product.ProductName}\" đã hết hàng.");
        if (quantity < 1) return await ResultAsync(productId, false, "Số lượng mua phải lớn hơn 0.");

        var lines = GetLines();
        var line = lines.FirstOrDefault(l => l.ProductId == productId);
        int current = line?.Quantity ?? 0;
        int wanted = current + quantity;
        string? message = null;
        if (wanted > stock)
        {
            wanted = stock;
            message = $"Chỉ còn {stock} sản phẩm \"{product.ProductName}\", giỏ hàng đã được cập nhật tối đa.";
        }

        if (line == null) lines.Add(new CartLine { ProductId = productId, Quantity = wanted });
        else line.Quantity = wanted;
        SaveLines(lines);

        return await ResultAsync(productId, true, message ?? $"Đã thêm \"{product.ProductName}\" vào giỏ hàng.");
    }

    public async Task<CartActionResult> UpdateAsync(int productId, int quantity)
    {
        var lines = GetLines();
        var line = lines.FirstOrDefault(l => l.ProductId == productId);
        if (line == null) return await ResultAsync(productId, false, "Sản phẩm không có trong giỏ hàng.");

        var product = await LoadProductAsync(productId);
        if (product == null)
        {
            lines.Remove(line);
            SaveLines(lines);
            return await ResultAsync(productId, false, "Sản phẩm đã ngừng bán và được xóa khỏi giỏ hàng.");
        }

        int stock = product.StockQuantity();
        string? message = null;
        bool ok = true;
        if (quantity < 1)
        {
            quantity = 1;
            ok = false;
            message = "Số lượng tối thiểu là 1. Bấm biểu tượng thùng rác nếu muốn xóa sản phẩm.";
        }
        if (quantity > stock)
        {
            quantity = Math.Max(stock, 1);
            ok = false;
            message = $"Chỉ còn {stock} sản phẩm trong kho.";
        }

        line.Quantity = quantity;
        SaveLines(lines);
        return await ResultAsync(productId, ok, message);
    }

    public async Task<CartActionResult> RemoveAsync(int productId)
    {
        var lines = GetLines();
        lines.RemoveAll(l => l.ProductId == productId);
        SaveLines(lines);
        return await ResultAsync(productId, true, "Đã xóa sản phẩm khỏi giỏ hàng.");
    }

    // Lấy giỏ hàng đầy đủ thông tin; tự điều chỉnh nếu tồn kho thay đổi
    public async Task<CartViewModel> GetCartAsync()
    {
        var lines = GetLines();
        var cart = new CartViewModel();
        if (lines.Count == 0) return cart;

        var ids = lines.Select(l => l.ProductId).ToList();
        var products = await _db.Products.AsNoTracking()
            .Include(p => p.ProductDetail)
            .Where(p => ids.Contains(p.ProductId) && p.IsActive)
            .ToDictionaryAsync(p => p.ProductId);

        bool changed = false;
        foreach (var line in lines.ToList())
        {
            if (!products.TryGetValue(line.ProductId, out var p) || p.StockQuantity() <= 0)
            {
                cart.Warnings.Add(p == null
                    ? "Một sản phẩm đã ngừng bán và được xóa khỏi giỏ hàng."
                    : $"\"{p.ProductName}\" đã hết hàng và được xóa khỏi giỏ hàng.");
                lines.Remove(line);
                changed = true;
                continue;
            }

            int stock = p.StockQuantity();
            if (line.Quantity > stock)
            {
                cart.Warnings.Add($"\"{p.ProductName}\" chỉ còn {stock} sản phẩm, số lượng đã được điều chỉnh.");
                line.Quantity = stock;
                changed = true;
            }

            cart.Items.Add(new CartItemViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Quantity = line.Quantity,
                Stock = stock
            });
        }

        if (changed) SaveLines(lines);
        return cart;
    }

    private async Task<CartActionResult> ResultAsync(int productId, bool success, string? message)
    {
        var cart = await GetCartAsync();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        return new CartActionResult
        {
            Success = success,
            Message = message,
            ProductId = productId,
            Quantity = item?.Quantity ?? 0,
            Stock = item?.Stock ?? 0,
            LineTotal = (item?.LineTotal ?? 0).ToVnd(),
            Subtotal = cart.Subtotal.ToVnd(),
            ShippingFee = cart.ShippingFee == 0 ? "Miễn phí" : cart.ShippingFee.ToVnd(),
            Total = cart.Total.ToVnd(),
            CartCount = cart.TotalQuantity,
            IsEmpty = cart.IsEmpty
        };
    }
}
