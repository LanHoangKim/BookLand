using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Quản lý đơn hàng: danh sách không có Thêm/Sửa/Xóa; trang chi tiết hiển thị như hóa đơn mua hàng
public class OrdersController : AdminBaseController
{
    private readonly BookLandContext _db;

    public OrdersController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, string? status, int page = 1)
    {
        var query = _db.Orders.AsNoTracking().Include(o => o.Customer).Include(o => o.OrderDetails).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string k = search.Trim();
            string digits = new(k.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int code);
            query = query.Where(o => o.OrderId == code || o.Customer.FullName.Contains(k)
                                     || o.ReceiverPhone.Contains(k) || o.ReceiverName.Contains(k));
        }

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.StatusCounts = await _db.Orders.GroupBy(o => o.Status).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
        return View(await PagedList<Order>.CreateAsync(query.OrderByDescending(o => o.OrderDate), page, PageSize));
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product).ThenInclude(p => p.ProductDetail)
            .FirstOrDefaultAsync(o => o.OrderId == id);
        return order == null ? NotFound() : View(order);
    }

    // Cập nhật trạng thái; hủy đơn thì hoàn lại số lượng tồn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _db.Orders.Include(o => o.OrderDetails).ThenInclude(d => d.Product).ThenInclude(p => p.ProductDetail)
            .FirstOrDefaultAsync(o => o.OrderId == id);
        if (order == null) return NotFound();
        if (!OrderStatuses.All.Contains(status))
        {
            Error("Trạng thái không hợp lệ.");
            return RedirectToAction(nameof(Details), new { id });
        }
        if (order.Status == status) return RedirectToAction(nameof(Details), new { id });

        bool cancelling = status == OrderStatuses.Cancelled;
        bool restoring = order.Status == OrderStatuses.Cancelled;
        foreach (var d in order.OrderDetails)
        {
            var stock = d.Product.ProductDetail;
            if (stock == null) continue;
            if (cancelling)
            {
                stock.Quantity += d.Quantity;
                stock.SoldQuantity = Math.Max(0, stock.SoldQuantity - d.Quantity);
            }
            else if (restoring)
            {
                if (stock.Quantity < d.Quantity)
                {
                    Error($"Không đủ hàng để khôi phục đơn: \"{d.Product.ProductName}\" chỉ còn {stock.Quantity}.");
                    return RedirectToAction(nameof(Details), new { id });
                }
                stock.Quantity -= d.Quantity;
                stock.SoldQuantity += d.Quantity;
            }
        }

        order.Status = status;
        await _db.SaveChangesAsync();
        Success($"Đơn hàng {order.OrderCode()} chuyển sang \"{status}\".");
        return RedirectToAction(nameof(Details), new { id });
    }
}
