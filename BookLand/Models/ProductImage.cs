using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class ProductImage
{
    public int ProductImageId { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
