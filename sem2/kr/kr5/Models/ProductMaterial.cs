namespace kr5.Models;

public class ProductMaterial
{
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int MaterialId { get; set; }

    public Material Material { get; set; } = null!;

    public decimal QuantityNeeded { get; set; }
}
