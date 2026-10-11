using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BookLand.Models;

[ModelMetadataType(typeof(ProductMetadata))]
public partial class Product
{
    // % giảm giá so với giá bìa (0 nếu không giảm)
    public int DiscountPercent() =>
        OriginalPrice is > 0 && OriginalPrice > Price
            ? (int)Math.Round((1 - Price / OriginalPrice.Value) * 100)
            : 0;

    public int StockQuantity() => ProductDetail?.Quantity ?? 0;
}

public class ProductMetadata
{
    [Display(Name = "Mã sản phẩm")]
    public int ProductId { get; set; }

    [Display(Name = "Tên sản phẩm")]
    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ {2} đến {1} ký tự")]
    public string ProductName { get; set; } = null!;

    [Display(Name = "Danh mục")]
    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Tác giả")]
    public int? AuthorId { get; set; }

    [Display(Name = "Nhà xuất bản")]
    public int? PublisherId { get; set; }

    [Display(Name = "Nhà cung cấp")]
    public int? SupplierId { get; set; }

    [Display(Name = "Giá bán")]
    [Required(ErrorMessage = "Vui lòng nhập giá bán")]
    [Range(1000, 100000000, ErrorMessage = "Giá bán từ {1:N0} đến {2:N0} đ")]
    [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }

    [Display(Name = "Giá bìa")]
    [Range(1000, 100000000, ErrorMessage = "Giá bìa từ {1:N0} đến {2:N0} đ")]
    public decimal? OriginalPrice { get; set; }

    [Display(Name = "Ảnh sản phẩm")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Mô tả")]
    [DataType(DataType.MultilineText)]
    [StringLength(5000, ErrorMessage = "Mô tả tối đa {1} ký tự")]
    public string? Description { get; set; }

    [Display(Name = "Sản phẩm nổi bật")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Flash Sale")]
    public bool IsFlashSale { get; set; }

    [Display(Name = "Đang bán")]
    public bool IsActive { get; set; }

    [Display(Name = "Ngày tạo")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime CreatedAt { get; set; }
}

[ModelMetadataType(typeof(ProductDetailMetadata))]
public partial class ProductDetail { }

public class ProductDetailMetadata
{
    [Display(Name = "Mã chi tiết")]
    public int ProductDetailId { get; set; }

    [Display(Name = "Sản phẩm")]
    [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm")]
    public int ProductId { get; set; }

    [Display(Name = "Mã hàng")]
    [StringLength(20, ErrorMessage = "Mã hàng tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9-]*$", ErrorMessage = "Mã hàng chỉ gồm chữ, số và dấu gạch ngang")]
    public string? Sku { get; set; }

    [Display(Name = "Người dịch")]
    [StringLength(150, ErrorMessage = "Tên người dịch tối đa {1} ký tự")]
    public string? Translator { get; set; }

    [Display(Name = "Năm xuất bản")]
    [Range(1900, 2100, ErrorMessage = "Năm xuất bản từ {1} đến {2}")]
    public int? PublishYear { get; set; }

    [Display(Name = "Ngôn ngữ")]
    [StringLength(50, ErrorMessage = "Ngôn ngữ tối đa {1} ký tự")]
    public string? Language { get; set; }

    [Display(Name = "Trọng lượng (gr)")]
    [Range(1, 20000, ErrorMessage = "Trọng lượng từ {1} đến {2} gram")]
    public int? Weight { get; set; }

    [Display(Name = "Kích thước bao bì")]
    [StringLength(50, ErrorMessage = "Kích thước tối đa {1} ký tự")]
    public string? Dimensions { get; set; }

    [Display(Name = "Số trang")]
    [Range(1, 10000, ErrorMessage = "Số trang từ {1} đến {2}")]
    public int? PageCount { get; set; }

    [Display(Name = "Hình thức bìa")]
    public int? CoverTypeId { get; set; }

    [Display(Name = "Số lượng tồn")]
    [Required(ErrorMessage = "Vui lòng nhập số lượng tồn")]
    [Range(0, 100000, ErrorMessage = "Số lượng tồn từ {1} đến {2}")]
    public int Quantity { get; set; }

    [Display(Name = "Đã bán")]
    [Range(0, 10000000, ErrorMessage = "Số lượng đã bán không hợp lệ")]
    public int SoldQuantity { get; set; }
}

[ModelMetadataType(typeof(ProductImageMetadata))]
public partial class ProductImage { }

public class ProductImageMetadata
{
    [Display(Name = "Ảnh")]
    [Required]
    [StringLength(255)]
    public string ImageUrl { get; set; } = null!;

    [Display(Name = "Thứ tự")]
    public int SortOrder { get; set; }
}
