using BlazorServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace BlazorServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(StoreDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var products = await context.Products
            .OrderBy(product => product.Name)
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("Идентификатор товара не совпадает с маршрутом.");
        }

        var existingProduct = await context.Products.FindAsync(id);
        if (existingProduct is null)
        {
            return NotFound();
        }

        existingProduct.Name = product.Name;
        existingProduct.Category = product.Category;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.Description = product.Description;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut]
    public Task<IActionResult> UpdateProduct(Product product)
    {
        return UpdateProduct(product.Id, product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
