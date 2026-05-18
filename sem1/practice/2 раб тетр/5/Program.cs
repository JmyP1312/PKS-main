using System;

class CoffeeMachine
{
    // Параметры напитков
    private const int AMERICANO_WATER = 300;
    private const int AMERICANO_MILK = 0;
    private const int AMERICANO_PRICE = 150;

    private const int LATTE_WATER = 30;
    private const int LATTE_MILK = 270;
    private const int LATTE_PRICE = 170;

    // Текущие запасы
    private int water;
    private int milk;

    // Статистика
    private int americanoCount = 0;
    private int latteCount = 0;
    private int totalEarnings = 0;

    public void Start()
    {
        Console.WriteLine("=== Кофейный аппарат ===");
        Console.WriteLine("Добро пожаловать! Настройка аппарата...");
        Console.WriteLine();

        // Запрос начальных запасов
        Console.Write("Введите количество воды (мл): ");
        if (!int.TryParse(Console.ReadLine(), out water) || water < 0)
        {
            Console.WriteLine("Ошибка: введите корректное количество воды!");
            return;
        }

        Console.Write("Введите количество молока (мл): ");
        if (!int.TryParse(Console.ReadLine(), out milk) || milk < 0)
        {
            Console.WriteLine("Ошибка: введите корректное количество молока!");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Кофейный аппарат готов к работе!");
        ShowMenu();

        // Основной цикл обслуживания
        while (CanMakeAnyDrink())
        {
            ProcessOrder();
        }

        // Вывод итогового отчета
        ShowFinalReport();
    }

    private void ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== МЕНЮ ===");
        Console.WriteLine($"1. Американо - {AMERICANO_PRICE} руб. (вода: {AMERICANO_WATER} мл)");
        Console.WriteLine($"2. Латте - {LATTE_PRICE} руб. (вода: {LATTE_WATER} мл, молоко: {LATTE_MILK} мл)");
        Console.WriteLine("0. Показать остатки");
        Console.WriteLine();
    }

    private void ProcessOrder()
    {
        Console.Write("Выберите напиток (1 - Американо, 2 - Латте, 0 - остатки): ");
        
        if (!int.TryParse(Console.ReadLine(), out int choice))
        {
            Console.WriteLine("Ошибка: введите корректный номер!");
            return;
        }

        switch (choice)
        {
            case 1:
                MakeAmericano();
                break;
            case 2:
                MakeLatte();
                break;
            case 0:
                ShowCurrentResources();
                break;
            default:
                Console.WriteLine("Неверный выбор! Попробуйте еще раз.");
                break;
        }
    }

    private void MakeAmericano()
    {
        if (water >= AMERICANO_WATER)
        {
            water -= AMERICANO_WATER;
            americanoCount++;
            totalEarnings += AMERICANO_PRICE;
            
            Console.WriteLine("☕ Ваш американо готов!");
            Console.WriteLine($"💰 Стоимость: {AMERICANO_PRICE} руб.");
            ShowCurrentResources();
        }
        else
        {
            Console.WriteLine("❌ Не хватает воды для приготовления американо!");
            Console.WriteLine($"   Требуется: {AMERICANO_WATER} мл, доступно: {water} мл");
        }
    }

    private void MakeLatte()
    {
        if (water >= LATTE_WATER && milk >= LATTE_MILK)
        {
            water -= LATTE_WATER;
            milk -= LATTE_MILK;
            latteCount++;
            totalEarnings += LATTE_PRICE;
            
            Console.WriteLine("🥛 Ваш латте готов!");
            Console.WriteLine($"💰 Стоимость: {LATTE_PRICE} руб.");
            ShowCurrentResources();
        }
        else
        {
            if (water < LATTE_WATER)
            {
                Console.WriteLine("❌ Не хватает воды для приготовления латте!");
                Console.WriteLine($"   Требуется: {LATTE_WATER} мл, доступно: {water} мл");
            }
            if (milk < LATTE_MILK)
            {
                Console.WriteLine("❌ Не хватает молока для приготовления латте!");
                Console.WriteLine($"   Требуется: {LATTE_MILK} мл, доступно: {milk} мл");
            }
        }
    }

    private void ShowCurrentResources()
    {
        Console.WriteLine();
        Console.WriteLine("📊 Текущие остатки:");
        Console.WriteLine($"   Вода: {water} мл");
        Console.WriteLine($"   Молоко: {milk} мл");
        Console.WriteLine();
    }

    private bool CanMakeAnyDrink()
    {
        bool canMakeAmericano = water >= AMERICANO_WATER;
        bool canMakeLatte = water >= LATTE_WATER && milk >= LATTE_MILK;
        
        return canMakeAmericano || canMakeLatte;
    }

    private void ShowFinalReport()
    {
        Console.WriteLine();
        Console.WriteLine("🚫 Ингредиенты подошли к концу!");
        Console.WriteLine();
        Console.WriteLine("=== ИТОГОВЫЙ ОТЧЕТ СМЕНЫ ===");
        Console.WriteLine($"📊 Остатки в аппарате:");
        Console.WriteLine($"   Вода: {water} мл");
        Console.WriteLine($"   Молоко: {milk} мл");
        Console.WriteLine();
        Console.WriteLine($"☕ Приготовлено напитков:");
        Console.WriteLine($"   Американо: {americanoCount} чашек");
        Console.WriteLine($"   Латте: {latteCount} чашек");
        Console.WriteLine($"   Всего: {americanoCount + latteCount} чашек");
        Console.WriteLine();
        Console.WriteLine($"💰 Финансы:");
        Console.WriteLine($"   Доход от американо: {americanoCount * AMERICANO_PRICE} руб.");
        Console.WriteLine($"   Доход от латте: {latteCount * LATTE_PRICE} руб.");
        Console.WriteLine($"   ИТОГОВЫЙ ЗАРАБОТОК: {totalEarnings} руб.");
        Console.WriteLine();
        
        // Дополнительная статистика
        if (americanoCount + latteCount > 0)
        {
            double avgPrice = (double)totalEarnings / (americanoCount + latteCount);
            Console.WriteLine($"📈 Дополнительная статистика:");
            Console.WriteLine($"   Средняя цена напитка: {avgPrice:F2} руб.");
            Console.WriteLine($"   Популярность американо: {(double)americanoCount / (americanoCount + latteCount) * 100:F1}%");
            Console.WriteLine($"   Популярность латте: {(double)latteCount / (americanoCount + latteCount) * 100:F1}%");
        }
        
        Console.WriteLine();
        Console.WriteLine("Спасибо за использование кофейного аппарата! 👋");
    }

    static void Main()
    {
        CoffeeMachine machine = new CoffeeMachine();
        machine.Start();
    }
}