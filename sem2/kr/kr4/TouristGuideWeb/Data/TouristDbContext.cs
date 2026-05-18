using Microsoft.EntityFrameworkCore;
using TouristGuideWeb.Models;

namespace TouristGuideWeb.Data;

public class TouristDbContext : DbContext
{
    public TouristDbContext(DbContextOptions<TouristDbContext> options) : base(options)
    {
    }

    public DbSet<City> Cities { get; set; }
    public DbSet<Attraction> Attractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>().HasData(
            new City
            {
                Id = 1,
                Name = "Москва",
                Region = "Центральный федеральный округ",
                Population = 12537954,
                Description = "Столица России, крупнейший город страны.",
                History = "Москва — один из древнейших городов России. Первое упоминание о Москве встречается в летописи 1147 года. Город стал столицей Великого княжества Московского в XV веке и остаётся столицей России по сей день.",
                ImageUrl = "/images/moscow.jpg",
                CoatOfArmsUrl = "/images/Coat_of_arms_of_Moscow.svg.png"
            },
            new City
            {
                Id = 2,
                Name = "Санкт-Петербург",
                Region = "Северо-Западный федеральный округ",
                Population = 5383890,
                Description = "Культурная столица России, город на Неве.",
                History = "Санкт-Петербург основан Петром I в 1703 году. Город был столицей Российской империи до 1918 года. Известен своей архитектурой, музеями и культурным наследием.",
                ImageUrl = "/images/4646af3618e50dcf2cbddfb325446f11.webp",
                CoatOfArmsUrl = "/images/images.png"
            },
            new City
            {
                Id = 3,
                Name = "Казань",
                Region = "Приволжский федеральный округ",
                Population = 1259173,
                Description = "Столица Татарстана, город с богатой историей.",
                History = "Казань — один из древнейших городов России, основанный в 1005 году. В 1556 году город вошёл в состав России. Сегодня это крупный культурный и образовательный центр.",
                ImageUrl = "/images/324671d930c2c6b747910a712de92692.webp",
                CoatOfArmsUrl = "/images/Снимок экрана 2026-05-05 в 10.23.55 AM.png"
            },
            new City
            {
                Id = 4,
                Name = "Владивосток",
                Region = "Дальневосточный федеральный округ",
                Population = 604901,
                Description = "Город на Тихом океане, крупнейший порт Дальнего Востока.",
                History = "Владивосток основан в 1859 году как военный пост. Город стал важнейшей военно-морской базой России на Тихом океане и крупнейшим портом Дальнего Востока.",
                ImageUrl = "/images/vladivostok.jpg",
                CoatOfArmsUrl = "/images/Coat_of_Arms_of_Vladivostok.svg.png"
            }
        );

        modelBuilder.Entity<Attraction>().HasData(
            // Москвa
            new Attraction
            {
                Id = 1,
                Name = "Красная площадь",
                Description = "Главная площадь Москвы, включена в список Всемирного наследия ЮНЕСКО.",
                History = "Красная площадь — сердце Москвы и всей России. На протяжении веков здесь проходили важнейшие исторические события. В XVII веке площадь стала главным торговым центром города.",
                ImageUrl = "/images/red_square.jpg",
                WorkingHours = "Круглосуточно",
                TicketPrice = 0,
                CityId = 1
            },
            new Attraction
            {
                Id = 2,
                Name = "Московский Кремль",
                Description = "Древнейшая часть Москвы, резиденция Президента России.",
                History = "Кремль — древнейшая часть Москвы, впервые упоминается в 1156 году. На территории Кремля находятся древнейшие храмы и дворцы, соборы и музеи.",
                ImageUrl = "/images/kremlin.jpg",
                WorkingHours = "10:00-18:00",
                TicketPrice = 700,
                CityId = 1
            },
            new Attraction
            {
                Id = 3,
                Name = "Храм Христа Спасителя",
                Description = "Кафедральный собор Русской Православной Церкви.",
                History = "Храм Христа Спасителя был заложен в 1839 году в честь победы в Отечественной войне 1812 года. Разрушен в 1931 году и восстановлен в 1994-2000 годах.",
                ImageUrl = "/images/christ_savior.jpg",
                WorkingHours = "08:00-20:00",
                TicketPrice = 0,
                CityId = 1
            },
            // Санкт-Петербург
            new Attraction
            {
                Id = 4,
                Name = "Эрмитаж",
                Description = "Один из крупнейших музеев мира, государственный музей искусств.",
                History = "Эрмитаж основан Екатериной II в 1764 году. Коллекция музея насчитывает более 3 миллионов произведений искусства и памятников мировой культуры.",
                ImageUrl = "/images/hermitage.jpg",
                WorkingHours = "10:30-18:00",
                TicketPrice = 400,
                CityId = 2
            },
            new Attraction
            {
                Id = 5,
                Name = "Дворцовая площадь",
                Description = "Главная площадь Санкт-Петербурга, архитектурный ансамбль.",
                History = "Дворцовая площадь — главная площадь Санкт-Петербурга, сформировавшаяся в XVIII-XIX веках. На площади находится Зимний дворец, Александровская колонна и другие памятники архитектуры.",
                ImageUrl = "/images/palace_square.jpg",
                WorkingHours = "Круглосуточно",
                TicketPrice = 0,
                CityId = 2
            },
            new Attraction
            {
                Id = 6,
                Name = "Исаакиевский собор",
                Description = "Крупнейший собор Санкт-Петербурга, выдающийся памятник архитектуры.",
                History = "Исаакиевский собор строился с 1818 по 1858 год. Собор является одним из самых высоких купольных сооружений в мире.",
                ImageUrl = "/images/isaac.jpg",
                WorkingHours = "10:00-18:00",
                TicketPrice = 350,
                CityId = 2
            },
            // Казань
            new Attraction
            {
                Id = 7,
                Name = "Казанский кремль",
                Description = "Древнейшая часть Казани, объект Всемирного наследия ЮНЕСКО.",
                History = "Казанский кремль — древнейшая часть города, основан в XI веке. С 2000 года включён в список Всемирного наследия ЮНЕСКО как уникальный образец культурного наследия народов России.",
                ImageUrl = "/images/kazan_kremlin.jpg",
                WorkingHours = "06:00-22:00",
                TicketPrice = 0,
                CityId = 3
            },
            new Attraction
            {
                Id = 8,
                Name = "Мечеть Кул-Шариф",
                Description = "Главная мечеть Татарстана, символ Казани.",
                History = "Мечеть Кул-Шариф была построена в XVI веке и разрушена в 1552 году при осаде Казани. Современная мечеть восстановлена в 2005 году.",
                ImageUrl = "/images/kul_sharif.jpg",
                WorkingHours = "08:00-20:00",
                TicketPrice = 0,
                CityId = 3
            },
            // Владивосток
            new Attraction
            {
                Id = 9,
                Name = "Русский мост",
                Description = "Вантовый мост через пролив Босфор Восточный, один из крупнейших в мире.",
                History = "Русский мост построен к саммиту АТЭС 2012 года. Мост связал остров Русский с материковой частью Владивостока.",
                ImageUrl = "/images/russian_bridge.jpg",
                WorkingHours = "Круглосуточно",
                TicketPrice = 0,
                CityId = 4
            },
            new Attraction
            {
                Id = 10,
                Name = "Владивостокская крепость",
                Description = "Мощная система оборонительных сооружений, одна из сильнейших в мире.",
                History = "Владивостокская крепость строилась с 1879 по 1918 год. Крепость считалась одной из самых мощных в мире и включала более 100 фортов и батарей.",
                ImageUrl = "/images/fort.jpg",
                WorkingHours = "10:00-18:00",
                TicketPrice = 100,
                CityId = 4
            }
        );
    }
}
