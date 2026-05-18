using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Программа проверки счастливых билетов ===");
        Console.WriteLine("Билет считается счастливым, если сумма первых трёх цифр");
        Console.WriteLine("равна сумме последних трёх цифр.");
        Console.WriteLine();

        Console.Write("Введите шестизначный номер билета: ");
        
        // Проверяем корректность ввода
        if (!int.TryParse(Console.ReadLine(), out int ticketNumber))
        {
            Console.WriteLine("Ошибка: введите корректное число!");
            return;
        }

        // Проверяем, что число шестизначное
        if (ticketNumber < 100000 || ticketNumber > 999999)
        {
            Console.WriteLine("Ошибка: номер билета должен быть шестизначным (от 100000 до 999999)!");
            return;
        }

        // Извлекаем отдельные цифры числа, используя только математические операции
        int digit1 = ticketNumber / 100000;           // первая цифра
        int digit2 = (ticketNumber / 10000) % 10;     // вторая цифра
        int digit3 = (ticketNumber / 1000) % 10;      // третья цифра
        int digit4 = (ticketNumber / 100) % 10;       // четвертая цифра
        int digit5 = (ticketNumber / 10) % 10;        // пятая цифра
        int digit6 = ticketNumber % 10;               // шестая цифра

        // Вычисляем суммы первых трёх и последних трёх цифр
        int sumFirst = digit1 + digit2 + digit3;
        int sumLast = digit4 + digit5 + digit6;

        // Выводим детальную информацию
        Console.WriteLine();
        Console.WriteLine($"Номер билета: {ticketNumber}");
        Console.WriteLine($"Первые три цифры: {digit1} + {digit2} + {digit3} = {sumFirst}");
        Console.WriteLine($"Последние три цифры: {digit4} + {digit5} + {digit6} = {sumLast}");
        Console.WriteLine();

        // Проверяем и выводим результат
        if (sumFirst == sumLast)
        {
            Console.WriteLine("🎉 Билет счастливый!");
        }
        else
        {
            Console.WriteLine("😔 Билет обычный.");
        }

        // Дополнительные примеры для демонстрации
        Console.WriteLine("\n=== Примеры счастливых билетов ===");
        TestTicket(777777);
        TestTicket(255642);
        TestTicket(123321);
        
        Console.WriteLine("\n=== Примеры обычных билетов ===");
        TestTicket(123456);
        TestTicket(111222);
        TestTicket(987654);
    }

    // Вспомогательный метод для тестирования билетов
    static void TestTicket(int ticketNumber)
    {
        // Извлекаем цифры
        int digit1 = ticketNumber / 100000;
        int digit2 = (ticketNumber / 10000) % 10;
        int digit3 = (ticketNumber / 1000) % 10;
        int digit4 = (ticketNumber / 100) % 10;
        int digit5 = (ticketNumber / 10) % 10;
        int digit6 = ticketNumber % 10;

        // Вычисляем суммы
        int sumFirst = digit1 + digit2 + digit3;
        int sumLast = digit4 + digit5 + digit6;

        // Выводим результат
        string result = (sumFirst == sumLast) ? "счастливый" : "обычный";
        Console.WriteLine($"{ticketNumber}: {sumFirst} = {sumLast} -> {result}");
    }
}