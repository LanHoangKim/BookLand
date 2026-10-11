using BookLand.Areas.Admin.Models;
using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Quản lý khách hàng: chỉ xem, không có Thêm/Sửa/Xóa (khách tự đăng ký trên website)
public class CustomersController : AdminBaseController
{
    private readonly BookLandContext _db;

    public CustomersController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            string k = search.Trim();
            query = query.Where(c => c.FullName.Contains(k) || c.Email.Contains(k) || c.Phone.Contains(k));
        }

        var items = query.OrderByDescending(c => c.CreatedAt).Select(c => new CustomerIndexItem
        {
            Customer = c,
            OrderCount = c.Orders.Count,
            TotalSpent = c.Orders.Where(o => o.Status != OrderStatuses.Cancelled).Sum(o => (decimal?)o.TotalAmount) ?? 0
        });
        ViewBag.Search = search;
        return View(await PagedList<CustomerIndexItem>.CreateAsync(items, page, PageSize));
    }

    // Details: thông tin khách hàng + danh sách đơn hàng (partial _OrderListPartial)
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers.AsNoTracking()
            .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
        return customer == null ? NotFound() : View(customer);
    }
}
