using BookLand.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookLand.ViewComponents;

// Số lượng sản phẩm trong giỏ, hiển thị trên icon giỏ hàng ở header
public class CartCountViewComponent : ViewComponent
{
    private readonly CartService _cart;

    public CartCountViewComponent(CartService cart)
    {
        _cart = cart;
    }

    public IViewComponentResult Invoke()
    {
        return View(_cart.Count());
    }
}
