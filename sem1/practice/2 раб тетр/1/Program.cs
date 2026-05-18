using System;
using System.Globalization;

class Program
{
    // Функция для вычисления факториала
    static double Factorial(int n)
    {
        if (n == 0) return 1;
        double result = 1;
        for (int i = 1; i <= n; i++)
            result *= i;
        return result;
    }

    // Функция для вычисления n-го члена ряда Маклорена (для e^x)
    static double GetNthTerm(double x, int n)
    {
        return Math.Pow(x, n) / Factorial(n);
    }

    // Функция для вычисления суммы ряда с заданной точностью
    static double CalculateSeries(double x, double epsilon)
    {
        double sum = 0;
        double term = 1; // Первый член ряда (x^0/0! = 1)
        int n = 0;

        while (Math.Abs(term) > epsilon)
        {
            sum += term;
            n++;
            term = Math.Pow(x, n) / Factorial(n); // Вычисление следующего члена
        }

        return sum;
    }

    static void Main()
    {
        Console.WriteLine("=== Программа для вычисления функции через ряд Маклорена ===");
        Console.WriteLine("Разложение функции e^x = 1 + x + x²/2! + x³/3! + ...");
        Console.WriteLine();

        Console.Write("Введите значение x: ");
        double x = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        // Вычисление с заданной точностью
        Console.Write("Введите точность (e < 0.01): ");
        double epsilon = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        if (epsilon >= 0.01)
        {
            Console.WriteLine("Ошибка: Точность должна быть меньше 0.01");
            return;
        }

        if (epsilon <= 0)
        {
            Console.WriteLine("Ошибка: Точность должна быть положительным числом");
            return;
        }

        double result = CalculateSeries(x, epsilon);
        double actualValue = Math.Exp(x); // Точное значение e^x для сравнения

        Console.WriteLine();
        Console.WriteLine($"Результаты вычислений:");
        Console.WriteLine($"Значение функции через ряд Маклорена: {result:F8}");
        Console.WriteLine($"Точное значение e^x: {actualValue:F8}");
        Console.WriteLine($"Абсолютная погрешность: {Math.Abs(result - actualValue):F8}");

        // Вычисление n-го члена
        Console.WriteLine();
        Console.Write("Введите номер члена ряда (n) для отдельного вычисления: ");
        int n = int.Parse(Console.ReadLine());

        if (n < 0)
        {
            Console.WriteLine("Ошибка: Номер члена ряда не может быть отрицательным");
            return;
        }

        double nthTerm = GetNthTerm(x, n);
        Console.WriteLine($"Значение {n}-го члена ряда: {nthTerm:F8}");
        Console.WriteLine($"Формула: x^{n}/{n}! = {x}^{n}/{Factorial(n)} = {nthTerm:F8}");
    }
}