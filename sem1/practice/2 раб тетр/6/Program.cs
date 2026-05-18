using System;

class BacteriaLab
{
    static void Main()
    {
        Console.WriteLine("=== Лабораторный опыт: Бактерии vs Антибиотик ===");
        Console.WriteLine("Симуляция роста бактерий под воздействием антибиотика");
        Console.WriteLine();

        // Ввод начальных данных с валидацией
        Console.Write("Введите количество бактерий (N): ");
        if (!int.TryParse(Console.ReadLine(), out int N) || N <= 0)
        {
            Console.WriteLine("Ошибка: введите положительное число бактерий!");
            return;
        }

        Console.Write("Введите количество капель антибиотика (X): ");
        if (!int.TryParse(Console.ReadLine(), out int X) || X <= 0)
        {
            Console.WriteLine("Ошибка: введите положительное количество капель!");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("=== ПАРАМЕТРЫ ЭКСПЕРИМЕНТА ===");
        Console.WriteLine($"Начальное количество бактерий: {N}");
        Console.WriteLine($"Количество капель антибиотика: {X}");
        Console.WriteLine($"Эффективность антибиотика: {X * 10} бактерий в первый час");
        Console.WriteLine();
        Console.WriteLine("Правила симуляции:");
        Console.WriteLine("• Каждый час количество бактерий удваивается");
        Console.WriteLine("• Антибиотик убивает бактерии после их размножения");
        Console.WriteLine("• Эффективность антибиотика уменьшается на X каждый час");
        Console.WriteLine("• Симуляция останавливается, когда антибиотик перестает действовать");
        Console.WriteLine();

        long bacteria = N; // Используем long для больших чисел
        int hours = 0;
        int killPower = X * 10; // Начальная мощность антибиотика

        Console.WriteLine("=== ДИНАМИКА ИЗМЕНЕНИЯ ===");
        Console.WriteLine($"Час 0 (начало): Бактерий = {bacteria}");
        Console.WriteLine();

        // Основной цикл симуляции
        while (killPower > 0 && bacteria > 0)
        {
            hours++;
            
            // Показываем процесс пошагово
            Console.WriteLine($"--- Час {hours} ---");
            Console.WriteLine($"1. Размножение: {bacteria} → {bacteria * 2} (удвоение)");
            
            // Бактерии удваиваются
            bacteria *= 2;
            
            // Действие антибиотика
            long killedBacteria = Math.Min(killPower, bacteria);
            bacteria -= killedBacteria;
            
            Console.WriteLine($"2. Антибиотик убивает: {killedBacteria} бактерий");
            Console.WriteLine($"3. Осталось бактерий: {bacteria}");
            
            // Уменьшение мощности антибиотика
            killPower -= X;
            Console.WriteLine($"4. Эффективность антибиотика: {Math.Max(killPower, 0)} (уменьшилась на {X})");
            
            // Проверка на отрицательное количество бактерий
            if (bacteria < 0) bacteria = 0;
            
            Console.WriteLine();
            
            // Условие остановки - проверяем возможность продолжения
            if (bacteria == 0)
            {
                Console.WriteLine("🦠 Все бактерии уничтожены!");
                break;
            }
        }

        Console.WriteLine("=== РЕЗУЛЬТАТЫ ЭКСПЕРИМЕНТА ===");
        Console.WriteLine($"Продолжительность: {hours} часов");
        Console.WriteLine($"Конечное количество бактерий: {bacteria}");
        
        if (bacteria == 0)
        {
            Console.WriteLine("🎉 Результат: Антибиотик полностью уничтожил все бактерии!");
        }
        else if (killPower <= 0)
        {
            Console.WriteLine("⚠️  Результат: Антибиотик закончился, бактерии выжили!");
            
            // Прогноз дальнейшего роста
            Console.WriteLine();
            Console.WriteLine("📈 Прогноз роста без антибиотика:");
            long futureBacteria = bacteria;
            for (int futureHours = 1; futureHours <= 5; futureHours++)
            {
                futureBacteria *= 2;
                Console.WriteLine($"   Через {futureHours} час(ов): {futureBacteria} бактерий");
            }
        }

        // Анализ эффективности
        Console.WriteLine();
        Console.WriteLine("=== АНАЛИЗ ЭФФЕКТИВНОСТИ ===");
        double survivalRate = (double)bacteria / N * 100;
        Console.WriteLine($"Выживаемость бактерий: {survivalRate:F2}% от начального количества");
        
        if (hours > 0)
        {
            double avgKillRate = (double)(N - bacteria) / hours;
            Console.WriteLine($"Средняя скорость уничтожения: {avgKillRate:F1} бактерий/час");
        }

        // Рекомендации
        Console.WriteLine();
        Console.WriteLine("💡 РЕКОМЕНДАЦИИ:");
        if (bacteria > N)
        {
            Console.WriteLine("• Увеличить дозировку антибиотика");
            Console.WriteLine("• Рассмотреть применение более сильного антибиотика");
        }
        else if (bacteria == 0)
        {
            Console.WriteLine("• Текущая дозировка эффективна");
            Console.WriteLine("• Можно рассмотреть снижение дозировки для экономии");
        }
        else
        {
            Console.WriteLine("• Умеренно эффективная дозировка");
            Console.WriteLine("• Рекомендуется увеличить количество капель");
        }
    }
}