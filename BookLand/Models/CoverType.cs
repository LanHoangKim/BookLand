using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class CoverType
{
    public int CoverTypeId { get; set; }

    public string CoverTypeName { get; set; } = null!;

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
}
