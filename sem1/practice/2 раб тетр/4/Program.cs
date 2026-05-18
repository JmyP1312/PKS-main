using System;

class NumberGuesser
{
    static void Main()
    {
        Console.WriteLine("=== Игра 'Угадай число' ===");
        Console.WriteLine("Загадайте число от 0 до 63.");
        Console.WriteLine("Я угадаю его максимум за 7 вопросов!");
        Console.WriteLine("Отвечайте '1' (да) или '0' (нет) на мои вопросы.");
        Console.WriteLine();

        // Используем битовые маски для проверки каждого бита
        int[] bitPositions = { 32, 16, 8, 4, 2, 1 }; // 2^5, 2^4, 2^3, 2^2, 2^1, 2^0
        int result = 0;
        int questionNumber = 1;

        Console.WriteLine("Начинаем угадывать...");
        Console.WriteLine();

        // Проверяем каждый бит числа от старшего к младшему
        foreach (int bitValue in bitPositions)
        {
            Console.Write($"Вопрос {questionNumber}: Ваше число больше или равно {result + bitValue}? ");
            Console.Write("(1 - да, 0 - нет): ");
            
            string answer = Console.ReadLine();
            
            // Валидация ввода
            while (answer != "1" && answer != "0")
            {
                Console.Write("Пожалуйста, введите '1' (да) или '0' (нет): ");
                answer = Console.ReadLine();
            }

            if (answer == "1")
            {
                result |= bitValue; // Устанавливаем соответствующий бит в 1
                Console.WriteLine($"   → Устанавливаю бит {GetBitPosition(bitValue)} в 1");
            }
            else
            {
                Console.WriteLine($"   → Бит {GetBitPosition(bitValue)} остается 0");
            }

            Console.WriteLine($"   → Текущее предположение: {result}");
            Console.WriteLine();
            
            questionNumber++;
        }

        Console.WriteLine("🎯 Угадывание завершено!");
        Console.WriteLine($"Ваше число: {result}");
        Console.WriteLine();
        
        // Проверяем правильность
        Console.Write("Я угадал правильно? (1 - да, 0 - нет): ");
        string verification = Console.ReadLine();
        
        if (verification == "1")
        {
            Console.WriteLine("🎉 Отлично! Спасибо за игру!");
        }
        else
        {
            Console.WriteLine("😞 Что-то пошло не так. Возможно, была ошибка в ответах.");
        }

        // Демонстрация работы алгоритма
        Console.WriteLine();
        Console.WriteLine("=== Как работает алгоритм ===");
        Console.WriteLine("Алгоритм использует двоичное представление числа.");
        Console.WriteLine("Каждый вопрос определяет значение одного бита:");
        Console.WriteLine("- Бит 5 (32): числа 32-63");
        Console.WriteLine("- Бит 4 (16): числа 16-31, 48-63");
        Console.WriteLine("- Бит 3 (8):  числа 8-15, 24-31, 40-47, 56-63");
        Console.WriteLine("- Бит 2 (4):  числа 4-7, 12-15, 20-23, ...");
        Console.WriteLine("- Бит 1 (2):  числа 2-3, 6-7, 10-11, ...");
        Console.WriteLine("- Бит 0 (1):  нечетные числа");
        Console.WriteLine();
        Console.WriteLine($"Ваше число {result} в двоичном виде: {Convert.ToString(result, 2).PadLeft(6, '0')}");
    }

    // Вспомогательный метод для определения позиции бита
    static int GetBitPosition(int bitValue)
    {
        return bitValue switch
        {
            32 => 5,
            16 => 4,
            8 => 3,
            4 => 2,
            2 => 1,
            1 => 0,
            _ => -1
        };
    }
}