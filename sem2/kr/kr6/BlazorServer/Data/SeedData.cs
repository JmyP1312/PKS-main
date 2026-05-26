using Microsoft.EntityFrameworkCore;
using Shared;

namespace BlazorServer.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var context = services.GetRequiredService<StoreDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (await context.Products.AnyAsync())
        {
            return;
        }

        context.Products.AddRange(
            new Product
            {
                Name = "Ноутбук Acer Swift",
                Category = "Электроника",
                Price = 84990,
                Stock = 7,
                Description = "Легкий ноутбук для учебы и работы"
            },
            new Product
            {
                Name = "Клавиатура Logitech K380",
                Category = "Периферия",
                Price = 4990,
                Stock = 18,
                Description = "Компактная беспроводная клавиатура"
            },
            new Product
            {
                Name = "Монитор Samsung 27",
                Category = "Электроника",
                Price = 21990,
                Stock = 5,
                Description = "IPS-монитор для рабочего места"
            });

        await context.SaveChangesAsync();
    }
}
