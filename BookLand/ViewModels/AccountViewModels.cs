using System.ComponentModel.DataAnnotations;

namespace BookLand.ViewModels;

public class LoginViewModel
{
    [Display(Name = "Số điện thoại/Email")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại hoặc email")]
    [StringLength(100)]
    public string UserName { get; set; } = "";

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ {2} đến {1} ký tự")]
    public string FullName { get; set; } = "";

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số và bắt đầu bằng 0")]
    public string Phone { get; set; } = "";

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email tối đa {1} ký tự")]
    public string Email { get; set; } = "";

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ {2} đến {1} ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Nhập lại mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileViewModel
{
    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ {2} đến {1} ký tự")]
    public string FullName { get; set; } = "";

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số và bắt đầu bằng 0")]
    public string Phone { get; set; } = "";

    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }

    [Display(Name = "Địa chỉ")]
    [StringLength(255, ErrorMessage = "Địa chỉ tối đa {1} ký tự")]
    public string? Address { get; set; }
}

public class ChangePasswordViewModel
{
    [Display(Name = "Mật khẩu hiện tại")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Display(Name = "Mật khẩu mới")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ {2} đến {1} ký tự")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Display(Name = "Nhập lại mật khẩu mới")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class CheckoutViewModel
{
    [Display(Name = "Họ và tên người nhận")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ {2} đến {1} ký tự")]
    public string ReceiverName { get; set; } = "";

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số và bắt đầu bằng 0")]
    public string ReceiverPhone { get; set; } = "";

    [Display(Name = "Địa chỉ nhận hàng")]
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
    [StringLength(255, MinimumLength = 10, ErrorMessage = "Địa chỉ từ {2} đến {1} ký tự")]
    public string ShippingAddress { get; set; } = "";

    [Display(Name = "Ghi chú")]
    [StringLength(500, ErrorMessage = "Ghi chú tối đa {1} ký tự")]
    public string? Note { get; set; }

    [Display(Name = "Phương thức thanh toán")]
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    public string PaymentMethod { get; set; } = PaymentMethods.Cod;

    public string? Email { get; set; }

    public CartViewModel Cart { get; set; } = new();
}

public static class PaymentMethods
{
    public const string Cod = "Thanh toán khi nhận hàng";
    public const string BankTransfer = "Chuyển khoản ngân hàng";
    public static readonly string[] All = [Cod, BankTransfer];
}

public static class OrderStatuses
{
    public const string Pending = "Chờ xác nhận";
    public const string Shipping = "Đang giao";
    public const string Done = "Đã giao";
    public const string Cancelled = "Đã hủy";
    public static readonly string[] All = [Pending, Shipping, Done, Cancelled];
}
