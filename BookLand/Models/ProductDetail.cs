using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class ProductDetail
{
    public int ProductDetailId { get; set; }

    public int ProductId { get; set; }

    public string? Sku { get; set; }

    public string? Translator { get; set; }

    public int? PublishYear { get; set; }

    public string? Language { get; set; }

    public int? Weight { get; set; }

    public string? Dimensions { get; set; }

    public int? PageCount { get; set; }

    public int? CoverTypeId { get; set; }

    public int Quantity { get; set; }

    public int SoldQuantity { get; set; }

    public virtual CoverType? CoverType { get; set; }

    public virtual Product Product { get; set; } = null!;
}
