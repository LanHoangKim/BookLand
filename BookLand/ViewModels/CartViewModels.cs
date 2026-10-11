namespace BookLand.ViewModels;

// Dòng giỏ hàng lưu trong Session: chỉ lưu mã sản phẩm và số lượng, giá luôn lấy mới từ database
public class CartLine
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public decimal LineTotal => Price * Quantity;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = [];
    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
    public decimal ShippingFee => IsEmpty || Subtotal >= CartRules.FreeShipThreshold ? 0 : CartRules.ShippingFee;
    public decimal Total => Subtotal + ShippingFee;
    public bool IsEmpty => Items.Count == 0;
    public List<string> Warnings { get; set; } = [];
}

public static class CartRules
{
    public const decimal FreeShipThreshold = 300_000;
    public const decimal ShippingFee = 30_000;
}

// Kết quả trả về cho AJAX khi thêm / cập nhật / xóa sản phẩm trong giỏ
public class CartActionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public string LineTotal { get; set; } = "";
    public string Subtotal { get; set; } = "";
    public string ShippingFee { get; set; } = "";
    public string Total { get; set; } = "";
    public int CartCount { get; set; }
    public bool IsEmpty { get; set; }
}
