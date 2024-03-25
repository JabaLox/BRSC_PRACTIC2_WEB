using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PracticeWEB.Models;

public partial class Product
{
    public string? ArticleProduct { get; set; } = null!;

    public string? NameProduct { get; set; } = null!;

    public string? DesriptionProduct { get; set; } = null!;

    [Required(ErrorMessage = "Цена обязательна")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Введите корректную цену")]
    public decimal? CostProduct { get; set; }

    public int? Category { get; set; }

    public int? Manufacture { get; set; }

    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Ввод только цифрами!")]
    public int? DiscountProduct { get; set; }

    public int? CountInStockProduct { get; set; }

    public virtual Category? CategoryNavigation { get; set; } = null!;

    public virtual Manufacture? ManufactureNavigation { get; set; } = null!;

    public virtual ICollection<OrderProduct>? OrderProducts { get; set; } = new List<OrderProduct>();
}
