using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TouristGuideWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Population = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    History = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CoatOfArmsUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    History = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    WorkingHours = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TicketPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attractions_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CoatOfArmsUrl", "Description", "History", "ImageUrl", "Name", "Population", "Region" },
                values: new object[,]
                {
                    { 1, "/images/Coat_of_arms_of_Moscow.svg.png", "Столица России, крупнейший город страны.", "Москва — один из древнейших городов России. Первое упоминание о Москве встречается в летописи 1147 года. Город стал столицей Великого княжества Московского в XV веке и остаётся столицей России по сей день.", "/images/moscow.jpg", "Москва", 12537954, "Центральный федеральный округ" },
                    { 2, "/images/images.png", "Культурная столица России, город на Неве.", "Санкт-Петербург основан Петром I в 1703 году. Город был столицей Российской империи до 1918 года. Известен своей архитектурой, музеями и культурным наследием.", "/images/4646af3618e50dcf2cbddfb325446f11.webp", "Санкт-Петербург", 5383890, "Северо-Западный федеральный округ" },
                    { 3, "/images/Снимок экрана 2026-05-05 в 10.23.55 AM.png", "Столица Татарстана, город с богатой историей.", "Казань — один из древнейших городов России, основанный в 1005 году. В 1556 году город вошёл в состав России. Сегодня это крупный культурный и образовательный центр.", "/images/324671d930c2c6b747910a712de92692.webp", "Казань", 1259173, "Приволжский федеральный округ" },
                    { 4, "/images/Coat_of_Arms_of_Vladivostok.svg.png", "Город на Тихом океане, крупнейший порт Дальнего Востока.", "Владивосток основан в 1859 году как военный пост. Город стал важнейшей военно-морской базой России на Тихом океане и крупнейшим портом Дальнего Востока.", "/images/vladivostok.jpg", "Владивосток", 604901, "Дальневосточный федеральный округ" }
                });

            migrationBuilder.InsertData(
                table: "Attractions",
                columns: new[] { "Id", "CityId", "Description", "History", "ImageUrl", "Name", "TicketPrice", "WorkingHours" },
                values: new object[,]
                {
                    { 1, 1, "Главная площадь Москвы, включена в список Всемирного наследия ЮНЕСКО.", "Красная площадь — сердце Москвы и всей России. На протяжении веков здесь проходили важнейшие исторические события. В XVII веке площадь стала главным торговым центром города.", "/images/red_square.jpg", "Красная площадь", 0m, "Круглосуточно" },
                    { 2, 1, "Древнейшая часть Москвы, резиденция Президента России.", "Кремль — древнейшая часть Москвы, впервые упоминается в 1156 году. На территории Кремля находятся древнейшие храмы и дворцы, соборы и музеи.", "/images/kremlin.jpg", "Московский Кремль", 700m, "10:00-18:00" },
                    { 3, 1, "Кафедральный собор Русской Православной Церкви.", "Храм Христа Спасителя был заложен в 1839 году в честь победы в Отечественной войне 1812 года. Разрушен в 1931 году и восстановлен в 1994-2000 годах.", "/images/christ_savior.jpg", "Храм Христа Спасителя", 0m, "08:00-20:00" },
                    { 4, 2, "Один из крупнейших музеев мира, государственный музей искусств.", "Эрмитаж основан Екатериной II в 1764 году. Коллекция музея насчитывает более 3 миллионов произведений искусства и памятников мировой культуры.", "/images/hermitage.jpg", "Эрмитаж", 400m, "10:30-18:00" },
                    { 5, 2, "Главная площадь Санкт-Петербурга, архитектурный ансамбль.", "Дворцовая площадь — главная площадь Санкт-Петербурга, сформировавшаяся в XVIII-XIX веках. На площади находится Зимний дворец, Александровская колонна и другие памятники архитектуры.", "/images/palace_square.jpg", "Дворцовая площадь", 0m, "Круглосуточно" },
                    { 6, 2, "Крупнейший собор Санкт-Петербурга, выдающийся памятник архитектуры.", "Исаакиевский собор строился с 1818 по 1858 год. Собор является одним из самых высоких купольных сооружений в мире.", "/images/isaac.jpg", "Исаакиевский собор", 350m, "10:00-18:00" },
                    { 7, 3, "Древнейшая часть Казани, объект Всемирного наследия ЮНЕСКО.", "Казанский кремль — древнейшая часть города, основан в XI веке. С 2000 года включён в список Всемирного наследия ЮНЕСКО как уникальный образец культурного наследия народов России.", "/images/kazan_kremlin.jpg", "Казанский кремль", 0m, "06:00-22:00" },
                    { 8, 3, "Главная мечеть Татарстана, символ Казани.", "Мечеть Кул-Шариф была построена в XVI веке и разрушена в 1552 году при осаде Казани. Современная мечеть восстановлена в 2005 году.", "/images/kul_sharif.jpg", "Мечеть Кул-Шариф", 0m, "08:00-20:00" },
                    { 9, 4, "Вантовый мост через пролив Босфор Восточный, один из крупнейших в мире.", "Русский мост построен к саммиту АТЭС 2012 года. Мост связал остров Русский с материковой частью Владивостока.", "/images/russian_bridge.jpg", "Русский мост", 0m, "Круглосуточно" },
                    { 10, 4, "Мощная система оборонительных сооружений, одна из сильнейших в мире.", "Владивостокская крепость строилась с 1879 по 1918 год. Крепость считалась одной из самых мощных в мире и включала более 100 фортов и батарей.", "/images/fort.jpg", "Владивостокская крепость", 100m, "10:00-18:00" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attractions_CityId",
                table: "Attractions",
                column: "CityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attractions");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
