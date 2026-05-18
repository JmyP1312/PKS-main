using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Программа сокращения дробей ===");
        Console.WriteLine("Программа приводит дробь M/N к несократимому виду");
        Console.WriteLine();

        // Ввод числителя
        Console.Write("Введите числитель M: ");
        if (!int.TryParse(Console.ReadLine(), out int m))
        {
            Console.WriteLine("Ошибка: введите корректное целое число для числителя!");
            return;
        }

        // Ввод знаменателя
        Console.Write("Введите знаменатель N: ");
        if (!int.TryParse(Console.ReadLine(), out int n))
        {
            Console.WriteLine("Ошибка: введите корректное целое число для знаменателя!");
            return;
        }

        // Проверка на ноль в знаменателе
        if (n == 0)
        {
            Console.WriteLine("Ошибка: знаменатель не может быть равен нулю!");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Исходная дробь: {m}/{n}");

        // Обработка особых случаев
        if (m == 0)
        {
            Console.WriteLine("Результат: 0/1 (числитель равен нулю)");
            return;
        }

        // Работа с отрицательными числами
        // Выносим знак в числитель для стандартного представления
        bool isNegative = (m < 0) ^ (n < 0); // XOR для определения знака результата
        int absM = Math.Abs(m);
        int absN = Math.Abs(n);

        // Находим НОД для абсолютных значений
        int gcd = FindGCD(absM, absN);

        // Сокращаем дробь
        int numerator = absM / gcd;
        int denominator = absN / gcd;

        // Применяем знак к числителю
        if (isNegative)
            numerator = -numerator;

        // Выводим подробную информацию о процессе
        Console.WriteLine($"НОД({absM}, {absN}) = {gcd}");
        Console.WriteLine($"Сокращение: {m}/{n} = {numerator}/{denominator}");
        Console.WriteLine();

        // Выводим результат
        if (denominator == 1)
        {
            Console.WriteLine($"Результат: {numerator} (целое число)");
        }
        else
        {
            Console.WriteLine($"Несократимая дробь: {numerator}/{denominator}");
        }

        // Дополнительная информация
        if (gcd == 1)
        {
            Console.WriteLine("Примечание: дробь уже была несократимой");
        }
        else
        {
            Console.WriteLine($"Примечание: дробь была сокращена на {gcd}");
        }

        // Десятичное представление
        double decimalValue = (double)numerator / denominator;
        Console.WriteLine($"Десятичное значение: {decimalValue:F6}");

        // Демонстрация примеров
        Console.WriteLine("\n=== Примеры сокращения дробей ===");
        DemonstrateFraction(12, 18);
        DemonstrateFraction(5, 7);
        DemonstrateFraction(-8, 12);
        DemonstrateFraction(15, -25);
        DemonstrateFraction(100, 50);
        DemonstrateFraction(0, 5);
    }

    // Метод для нахождения НОД (алгоритм Евклида)
    static int FindGCD(int a, int b)
    {
        // Работаем с абсолютными значениями
        a = Math.Abs(a);
        b = Math.Abs(b);

        // Классический алгоритм Евклида
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    // Вспомогательный метод для демонстрации примеров
    static void DemonstrateFraction(int m, int n)
    {
        if (n == 0)
        {
            Console.WriteLine($"{m}/{n} -> недопустимо (деление на ноль)");
            return;
        }

        if (m == 0)
        {
            Console.WriteLine($"{m}/{n} -> 0");
            return;
        }

        bool isNegative = (m < 0) ^ (n < 0);
        int absM = Math.Abs(m);
        int absN = Math.Abs(n);
        int gcd = FindGCD(absM, absN);
        
        int numerator = absM / gcd;
        int denominator = absN / gcd;
        
        if (isNegative)
            numerator = -numerator;

        if (denominator == 1)
        {
            Console.WriteLine($"{m}/{n} -> {numerator}");
        }
        else
        {
            Console.WriteLine($"{m}/{n} -> {numerator}/{denominator}");
        }
    }
}