using System;
using System.Collections.Generic;

namespace PracticeWEB.Models;

public partial class Category
{
    public int IdCategory { get; set; }

    public string NameCategory { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
