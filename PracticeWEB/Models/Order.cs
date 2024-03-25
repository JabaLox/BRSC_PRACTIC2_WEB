using System;
using System.Collections.Generic;

namespace PracticeWEB.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public int UserOrder { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual User UserOrderNavigation { get; set; } = null!;
}
