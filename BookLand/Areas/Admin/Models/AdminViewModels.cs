using System.ComponentModel.DataAnnotations;
using BookLand.Models;

namespace BookLand.Areas.Admin.Models;

// Tiêu đề trang Admin: tên trang, nhóm menu cha, có nút Thêm mới hay không
public record PageHeaderModel(string Title, string? Parent = null, bool CanCreate = false);

public class AdminLoginViewModel
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string Username { get; set; } = "";

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

// Dùng cho trang Thêm/Sửa người quản trị (mật khẩu chỉ bắt buộc khi thêm mới)
public class AdminUserFormViewModel
{
    public int AdminId { get; set; }

    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ {2} đến {1} ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Chỉ dùng chữ không dấu, số, dấu chấm và gạch dưới")]
    public string Username { get; set; } = "";

    [Display(Name = "Họ tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ {2} đến {1} ký tự")]
    public string FullName { get; set; } = "";

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email tối đa {1} ký tự")]
    public string? Email { get; set; }

    [Display(Name = "Vai trò")]
    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    public string Role { get; set; } = AdminRoles.Staff;

    [Display(Name = "Mật khẩu")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ {2} đến {1} ký tự")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Nhập lại mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
}

public static class AdminRoles
{
    public const string Admin = "Quản trị viên";
    public const string Staff = "Nhân viên";
    public static readonly string[] All = [Admin, Staff];
}

public class DashboardViewModel
{
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public int PendingOrders { get; set; }
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<Order> RecentOrders { get; set; } = [];
    public List<Product> TopSelling { get; set; } = [];
    public List<Product> LowStock { get; set; } = [];
    public List<(string Label, decimal Value)> RevenueByMonth { get; set; } = [];
}

public class CategoryIndexItem
{
    public Category Category { get; set; } = null!;
    public int ProductCount { get; set; }
}

public class NamedCountItem<T>
{
    public T Item { get; set; } = default!;
    public int ProductCount { get; set; }
}

public class CustomerIndexItem
{
    public Customer Customer { get; set; } = null!;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}
