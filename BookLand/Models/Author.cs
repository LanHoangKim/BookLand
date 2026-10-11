using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? Nationality { get; set; }

    public int? BirthYear { get; set; }

    public string? Biography { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
