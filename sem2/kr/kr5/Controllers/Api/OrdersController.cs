using kr5.Data;
using kr5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kr5.Controllers.Api;

[ApiController]
[Route("api/orders")]
public class OrdersController(ProductionDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] string? date)
    {
        var query = context.WorkOrders
            .Include(x => x.Product)
            .Include(x => x.ProductionLine)
            .AsNoTracking();

        if (status == "active")
        {
            query = query.Where(x => x.Status == "Pending" || x.Status == "InProgress");
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (date == "today")
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            query = query.Where(x => x.StartDate >= today && x.StartDate < tomorrow);
        }

        var orders = await query
            .OrderByDescending(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                Product = x.Product.Name,
                Line = x.ProductionLine == null ? "Не назначена" : x.ProductionLine.Name,
                x.Quantity,
                x.Status,
                x.StartDate,
                x.EstimatedEndDate,
                x.ProgressPercent
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateRequest request)
    {
        var product = await context.Products
            .Include(x => x.ProductMaterials)
            .ThenInclude(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == request.ProductId);
        if (product is null)
        {
            return BadRequest("Продукт не найден.");
        }

        var line = request.LineId is null
            ? null
            : await context.ProductionLines.FindAsync(request.LineId.Value);

        if (request.LineId is not null && line is null)
        {
            return BadRequest("Производственная линия не найдена.");
        }

        var shortages = product.ProductMaterials
            .Where(x => x.Material.Quantity < x.QuantityNeeded * request.Quantity)
            .Select(x => $"{x.Material.Name}: нужно {x.QuantityNeeded * request.Quantity} {x.Material.UnitOfMeasure}, есть {x.Material.Quantity}")
            .ToList();

        if (shortages.Count > 0)
        {
            return BadRequest(new { message = "Недостаточно материалов.", shortages });
        }

        var efficiency = line?.EfficiencyFactor ?? 1f;
        var start = request.StartDate ?? DateTime.Now;
        var minutes = (request.Quantity * product.ProductionTimePerUnit) / efficiency;

        var order = new WorkOrder
        {
            ProductId = product.Id,
            ProductionLineId = line?.Id,
            Quantity = request.Quantity,
            StartDate = start,
            EstimatedEndDate = start.AddMinutes(minutes),
            Status = "Pending",
            ProgressPercent = 0
        };

        foreach (var material in product.ProductMaterials)
        {
            material.Material.Quantity -= material.QuantityNeeded * request.Quantity;
        }

        context.WorkOrders.Add(order);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(Details), new { id = order.Id }, new { order.Id, order.EstimatedEndDate });
    }

    [HttpPut("{id:int}/progress")]
    public async Task<IActionResult> Progress(int id, ProgressRequest request)
    {
        var order = await context.WorkOrders
            .Include(x => x.ProductionLine)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        order.ProgressPercent = Math.Clamp(request.Percent, 0, 100);
        order.Status = order.ProgressPercent >= 100 ? "Completed" : "InProgress";

        if (order.ProductionLine is not null)
        {
            order.ProductionLine.CurrentWorkOrderId = order.Status == "Completed" ? null : order.Id;
            order.ProductionLine.Status = order.Status == "Completed" ? order.ProductionLine.Status : "Active";
        }

        await context.SaveChangesAsync();
        return Ok(new { order.Id, order.Status, order.ProgressPercent });
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await context.WorkOrders
            .Include(x => x.ProductionLine)
            .Include(x => x.Product)
                .ThenInclude(x => x.ProductMaterials)
                    .ThenInclude(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        // Return materials to warehouse
        foreach (var productMaterial in order.Product.ProductMaterials)
        {
            productMaterial.Material.Quantity += productMaterial.QuantityNeeded * order.Quantity;
        }

        order.Status = "Cancelled";
        order.ProgressPercent = 0;
        if (order.ProductionLine is not null)
        {
            order.ProductionLine.CurrentWorkOrderId = null;
        }

        await context.SaveChangesAsync();
        return Ok(new { order.Id, order.Status });
    }

    [HttpPut("{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, RescheduleRequest request)
    {
        var order = await context.WorkOrders
            .Include(x => x.Product)
            .Include(x => x.ProductionLine)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        var efficiency = order.ProductionLine?.EfficiencyFactor ?? 1f;
        var minutes = (order.Quantity * order.Product.ProductionTimePerUnit) / efficiency;
        order.StartDate = request.StartDate;
        order.EstimatedEndDate = request.StartDate.AddMinutes(minutes);

        await context.SaveChangesAsync();
        return Ok(new { order.Id, order.StartDate, order.EstimatedEndDate });
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await context.WorkOrders
            .Include(x => x.Product)
                .ThenInclude(x => x.ProductMaterials)
                    .ThenInclude(x => x.Material)
            .Include(x => x.ProductionLine)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order is null) return NotFound();

        var result = new
        {
            order.Id,
            Product = order.Product.Name,
            Line = order.ProductionLine?.Name,
            order.Quantity,
            order.Status,
            order.StartDate,
            order.EstimatedEndDate,
            order.ProgressPercent,
            Materials = order.Product.ProductMaterials.Select(pm => new
            {
                pm.Material.Name,
                pm.QuantityNeeded,
                Total = pm.QuantityNeeded * order.Quantity,
                pm.Material.UnitOfMeasure
            })
        };

        return Ok(result);
    }

    [HttpPut("{id:int}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var order = await context.WorkOrders
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();

        order.Status = "InProgress";
        order.ProgressPercent = 0;

        await context.SaveChangesAsync();
        return Ok(new { order.Id, order.Status, order.ProgressPercent });
    }

    [HttpPut("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, AssignRequest request)
    {
        var line = await context.ProductionLines
            .Include(x => x.CurrentWorkOrder)
            .FirstOrDefaultAsync(x => x.Id == request.LineId);
        if (line is null) return NotFound("Линия не найдена");

        if (line.CurrentWorkOrderId != null)
            return BadRequest("Линия уже занята другим заказом");

        var order = await context.WorkOrders
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound("Заказ не найден");

        order.ProductionLineId = line.Id;
        line.CurrentWorkOrderId = order.Id;
        line.Status = "Active";

        await context.SaveChangesAsync();
        return Ok(new { order.Id, line = line.Name });
    }
}

public record OrderCreateRequest(int ProductId, int Quantity, int? LineId, DateTime? StartDate);
public record ProgressRequest(int Percent);
public record RescheduleRequest(DateTime StartDate);
public record AssignRequest(int LineId);
