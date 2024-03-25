using System;
using System.Collections.Generic;

namespace PracticeWEB.Models;

public partial class User
{
    public int? IdUser { get; set; }

    public string? NameUser { get; set; } = null!;

    public string LoginUser { get; set; } = null!;

    public string PasswordUser { get; set; } = null!;

    public int? RoleUser { get; set; }

    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();

    public virtual Role? RoleUserNavigation { get; set; } = null!;
}
