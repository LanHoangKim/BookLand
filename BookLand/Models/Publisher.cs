using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string PublisherName { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
