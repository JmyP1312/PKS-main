using Microsoft.EntityFrameworkCore;
using LibraryManager.Models;

namespace LibraryManager.Data
{
    public class LibraryDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=library.db");
        }

        public void SeedData()
        {
            if (Authors.Any() || Genres.Any()) return;

            var scifi  = new LibraryManager.Models.Genre { Name = "Научная фантастика", Description = "Литература, исследующая научные идеи и технологии будущего" };
            var fantasy = new LibraryManager.Models.Genre { Name = "Фэнтези",            Description = "Произведения с элементами магии и фантастического мира" };
            var detective = new LibraryManager.Models.Genre { Name = "Детектив",         Description = "Произведения с расследованием преступления" };
            var romance = new LibraryManager.Models.Genre { Name = "Романтика",          Description = "Произведения о романтических отношениях" };
            var classic = new LibraryManager.Models.Genre { Name = "Классика",           Description = "Классические произведения литературы" };
            var horror  = new LibraryManager.Models.Genre { Name = "Ужасы",              Description = "Произведения, вызывающие страх и ужас" };
            var adventure = new LibraryManager.Models.Genre { Name = "Приключения",      Description = "Приключенческие, полные действия произведения" };
            Genres.AddRange(scifi, fantasy, detective, romance, classic, horror, adventure);

            var asimov  = new LibraryManager.Models.Author { FirstName = "Исаак",    LastName = "Азимов",     Country = "США",            BirthDate = new DateTime(1920, 1, 2) };
            var martin  = new LibraryManager.Models.Author { FirstName = "Джордж",   LastName = "Мартин",     Country = "США",            BirthDate = new DateTime(1948, 9, 20) };
            var christie = new LibraryManager.Models.Author { FirstName = "Агата",   LastName = "Кристи",     Country = "Великобритания", BirthDate = new DateTime(1890, 1, 15) };
            var dostoevsky = new LibraryManager.Models.Author { FirstName = "Федор", LastName = "Достоевский",Country = "Россия",         BirthDate = new DateTime(1821, 11, 11) };
            var tolstoy = new LibraryManager.Models.Author { FirstName = "Лев",      LastName = "Толстой",    Country = "Россия",         BirthDate = new DateTime(1828, 9, 9) };
            var doyle   = new LibraryManager.Models.Author { FirstName = "Артур",    LastName = "Конан Дойл", Country = "Великобритания", BirthDate = new DateTime(1859, 5, 22) };
            var verne   = new LibraryManager.Models.Author { FirstName = "Жюль",     LastName = "Верн",       Country = "Франция",        BirthDate = new DateTime(1828, 2, 8) };
            var oleg    = new LibraryManager.Models.Author { FirstName = "Нечипаренко", LastName = "Олег",    Country = "Испания",        BirthDate = new DateTime(2021, 2, 2) };
            Authors.AddRange(asimov, martin, christie, dostoevsky, tolstoy, doyle, verne, oleg);
            SaveChanges();

            var books = new List<LibraryManager.Models.Book>
            {
                new LibraryManager.Models.Book { Title = "Основание",                     ISBN = "978-0553293357", PublishYear = 1951, QuantityInStock = 5,
                    Authors = new List<LibraryManager.Models.Author> { asimov },
                    Genres  = new List<LibraryManager.Models.Genre> { scifi } },

                new LibraryManager.Models.Book { Title = "Я, робот",                      ISBN = "978-0553382563", PublishYear = 1950, QuantityInStock = 3,
                    Authors = new List<LibraryManager.Models.Author> { asimov },
                    Genres  = new List<LibraryManager.Models.Genre> { scifi } },

                new LibraryManager.Models.Book { Title = "Игра престолов",                ISBN = "9780553588483", PublishYear = 1996, QuantityInStock = 2,
                    Authors = new List<LibraryManager.Models.Author> { martin },
                    Genres  = new List<LibraryManager.Models.Genre> { fantasy, adventure } },

                new LibraryManager.Models.Book { Title = "Праздник Крыс",                 ISBN = "9780553108385", PublishYear = 1998, QuantityInStock = 4,
                    Authors = new List<LibraryManager.Models.Author> { martin },
                    Genres  = new List<LibraryManager.Models.Genre> { fantasy } },

                new LibraryManager.Models.Book { Title = "Убийство в восточном экспрессе",ISBN = "9780062693662", PublishYear = 1934, QuantityInStock = 6,
                    Authors = new List<LibraryManager.Models.Author> { christie },
                    Genres  = new List<LibraryManager.Models.Genre> { detective } },

                new LibraryManager.Models.Book { Title = "Смерть на Ниле",                ISBN = "9780062693679", PublishYear = 1937, QuantityInStock = 5,
                    Authors = new List<LibraryManager.Models.Author> { christie },
                    Genres  = new List<LibraryManager.Models.Genre> { detective } },

                new LibraryManager.Models.Book { Title = "Преступление и наказание",      ISBN = "9785170970322", PublishYear = 1866, QuantityInStock = 4,
                    Authors = new List<LibraryManager.Models.Author> { dostoevsky },
                    Genres  = new List<LibraryManager.Models.Genre> { classic } },

                new LibraryManager.Models.Book { Title = "Война и мир",                   ISBN = "9785170850878", PublishYear = 1869, QuantityInStock = 3,
                    Authors = new List<LibraryManager.Models.Author> { tolstoy },
                    Genres  = new List<LibraryManager.Models.Genre> { classic } },

                new LibraryManager.Models.Book { Title = "Собака Баскервилей",            ISBN = "9780062073556", PublishYear = 1901, QuantityInStock = 7,
                    Authors = new List<LibraryManager.Models.Author> { doyle },
                    Genres  = new List<LibraryManager.Models.Genre> { detective, adventure } },

                new LibraryManager.Models.Book { Title = "Таинственный остров",           ISBN = "9785170916697", PublishYear = 1874, QuantityInStock = 2,
                    Authors = new List<LibraryManager.Models.Author> { verne },
                    Genres  = new List<LibraryManager.Models.Genre> { adventure, scifi } },

                new LibraryManager.Models.Book { Title = "Дежавю",                        ISBN = "9785170916698", PublishYear = 1874, QuantityInStock = 1,
                    Authors = new List<LibraryManager.Models.Author> { oleg },
                    Genres  = new List<LibraryManager.Models.Genre> { scifi } },
            };
            Books.AddRange(books);
            SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка M2M: Book <-> Author
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .UsingEntity(j => j.ToTable("BookAuthors"));

            // Настройка M2M: Book <-> Genre
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(g => g.Books)
                .UsingEntity(j => j.ToTable("BookGenres"));

            // Ограничения на длину строк
            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .HasMaxLength(255);

            modelBuilder.Entity<Book>()
                .Property(b => b.ISBN)
                .HasMaxLength(20);

            modelBuilder.Entity<Author>()
                .Property(a => a.FirstName)
                .HasMaxLength(100);

            modelBuilder.Entity<Author>()
                .Property(a => a.LastName)
                .HasMaxLength(100);

            modelBuilder.Entity<Author>()
                .Property(a => a.Country)
                .HasMaxLength(100);

            modelBuilder.Entity<Genre>()
                .Property(g => g.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Genre>()
                .Property(g => g.Description)
                .HasMaxLength(500);
        }
    }
}
