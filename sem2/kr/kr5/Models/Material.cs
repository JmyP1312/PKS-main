namespace kr5.Models;

public class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string UnitOfMeasure { get; set; } = string.Empty;

    public decimal MinimalStock { get; set; }

    public bool IsLowStock => Quantity <= MinimalStock;

    public List<ProductMaterial> ProductMaterials { get; set; } = [];
}
