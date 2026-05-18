using kr5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kr5.Controllers.Api;

[ApiController]
[Route("api/calculate")]
public class CalculateController(ProductionDbContext context) : ControllerBase
{
    [HttpPost("production")]
    public async Task<IActionResult> Production(ProductionCalculateRequest request)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProductId);
        if (product is null)
        {
            return NotFound();
        }

        var efficiency = request.EfficiencyFactor is null ? 1f : Math.Clamp(request.EfficiencyFactor.Value, 0.5f, 2.0f);
        var minutes = (request.Quantity * product.ProductionTimePerUnit) / efficiency;

        return Ok(new
        {
            Product = product.Name,
            request.Quantity,
            EfficiencyFactor = efficiency,
            Minutes = Math.Ceiling(minutes),
            Hours = Math.Round(minutes / 60, 2)
        });
    }
}

public record ProductionCalculateRequest(int ProductId, int Quantity, float? EfficiencyFactor);
