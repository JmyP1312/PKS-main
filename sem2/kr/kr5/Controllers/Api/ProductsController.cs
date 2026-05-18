using kr5.Data;
using kr5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kr5.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductionDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category, [FromQuery] string? search)
    {
        var query = context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search));
        }

        var products = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                ProdTime = x.ProductionTimePerUnit,
                x.Category,
                x.MinimalStock,
                x.Description,
                x.Specifications
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await context.Products
            .AsNoTracking()
            .Select(x => x.Category)
            .Where(x => x != "")
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}/materials")]
    public async Task<IActionResult> Materials(int id)
    {
        var exists = await context.Products.AnyAsync(x => x.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        var materials = await context.ProductMaterials
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .Select(x => new
            {
                x.MaterialId,
                x.Material.Name,
                x.Material.UnitOfMeasure,
                x.QuantityNeeded,
                Available = x.Material.Quantity
            })
            .ToListAsync();

        return Ok(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.ProdTime <= 0)
        {
            return BadRequest("Укажите название и положительное время производства.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            ProductionTimePerUnit = request.ProdTime,
            Category = request.Category.Trim(),
            Description = request.Description ?? string.Empty,
            Specifications = string.IsNullOrWhiteSpace(request.Specifications) ? "{}" : request.Specifications,
            MinimalStock = request.MinimalStock
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        foreach (var material in request.Materials.Where(x => x.QuantityNeeded > 0))
        {
            context.ProductMaterials.Add(new ProductMaterial
            {
                ProductId = product.Id,
                MaterialId = material.MaterialId,
                QuantityNeeded = material.QuantityNeeded
            });
        }

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }
}

public record ProductCreateRequest(
    string Name,
    int ProdTime,
    string Category,
    string? Description,
    string? Specifications,
    int MinimalStock,
    List<ProductMaterialRequest> Materials);

public record ProductMaterialRequest(int MaterialId, decimal QuantityNeeded);
