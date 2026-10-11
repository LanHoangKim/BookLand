using BookLand.Helpers;
using BookLand.Models;
using BookLand.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookLand.Areas.Admin.Controllers;

public class ProductsController : AdminBaseController
{
    private readonly BookLandContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(BookLandContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Index: chỉ hiện các cột chính (ảnh, tên, giá, số lượng tồn, chủng loại)
    public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
    {
        var query = _db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.ProductDetail).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.ProductName.Contains(search.Trim()));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId || p.Category.ParentId == categoryId);

        ViewBag.Search = search;
        ViewBag.CategoryId = new SelectList(await _db.Categories.AsNoTracking().OrderBy(c => c.ParentId == null ? c.CategoryId : c.ParentId).ThenBy(c => c.ParentId).ThenBy(c => c.SortOrder)
            .Select(c => new { c.CategoryId, Name = c.ParentId == null ? c.CategoryName : "— " + c.CategoryName }).ToListAsync(),
            "CategoryId", "Name", categoryId);
        return View(await PagedList<Product>.CreateAsync(query.OrderByDescending(p => p.ProductId), page, PageSize));
    }

    // Details: dùng chung partial _ProductDetailPartial với trang khách hàng (không có nút mua)
    public async Task<IActionResult> Details(int id)
    {
        var product = await LoadFullAsync(id);
        return product == null ? NotFound() : View(product);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookupsAsync(null);
        ViewBag.Quantity = 0;
        return View(new Product { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ProductName,CategoryId,AuthorId,PublisherId,SupplierId,Price,OriginalPrice,Description,IsFeatured,IsFlashSale,IsActive")] Product product,
        IFormFile? imageFile, List<IFormFile>? galleryFiles, int quantity = 0)
    {
        if (imageFile == null) ModelState.AddModelError("imageFile", "Vui lòng chọn ảnh sản phẩm.");
        ValidateProduct(product, imageFile, galleryFiles);
        if (quantity < 0 || quantity > 100000) ModelState.AddModelError("quantity", "Số lượng tồn từ 0 đến 100.000.");

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(product);
            ViewBag.Quantity = quantity;
            return View(product);
        }

        product.ProductName = product.ProductName.Trim();
        product.CreatedAt = DateTime.Now;
        product.ImageUrl = await ImageHelper.SaveAsync(imageFile!, _env, "products");
        product.ProductDetail = new ProductDetail { Quantity = quantity, SoldQuantity = 0 };
        await AddGalleryAsync(product, galleryFiles);

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        Success($"Đã thêm sản phẩm \"{product.ProductName}\". Hãy cập nhật thông số trong mục Chi tiết sản phẩm.");
        return RedirectToAction(nameof(Details), new { id = product.ProductId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();
        await LoadLookupsAsync(product);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("ProductId,ProductName,CategoryId,AuthorId,PublisherId,SupplierId,Price,OriginalPrice,Description,IsFeatured,IsFlashSale,IsActive")] Product input,
        IFormFile? imageFile, List<IFormFile>? galleryFiles, List<int>? removeImageIds)
    {
        if (id != input.ProductId) return NotFound();
        var product = await _db.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();

        ValidateProduct(input, imageFile, galleryFiles);
        if (!ModelState.IsValid)
        {
            input.ImageUrl = product.ImageUrl;
            input.ProductImages = product.ProductImages;
            await LoadLookupsAsync(input);
            return View(input);
        }

        product.ProductName = input.ProductName.Trim();
        product.CategoryId = input.CategoryId;
        product.AuthorId = input.AuthorId;
        product.PublisherId = input.PublisherId;
        product.SupplierId = input.SupplierId;
        product.Price = input.Price;
        product.OriginalPrice = input.OriginalPrice;
        product.Description = input.Description;
        product.IsFeatured = input.IsFeatured;
        product.IsFlashSale = input.IsFlashSale;
        product.IsActive = input.IsActive;

        if (imageFile != null)
        {
            ImageHelper.DeleteUploaded(product.ImageUrl, _env);
            product.ImageUrl = await ImageHelper.SaveAsync(imageFile, _env, "products");
        }
        if (removeImageIds?.Count > 0)
        {
            foreach (var img in product.ProductImages.Where(i => removeImageIds.Contains(i.ProductImageId)).ToList())
            {
                ImageHelper.DeleteUploaded(img.ImageUrl, _env);
                _db.ProductImages.Remove(img);
            }
        }
        await AddGalleryAsync(product, galleryFiles);

        await _db.SaveChangesAsync();
        Success($"Đã cập nhật sản phẩm \"{product.ProductName}\".");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.ProductDetail)
            .Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();
        ViewBag.Blockers = await GetBlockersAsync(id);
        return View(product);
    }

    // Delete: không xóa nếu sản phẩm đã có trong đơn hàng (tránh mất dữ liệu đơn hàng liên quan)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products.Include(p => p.Category).Include(p => p.ProductDetail)
            .Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();

        var blockers = await GetBlockersAsync(id);
        if (blockers.Count > 0)
        {
            ViewBag.Blockers = blockers;
            ViewBag.Error = "Không thể xóa sản phẩm này. Bạn có thể bỏ chọn \"Đang bán\" để ẩn sản phẩm.";
            return View(product);
        }

        // Ảnh phụ và chi tiết sản phẩm là dữ liệu riêng của sản phẩm nên xóa kèm
        var files = product.ProductImages.Select(i => i.ImageUrl).Append(product.ImageUrl).ToList();
        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.ProductImages.RemoveRange(product.ProductImages);
        if (product.ProductDetail != null) _db.ProductDetails.Remove(product.ProductDetail);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        files.ForEach(f => ImageHelper.DeleteUploaded(f, _env));

        Success($"Đã xóa sản phẩm \"{product.ProductName}\".");
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetBlockersAsync(int id)
    {
        int orders = await _db.OrderDetails.Where(d => d.ProductId == id).Select(d => d.OrderId).Distinct().CountAsync();
        return orders > 0 ? [$"Sản phẩm đã có trong {orders} đơn hàng."] : [];
    }

    private void ValidateProduct(Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles)
    {
        if (product.OriginalPrice.HasValue && product.OriginalPrice < product.Price)
            ModelState.AddModelError(nameof(product.OriginalPrice), "Giá bìa phải lớn hơn hoặc bằng giá bán.");
        if (imageFile != null && ImageHelper.Validate(imageFile) is string error)
            ModelState.AddModelError("imageFile", error);
        foreach (var f in galleryFiles ?? [])
        {
            if (ImageHelper.Validate(f) is string galleryError)
            {
                ModelState.AddModelError("galleryFiles", $"{f.FileName}: {galleryError}");
            }
        }
    }

    private async Task AddGalleryAsync(Product product, List<IFormFile>? galleryFiles)
    {
        int order = product.ProductImages.Count == 0 ? 0 : product.ProductImages.Max(i => i.SortOrder);
        foreach (var f in galleryFiles ?? [])
        {
            product.ProductImages.Add(new ProductImage
            {
                ImageUrl = await ImageHelper.SaveAsync(f, _env, "products"),
                SortOrder = ++order
            });
        }
    }

    private Task<Product?> LoadFullAsync(int id) =>
        _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Include(p => p.Publisher)
            .Include(p => p.Supplier)
            .Include(p => p.ProductDetail).ThenInclude(d => d!.CoverType)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id);

    // Danh mục: chỉ chọn danh mục con, nhóm theo danh mục cha
    private async Task LoadLookupsAsync(Product? product)
    {
        var categories = await _db.Categories.AsNoTracking().Include(c => c.Parent)
            .Where(c => c.ParentId != null).OrderBy(c => c.Parent!.SortOrder).ThenBy(c => c.SortOrder).ToListAsync();
        var groups = categories.Select(c => c.Parent!).DistinctBy(p => p.CategoryId)
            .ToDictionary(p => p.CategoryId, p => new SelectListGroup { Name = p.CategoryName });
        ViewBag.CategoryId = categories.Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.CategoryName,
            Group = groups[c.ParentId!.Value],
            Selected = product?.CategoryId == c.CategoryId
        }).ToList();
        ViewBag.AuthorId = new SelectList(await _db.Authors.AsNoTracking().OrderBy(a => a.AuthorName).ToListAsync(), "AuthorId", "AuthorName", product?.AuthorId);
        ViewBag.PublisherId = new SelectList(await _db.Publishers.AsNoTracking().OrderBy(p => p.PublisherName).ToListAsync(), "PublisherId", "PublisherName", product?.PublisherId);
        ViewBag.SupplierId = new SelectList(await _db.Suppliers.AsNoTracking().OrderBy(s => s.SupplierName).ToListAsync(), "SupplierId", "SupplierName", product?.SupplierId);
    }
}
