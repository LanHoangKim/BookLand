using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BookLand.Models;

// Data Annotation đặt trong lớp Metadata (không sửa trực tiếp file model sinh tự động từ database)

[ModelMetadataType(typeof(CategoryMetadata))]
public partial class Category { }

public class CategoryMetadata
{
    [Display(Name = "Mã danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Tên danh mục")]
    [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ {2} đến {1} ký tự")]
    public string CategoryName { get; set; } = null!;

    [Display(Name = "Danh mục cha")]
    public int? ParentId { get; set; }

    [Display(Name = "Ảnh đại diện")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Mô tả")]
    [StringLength(500, ErrorMessage = "Mô tả tối đa {1} ký tự")]
    public string? Description { get; set; }

    [Display(Name = "Thứ tự hiển thị")]
    [Range(0, 1000, ErrorMessage = "Thứ tự từ {1} đến {2}")]
    public int SortOrder { get; set; }

    [Display(Name = "Đang hiển thị")]
    public bool IsActive { get; set; }
}

[ModelMetadataType(typeof(AuthorMetadata))]
public partial class Author { }

public class AuthorMetadata
{
    [Display(Name = "Mã tác giả")]
    public int AuthorId { get; set; }

    [Display(Name = "Tên tác giả")]
    [Required(ErrorMessage = "Vui lòng nhập tên tác giả")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên tác giả từ {2} đến {1} ký tự")]
    public string AuthorName { get; set; } = null!;

    [Display(Name = "Quốc tịch")]
    [StringLength(50, ErrorMessage = "Quốc tịch tối đa {1} ký tự")]
    public string? Nationality { get; set; }

    [Display(Name = "Năm sinh")]
    [Range(1000, 2025, ErrorMessage = "Năm sinh từ {1} đến {2}")]
    public int? BirthYear { get; set; }

    [Display(Name = "Tiểu sử")]
    [StringLength(1000, ErrorMessage = "Tiểu sử tối đa {1} ký tự")]
    [DataType(DataType.MultilineText)]
    public string? Biography { get; set; }
}

[ModelMetadataType(typeof(PublisherMetadata))]
public partial class Publisher { }

public class PublisherMetadata
{
    [Display(Name = "Mã NXB")]
    public int PublisherId { get; set; }

    [Display(Name = "Tên nhà xuất bản")]
    [Required(ErrorMessage = "Vui lòng nhập tên nhà xuất bản")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên nhà xuất bản từ {2} đến {1} ký tự")]
    public string PublisherName { get; set; } = null!;

    [Display(Name = "Địa chỉ")]
    [StringLength(255, ErrorMessage = "Địa chỉ tối đa {1} ký tự")]
    public string? Address { get; set; }

    [Display(Name = "Điện thoại")]
    [RegularExpression(@"^[0-9 .+()-]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Display(Name = "Website")]
    [StringLength(150, ErrorMessage = "Website tối đa {1} ký tự")]
    public string? Website { get; set; }
}

[ModelMetadataType(typeof(SupplierMetadata))]
public partial class Supplier { }

public class SupplierMetadata
{
    [Display(Name = "Mã nhà cung cấp")]
    public int SupplierId { get; set; }

    [Display(Name = "Tên nhà cung cấp")]
    [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên nhà cung cấp từ {2} đến {1} ký tự")]
    public string SupplierName { get; set; } = null!;

    [Display(Name = "Địa chỉ")]
    [StringLength(255, ErrorMessage = "Địa chỉ tối đa {1} ký tự")]
    public string? Address { get; set; }

    [Display(Name = "Điện thoại")]
    [RegularExpression(@"^[0-9 .+()-]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Display(Name = "Website")]
    [StringLength(150, ErrorMessage = "Website tối đa {1} ký tự")]
    public string? Website { get; set; }
}

[ModelMetadataType(typeof(CoverTypeMetadata))]
public partial class CoverType { }

public class CoverTypeMetadata
{
    [Display(Name = "Mã hình thức")]
    public int CoverTypeId { get; set; }

    [Display(Name = "Hình thức bìa")]
    [Required(ErrorMessage = "Vui lòng nhập tên hình thức bìa")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên hình thức từ {2} đến {1} ký tự")]
    public string CoverTypeName { get; set; } = null!;
}
