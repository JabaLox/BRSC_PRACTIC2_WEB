using System;
using System.Collections.Generic;

namespace PracticeWEB.Models;

public partial class Manufacture
{
    public int IdManufacture { get; set; }

    public string NameManufacture { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
