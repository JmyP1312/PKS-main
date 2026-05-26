using System.ComponentModel.DataAnnotations;

namespace Shared;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название товара")]
    [StringLength(100, ErrorMessage = "Название не должно быть длиннее 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите категорию")]
    [StringLength(60, ErrorMessage = "Категория не должна быть длиннее 60 символов")]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "Цена должна быть больше 0")]
    public decimal Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Количество не может быть отрицательным")]
    public int Stock { get; set; }

    [StringLength(500, ErrorMessage = "Описание не должно быть длиннее 500 символов")]
    public string? Description { get; set; }
}
