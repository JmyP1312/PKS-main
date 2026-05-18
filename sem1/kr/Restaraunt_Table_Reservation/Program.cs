using RestaurantConsoleApp.Data;
using RestaurantConsoleApp.Services;
using RestaurantConsoleApp.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Инициализация базы данных
using (var db = new RestaurantDbContext())
{
    db.Database.EnsureCreated();
}

Console.Clear();
Console.WriteLine("╔════════════════════════════════════════════════╗");
Console.WriteLine("║   СИСТЕМА БРОНИРОВАНИЯ СТОЛИКОВ В РЕСТОРАНЕ   ║");
Console.WriteLine("╚════════════════════════════════════════════════╝");
Console.WriteLine();

// Главный цикл приложения
while (true)
{
    if (!AuthService.IsLoggedIn)
    {
        AuthMenu.Show();
    }
    else
    {
        MainMenu.Show();
    }
}
