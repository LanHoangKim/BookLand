using BookLand.Helpers;
using BookLand.Models;
using BookLand.Services;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Controllers;

[Authorize(AuthenticationSchemes = AuthSchemes.Customer)]
public class CheckoutController : Controller
{
    private readonly BookLandContext _db;
    private readonly CartService _cart;

    public CheckoutController(BookLandContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await _cart.GetCartAsync();
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Giỏ hàng đang trống, vui lòng chọn sản phẩm trước khi thanh toán.";
            return RedirectToAction("Index", "Cart");
        }

        // Điền sẵn thông tin thanh toán từ tài khoản đang đăng nhập
        var customer = await _db.Customers.FindAsync(User.GetUserId());
        if (customer == null) return RedirectToAction("Login", "Account");

        return View(new CheckoutViewModel
        {
            ReceiverName = customer.FullName,
            ReceiverPhone = customer.Phone,
            ShippingAddress = customer.Address ?? "",
            Email = customer.Email,
            Cart = cart
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cart = await _cart.GetCartAsync();
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Giỏ hàng đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        int customerId = User.GetUserId() ?? 0;
        var customer = await _db.Customers.FindAsync(customerId);
        if (customer == null) return RedirectToAction("Login", "Account");

        model.Cart = cart;
        model.Email = customer.Email;
        if (!PaymentMethods.All.Contains(model.PaymentMethod))
        {
            ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ");
        }
        if (cart.Warnings.Count > 0)
        {
            ModelState.AddModelError("", "Giỏ hàng vừa được cập nhật theo tồn kho, vui lòng kiểm tra lại trước khi đặt hàng.");
        }
        if (!ModelState.IsValid) return View(model);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var ids = cart.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Include(p => p.ProductDetail)
            .Where(p => ids.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId);

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.Now,
            ReceiverName = model.ReceiverName.Trim(),
            ReceiverPhone = model.ReceiverPhone.Trim(),
            ShippingAddress = model.ShippingAddress.Trim(),
            Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
            PaymentMethod = model.PaymentMethod,
            Status = OrderStatuses.Pending
        };

        foreach (var item in cart.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive || product.ProductDetail == null)
            {
                ModelState.AddModelError("", $"Sản phẩm \"{item.ProductName}\" không còn được bán.");
                continue;
            }
            if (product.ProductDetail.Quantity < item.Quantity)
            {
                ModelState.AddModelError("", $"\"{product.ProductName}\" chỉ còn {product.ProductDetail.Quantity} sản phẩm.");
                continue;
            }

            // Thành tiền từng mục theo giá hiện tại
            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Amount = product.Price * item.Quantity
            });

            // Cập nhật lại số lượng tồn và số lượng đã bán
            product.ProductDetail.Quantity -= item.Quantity;
            product.ProductDetail.SoldQuantity += item.Quantity;
        }

        if (!ModelState.IsValid)
        {
            await transaction.RollbackAsync();
            return View(model);
        }

        decimal subtotal = order.OrderDetails.Sum(d => d.Amount);
        order.ShippingFee = subtotal >= CartRules.FreeShipThreshold ? 0 : CartRules.ShippingFee;
        order.TotalAmount = subtotal + order.ShippingFee;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        _cart.Clear();
        return RedirectToAction(nameof(Success), new { id = order.OrderId });
    }

    public async Task<IActionResult> Success(int id)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == User.GetUserId());
        if (order == null) return NotFound();
        return View(order);
    }
}
