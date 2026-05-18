using System;
using System.Collections.Generic;
using System.Globalization;

// Перечисление частот повторения события
enum RepeatFrequency
{
    None, Daily, Weekly, Monthly, Yearly
}

// Класс события
class Event
{
    public string Title { get; set; }       // Название события
    public DateTime Date { get; set; }      // Дата события
    public RepeatFrequency Frequency { get; set; } // Частота повторения

    public Event(string title, DateTime date, RepeatFrequency freq)
    {
        Title = title;
        Date = date;
        Frequency = freq;
    }

    // Проверяет, происходит ли событие в указанную дату
    public bool OccursOn(DateTime day)
    {
        switch (Frequency)
        {
            case RepeatFrequency.None:
                return day.Date == Date.Date; // только один раз
            case RepeatFrequency.Daily:
                return day.Date >= Date.Date; // начиная с указанной даты, каждый день
            case RepeatFrequency.Weekly:
                return day.Date >= Date.Date && day.DayOfWeek == Date.DayOfWeek;
            case RepeatFrequency.Monthly:
                return day.Date >= Date.Date && day.Day == Date.Day;
            case RepeatFrequency.Yearly:
                return day.Date >= Date.Date && day.Day == Date.Day && day.Month == Date.Month;
            default:
                return false;
        }
    }
}

namespace CalendarApp
{
    class Program
    {
        // Список всех событий
        static List<Event> events = new List<Event>();

        static void Main()
        {
            // Ввод года, по умолчанию — текущий
            Console.Write("Введите год (или оставьте пустым для текущего): ");
            string input = Console.ReadLine();

            int year;
            if (string.IsNullOrWhiteSpace(input))
                year = DateTime.Now.Year;
            else if (!int.TryParse(input, out year))
            {
                year = DateTime.Now.Year;
                Console.WriteLine($"Некорректный ввод. Использую {year}.");
            }

            // Главное меню
            while (true)
            {
                Console.WriteLine("\nГлавное меню:");
                Console.WriteLine("1. Показать календарь за год");
                Console.WriteLine("2. Показать календарь за день");
                Console.WriteLine("3. Добавить новое событие");
                Console.WriteLine("4. Посмотреть события на выбранный день");
                Console.WriteLine("5. Проверить день недели (будний/выходной)");
                Console.WriteLine("0. Выход");
                Console.Write("Ваш выбор: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ShowYearCalendar(year); break; // вывод всего года
                    case "2": ShowDayCalendar(); break;      // вывод одного дня
                    case "3": AddEvent(); break;             // добавить событие
                    case "4": ShowEventsForDay(); break;     // события в конкретный день
                    case "5": CheckDayOfWeek(); break;       // проверка будний/выходной
                    case "0": return;                        // выход
                    default: Console.WriteLine("Такого пункта нет."); break;
                }
            }
        }

        // Показ всего года
        static void ShowYearCalendar(int year)
        {
            for (int month = 1; month <= 12; month++)
                PrintMonth(year, month);
        }

        // Показ одного дня + события
        static void ShowDayCalendar()
        {
            Console.Write("Введите дату (дд.мм.гггг): ");
            string input = Console.ReadLine();

            if (DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime day))
            {
                Console.WriteLine($"\n{day:dddd, dd MMMM yyyy}");
                Console.WriteLine("События на этот день:");
                bool found = false;

                foreach (var ev in events)
                    if (ev.OccursOn(day))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"- {ev.Title} ({ev.Frequency})");
                        Console.ResetColor();
                        found = true;
                    }

                if (!found) Console.WriteLine("Нет событий.");
            }
            else
            {
                Console.WriteLine("Неверная дата.");
            }
        }

        // Добавление нового события
        static void AddEvent()
        {
            Console.Write("Название события: ");
            string title = Console.ReadLine();

            Console.Write("Дата (дд.мм.гггг): ");
            string input = Console.ReadLine();
            if (!DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine("Неверная дата.");
                return;
            }

            // Выбор частоты
            Console.WriteLine("Частота: 0 - Нет, 1 - Ежедневно, 2 - Еженедельно, 3 - Ежемесячно, 4 - Ежегодно");
            string freqInput = Console.ReadLine();
            RepeatFrequency freq = RepeatFrequency.None;
            switch (freqInput)
            {
                case "1": freq = RepeatFrequency.Daily; break;
                case "2": freq = RepeatFrequency.Weekly; break;
                case "3": freq = RepeatFrequency.Monthly; break;
                case "4": freq = RepeatFrequency.Yearly; break;
            }

            // Сохраняем событие
            events.Add(new Event(title, date, freq));
            Console.WriteLine("Событие добавлено!");
        }

        // Показ событий на выбранный день
        static void ShowEventsForDay()
        {
            Console.Write("Введите дату (дд.мм.гггг): ");
            string input = Console.ReadLine();
            if (DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime day))
            {
                Console.WriteLine($"События на {day:dd.MM.yyyy}:");
                bool found = false;

                foreach (var ev in events)
                    if (ev.OccursOn(day))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"- {ev.Title} ({ev.Frequency})");
                        Console.ResetColor();
                        found = true;
                    }

                if (!found) Console.WriteLine("Нет событий.");
            }
            else
            {
                Console.WriteLine("Неверная дата.");
            }
        }

        // Проверка дня недели: будний или выходной
        static void CheckDayOfWeek()
        {
            Console.Write("Введите дату (дд.мм.гггг): ");
            string input = Console.ReadLine();

            if (DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime day))
            {
                Console.WriteLine($"{day:dd.MM.yyyy} — {day:dddd}");

                if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Это выходной день!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Это будний день.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("Неверная дата.");
            }
        }

        // Печать месяца
        static void PrintMonth(int year, int month)
        {
            string monthName = new DateTime(year, month, 1).ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("ru-RU"));
            Console.WriteLine($"\n{monthName}");

            // Заголовки дней недели
            string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            foreach (var day in days)
                Console.Write(day.PadRight(4));
            Console.WriteLine();

            // Вычисляем смещение для начала месяца
            DateTime firstDay = new DateTime(year, month, 1);
            int startDay = (int)firstDay.DayOfWeek;
            if (startDay == 0) startDay = 7;

            int daysInMonth = DateTime.DaysInMonth(year, month);
            int currentPos = 1;

            // Пустые клетки перед началом месяца
            for (int i = 1; i < startDay; i++)
            {
                Console.Write("    ");
                currentPos++;
            }

            // Печать всех дней
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(year, month, day);
                bool hasEvent = events.Exists(e => e.OccursOn(currentDate));

                // Подсветка текущей даты, выходных и событий
                if (currentDate == DateTime.Today)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (hasEvent)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ResetColor();

                Console.Write(day.ToString().PadRight(4));
                Console.ResetColor();

                if (currentPos % 7 == 0)
                    Console.WriteLine();

                currentPos++;
            }
            Console.WriteLine("\n");
        }
    }
}