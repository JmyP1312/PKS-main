using kr5.Models;
using Microsoft.EntityFrameworkCore;

namespace kr5.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var context = services.GetRequiredService<ProductionDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (await context.Products.AnyAsync())
        {
            return;
        }

        var steel = new Material { Name = "Сталь листовая", Quantity = 900, MinimalStock = 250, UnitOfMeasure = "кг" };
        var paint = new Material { Name = "Порошковая краска", Quantity = 46, MinimalStock = 60, UnitOfMeasure = "кг" };
        var controller = new Material { Name = "Контроллер PLC", Quantity = 38, MinimalStock = 20, UnitOfMeasure = "шт" };
        var cable = new Material { Name = "Кабель силовой", Quantity = 180, MinimalStock = 120, UnitOfMeasure = "м" };

        var cabinet = new Product
        {
            Name = "Шкаф управления",
            Category = "Электрооборудование",
            Description = "Сборка шкафа управления производственной линией.",
            Specifications = """{"voltage":"380V","protection":"IP54"}""",
            MinimalStock = 8,
            ProductionTimePerUnit = 95
        };

        var frame = new Product
        {
            Name = "Металлическая рама",
            Category = "Металлоконструкции",
            Description = "Несущая сварная рама для модулей линии.",
            Specifications = """{"material":"steel","finish":"powder paint"}""",
            MinimalStock = 12,
            ProductionTimePerUnit = 45
        };

        var panel = new Product
        {
            Name = "Операторская панель",
            Category = "Электрооборудование",
            Description = "Панель управления для станочного поста.",
            Specifications = """{"display":"10 inch","interface":"Ethernet"}""",
            MinimalStock = 6,
            ProductionTimePerUnit = 70
        };

        context.AddRange(steel, paint, controller, cable, cabinet, frame, panel);
        await context.SaveChangesAsync();

        context.ProductMaterials.AddRange(
            new ProductMaterial { ProductId = cabinet.Id, MaterialId = steel.Id, QuantityNeeded = 12 },
            new ProductMaterial { ProductId = cabinet.Id, MaterialId = paint.Id, QuantityNeeded = 1.6m },
            new ProductMaterial { ProductId = cabinet.Id, MaterialId = controller.Id, QuantityNeeded = 1 },
            new ProductMaterial { ProductId = cabinet.Id, MaterialId = cable.Id, QuantityNeeded = 8 },
            new ProductMaterial { ProductId = frame.Id, MaterialId = steel.Id, QuantityNeeded = 18 },
            new ProductMaterial { ProductId = frame.Id, MaterialId = paint.Id, QuantityNeeded = 2.3m },
            new ProductMaterial { ProductId = panel.Id, MaterialId = controller.Id, QuantityNeeded = 1 },
            new ProductMaterial { ProductId = panel.Id, MaterialId = cable.Id, QuantityNeeded = 4 }
        );

        var lineA = new ProductionLine { Name = "Линия A-100", Status = "Active", EfficiencyFactor = 1.15f };
        var lineB = new ProductionLine { Name = "Линия B-220", Status = "Stopped", EfficiencyFactor = 0.9f };
        var lineC = new ProductionLine { Name = "Линия C-Compact", Status = "Active", EfficiencyFactor = 1.35f };

        context.ProductionLines.AddRange(lineA, lineB, lineC);
        await context.SaveChangesAsync();
    }
}
