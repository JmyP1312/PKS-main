using kr5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kr5.Controllers.Api;

[ApiController]
[Route("api/lines")]
public class LinesController(ProductionDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool available = false)
    {
        var query = context.ProductionLines
            .Include(x => x.CurrentWorkOrder)
            .ThenInclude(x => x!.Product)
            .AsNoTracking();

        if (available)
        {
            query = query.Where(x => x.Status == "Active" && x.CurrentWorkOrderId == null);
        }

        var lines = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Status,
                x.EfficiencyFactor,
                x.IsAutomatic,
                x.CurrentWorkOrderId,
                CurrentProduct = x.CurrentWorkOrder == null ? null : x.CurrentWorkOrder.Product.Name,
                Progress = x.CurrentWorkOrder == null ? 0 : x.CurrentWorkOrder.ProgressPercent
            })
            .ToListAsync();

        return Ok(lines);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> Status(int id, LineStatusRequest request)
    {
        var line = await context.ProductionLines.FindAsync(id);
        if (line is null)
        {
            return NotFound();
        }

        if (request.Status is not ("Active" or "Stopped"))
        {
            return BadRequest("Статус должен быть Active или Stopped.");
        }

        line.Status = request.Status;
        if (line.Status == "Stopped")
        {
            line.CurrentWorkOrderId = null;
        }

        await context.SaveChangesAsync();
        return Ok(line);
    }

    [HttpPut("{id:int}/efficiency")]
    public async Task<IActionResult> Efficiency(int id, EfficiencyRequest request)
    {
        var line = await context.ProductionLines.FindAsync(id);
        if (line is null)
        {
            return NotFound();
        }

        line.EfficiencyFactor = Math.Clamp(request.EfficiencyFactor, 0.5f, 2.0f);
        await context.SaveChangesAsync();
        return Ok(line);
    }

    [HttpPut("{id:int}/automatic")]
    public async Task<IActionResult> SetAutomatic(int id, AutomaticRequest request)
    {
        var line = await context.ProductionLines.FindAsync(id);
        if (line is null)
        {
            return NotFound();
        }

        line.IsAutomatic = request.IsAutomatic;
        await context.SaveChangesAsync();
        return Ok(line);
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> Schedule(int id)
    {
        var exists = await context.ProductionLines.AnyAsync(x => x.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        var orders = await context.WorkOrders
            .AsNoTracking()
            .Where(x => x.ProductionLineId == id && x.Status != "Cancelled")
            .OrderBy(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                Product = x.Product.Name,
                x.Quantity,
                x.Status,
                x.StartDate,
                x.EstimatedEndDate,
                x.ProgressPercent
            })
            .ToListAsync();

        return Ok(orders);
    }
}

public record LineStatusRequest(string Status);
public record EfficiencyRequest(float EfficiencyFactor);
public record AutomaticRequest(bool IsAutomatic);
