using BookLand.Areas.Admin.Models;
using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

// Trang tổng quan
public class HomeController : AdminBaseController
{
    private readonly BookLandContext _db;

    public HomeController(BookLandContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var validOrders = _db.Orders.Where(o => o.Status != OrderStatuses.Cancelled);
        var vm = new DashboardViewModel
        {
            Revenue = await validOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            OrderCount = await _db.Orders.CountAsync(),
            PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatuses.Pending),
            CustomerCount = await _db.Customers.CountAsync(),
            ProductCount = await _db.Products.CountAsync(),
            OutOfStockCount = await _db.ProductDetails.CountAsync(d => d.Quantity == 0),
            RecentOrders = await _db.Orders.AsNoTracking().Include(o => o.Customer).Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate).Take(6).ToListAsync(),
            TopSelling = await _db.Products.AsNoTracking().Include(p => p.ProductDetail)
                .OrderByDescending(p => p.ProductDetail!.SoldQuantity).Take(5).ToListAsync(),
            LowStock = await _db.Products.AsNoTracking().Include(p => p.ProductDetail)
                .Where(p => p.ProductDetail != null && p.ProductDetail.Quantity <= 10)
                .OrderBy(p => p.ProductDetail!.Quantity).Take(6).ToListAsync()
        };

        // Doanh thu 6 tháng gần nhất
        var from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        var monthly = await validOrders.Where(o => o.OrderDate >= from)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync();
        for (int i = 0; i < 6; i++)
        {
            var m = from.AddMonths(i);
            vm.RevenueByMonth.Add(($"T{m.Month}/{m.Year % 100}",
                monthly.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Total ?? 0));
        }

        return View(vm);
    }
}
