using System;

class MarsColonization
{
    static void Main()
    {
        Console.WriteLine("=== Колонизация Марса: Планировщик базы ===");
        Console.WriteLine("Программа для расчета максимальной толщины защиты модулей");
        Console.WriteLine();

        // Ввод данных с валидацией
        Console.Write("Введите количество модулей (n): ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
        {
            Console.WriteLine("Ошибка: введите положительное количество модулей!");
            return;
        }

        Console.Write("Введите размеры модуля (a b): ");
        string[] moduleInput = Console.ReadLine()?.Split();
        if (moduleInput?.Length != 2 || 
            !int.TryParse(moduleInput[0], out int a) || a <= 0 ||
            !int.TryParse(moduleInput[1], out int b) || b <= 0)
        {
            Console.WriteLine("Ошибка: введите два положительных числа для размеров модуля!");
            return;
        }

        Console.Write("Введите размеры поля (h w): ");
        string[] fieldInput = Console.ReadLine()?.Split();
        if (fieldInput?.Length != 2 || 
            !int.TryParse(fieldInput[0], out int h) || h <= 0 ||
            !int.TryParse(fieldInput[1], out int w) || w <= 0)
        {
            Console.WriteLine("Ошибка: введите два положительных числа для размеров поля!");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("=== ПАРАМЕТРЫ МИССИИ ===");
        Console.WriteLine($"Количество модулей: {n}");
        Console.WriteLine($"Размер модуля: {a} × {b} метров");
        Console.WriteLine($"Размер поля: {h} × {w} метров");
        Console.WriteLine($"Площадь одного модуля: {a * b} м²");
        Console.WriteLine($"Общая площадь всех модулей: {n * a * b} м²");
        Console.WriteLine($"Площадь поля: {h * w} м²");
        Console.WriteLine();

        // Вычисление максимальной толщины защиты
        int maxProtection = CalculateMaxProtection(n, a, b, h, w);

        Console.WriteLine("=== РЕЗУЛЬТАТЫ РАСЧЕТА ===");
        if (maxProtection == -1)
        {
            Console.WriteLine("❌ НЕВОЗМОЖНО разместить модули на данном поле!");
            Console.WriteLine();
            Console.WriteLine("Анализ проблемы:");
            
            // Анализируем, почему невозможно
            bool canFitOrientation1 = h >= a && w >= b;
            bool canFitOrientation2 = h >= b && w >= a;
            
            if (!canFitOrientation1 && !canFitOrientation2)
            {
                Console.WriteLine("• Даже один модуль не помещается на поле в любой ориентации");
                Console.WriteLine($"  Требуется поле минимум {Math.Max(a, b)} × {Math.Min(a, b)} метров");
            }
            else
            {
                int maxModules1 = canFitOrientation1 ? (h / a) * (w / b) : 0;
                int maxModules2 = canFitOrientation2 ? (h / b) * (w / a) : 0;
                int maxPossible = Math.Max(maxModules1, maxModules2);
                
                Console.WriteLine($"• Максимальное количество модулей на поле: {maxPossible}");
                Console.WriteLine($"• Требуется: {n}, доступно: {maxPossible}");
                Console.WriteLine($"• Недостает места для {n - maxPossible} модулей");
            }
        }
        else
        {
            Console.WriteLine($"🎯 Максимальная толщина защиты: {maxProtection} метров");
            
            if (maxProtection == 0)
            {
                Console.WriteLine("⚠️  Защитный слой добавить нельзя, модули помещаются впритык");
            }
            else
            {
                Console.WriteLine($"✅ Можно добавить защитный слой толщиной до {maxProtection} метров");
            }
            
            // Подробный анализ размещения
            Console.WriteLine();
            AnalyzePlacement(n, a, b, h, w, maxProtection);
            
            // Показываем несколько вариантов защиты
            Console.WriteLine();
            Console.WriteLine("=== ВАРИАНТЫ ЗАЩИТЫ ===");
            for (int d = 0; d <= Math.Min(maxProtection, 5); d++)
            {
                ShowProtectionVariant(n, a, b, h, w, d);
            }
        }

        // Рекомендации
        Console.WriteLine();
        Console.WriteLine("=== РЕКОМЕНДАЦИИ ===");
        if (maxProtection == -1)
        {
            Console.WriteLine("• Увеличить размеры поля");
            Console.WriteLine("• Уменьшить количество модулей");
            Console.WriteLine("• Рассмотреть модули меньшего размера");
        }
        else if (maxProtection == 0)
        {
            Console.WriteLine("• Рассмотреть увеличение размеров поля для добавления защиты");
            Console.WriteLine("• Возможно, уменьшить количество модулей для освобождения места");
        }
        else
        {
            Console.WriteLine($"• Оптимальная толщина защиты: {maxProtection} метров");
            Console.WriteLine("• База будет надежно защищена от марсианских условий");
            if (maxProtection >= 3)
            {
                Console.WriteLine("• Высокий уровень защиты обеспечит безопасность колонистов");
            }
        }
    }

    static int CalculateMaxProtection(int n, int a, int b, int h, int w)
    {
        // Проверяем возможность размещения без защиты
        if (!CanPlaceModules(n, a, b, h, w, 0))
            return -1;

        // Бинарный поиск максимальной толщины
        int left = 0;
        int right = Math.Min(h, w) / 2; // Максимально возможная толщина
        int result = 0;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            if (CanPlaceModules(n, a, b, h, w, mid))
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }

    static bool CanPlaceModules(int n, int a, int b, int h, int w, int d)
    {
        // Размеры модуля с защитой
        int aWithProtection = a + 2 * d;
        int bWithProtection = b + 2 * d;

        // Проверяем оба варианта ориентации
        bool orientation1 = h >= aWithProtection && w >= bWithProtection && 
                           CanFit(n, aWithProtection, bWithProtection, h, w);
        bool orientation2 = h >= bWithProtection && w >= aWithProtection && 
                           CanFit(n, bWithProtection, aWithProtection, h, w);

        return orientation1 || orientation2;
    }

    static bool CanFit(int n, int moduleH, int moduleW, int fieldH, int fieldW)
    {
        int maxRows = fieldH / moduleH;
        int maxCols = fieldW / moduleW;
        return maxRows * maxCols >= n;
    }

    static void AnalyzePlacement(int n, int a, int b, int h, int w, int d)
    {
        int aWithProtection = a + 2 * d;
        int bWithProtection = b + 2 * d;

        Console.WriteLine("=== АНАЛИЗ РАЗМЕЩЕНИЯ ===");

        // Вариант 1: a×b ориентация
        if (h >= aWithProtection && w >= bWithProtection)
        {
            int rows1 = h / aWithProtection;
            int cols1 = w / bWithProtection;
            int modules1 = rows1 * cols1;
            
            Console.WriteLine($"Вариант 1 ({aWithProtection}×{bWithProtection}):");
            Console.WriteLine($"  Рядов: {rows1}, Колонок: {cols1}");
            Console.WriteLine($"  Помещается модулей: {modules1}");
            
            if (modules1 >= n)
            {
                Console.WriteLine($"  ✅ Достаточно места (нужно {n})");
                Console.WriteLine($"  Неиспользуемое пространство: {(h % aWithProtection) * w + (w % bWithProtection) * h - (h % aWithProtection) * (w % bWithProtection)} м²");
            }
        }

        // Вариант 2: b×a ориентация
        if (h >= bWithProtection && w >= aWithProtection)
        {
            int rows2 = h / bWithProtection;
            int cols2 = w / aWithProtection;
            int modules2 = rows2 * cols2;
            
            Console.WriteLine($"Вариант 2 ({bWithProtection}×{aWithProtection}):");
            Console.WriteLine($"  Рядов: {rows2}, Колонок: {cols2}");
            Console.WriteLine($"  Помещается модулей: {modules2}");
            
            if (modules2 >= n)
            {
                Console.WriteLine($"  ✅ Достаточно места (нужно {n})");
                Console.WriteLine($"  Неиспользуемое пространство: {(h % bWithProtection) * w + (w % aWithProtection) * h - (h % bWithProtection) * (w % aWithProtection)} м²");
            }
        }
    }

    static void ShowProtectionVariant(int n, int a, int b, int h, int w, int d)
    {
        if (!CanPlaceModules(n, a, b, h, w, d))
            return;

        int moduleArea = (a + 2 * d) * (b + 2 * d);
        int protectionArea = moduleArea - a * b;
        
        Console.WriteLine($"Защита {d}м: модуль {a + 2 * d}×{b + 2 * d}м, " +
                         $"защитная площадь {protectionArea}м² на модуль");
    }
}