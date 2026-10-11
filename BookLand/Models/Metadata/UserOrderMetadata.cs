using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BookLand.Models;

[ModelMetadataType(typeof(CustomerMetadata))]
public partial class Customer { }

public class CustomerMetadata
{
    [Display(Name = "Mã khách hàng")]
    public int CustomerId { get; set; }

    [Display(Name = "Họ tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    [Display(Name = "Email")]
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = null!;

    [Display(Name = "Số điện thoại")]
    [Required, StringLength(15)]
    public string Phone { get; set; } = null!;

    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Display(Name = "Ngày sinh")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateOnly? BirthDate { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Display(Name = "Ngày đăng ký")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime CreatedAt { get; set; }
}

[ModelMetadataType(typeof(AdminUserMetadata))]
public partial class AdminUser { }

public class AdminUserMetadata
{
    [Display(Name = "Mã")]
    public int AdminId { get; set; }

    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = null!;

    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Vai trò")]
    public string Role { get; set; } = null!;

    [Display(Name = "Ngày tạo")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime CreatedAt { get; set; }
}

[ModelMetadataType(typeof(OrderMetadata))]
public partial class Order
{
    public decimal Subtotal() => OrderDetails.Sum(d => d.Amount);

    public string OrderCode() => $"BL{OrderId:000000}";
}

public class OrderMetadata
{
    [Display(Name = "Mã đơn hàng")]
    public int OrderId { get; set; }

    [Display(Name = "Ngày đặt")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime OrderDate { get; set; }

    [Display(Name = "Người nhận")]
    public string ReceiverName { get; set; } = null!;

    [Display(Name = "Điện thoại")]
    public string ReceiverPhone { get; set; } = null!;

    [Display(Name = "Địa chỉ giao hàng")]
    public string ShippingAddress { get; set; } = null!;

    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    [Display(Name = "Thanh toán")]
    public string PaymentMethod { get; set; } = null!;

    [Display(Name = "Phí vận chuyển")]
    public decimal ShippingFee { get; set; }

    [Display(Name = "Tổng tiền")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = null!;
}
