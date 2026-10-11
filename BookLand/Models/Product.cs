using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public int? AuthorId { get; set; }

    public int? PublisherId { get; set; }

    public int? SupplierId { get; set; }

    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsFlashSale { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Author? Author { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ProductDetail? ProductDetail { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual Publisher? Publisher { get; set; }

    public virtual Supplier? Supplier { get; set; }
}
