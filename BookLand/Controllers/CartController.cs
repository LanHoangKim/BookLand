using BookLand.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookLand.Controllers;

public class CartController : Controller
{
    private readonly CartService _cart;

    public CartController(CartService cart)
    {
        _cart = cart;
    }

    private bool IsAjax => Request.Headers.XRequestedWith == "XMLHttpRequest";

    public async Task<IActionResult> Index()
    {
        return View(await _cart.GetCartAsync());
    }

    // Thêm vào giỏ: nhận số lượng từ trang chi tiết; xong chuyển tới trang giỏ hàng (hoặc thanh toán nếu "Mua ngay")
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? buyNow = null)
    {
        var result = await _cart.AddAsync(productId, quantity);
        if (IsAjax) return Json(result);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        if (result.Success && !string.IsNullOrEmpty(buyNow)) return RedirectToAction("Index", "Checkout");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var result = await _cart.UpdateAsync(productId, quantity);
        if (IsAjax) return Json(result);

        if (!result.Success) TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        var result = await _cart.RemoveAsync(productId);
        if (IsAjax) return Json(result);

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
