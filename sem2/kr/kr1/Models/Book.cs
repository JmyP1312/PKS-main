using System;
using System.Linq;

namespace LibraryManager.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        public string Title { get; set; }
        
        public string ISBN { get; set; }
        
        public int PublishYear { get; set; }
        
        public int QuantityInStock { get; set; }

        // Many-to-many: несколько авторов
        public ICollection<Author> Authors { get; set; } = new List<Author>();

        // Many-to-many: несколько жанров
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

        // Для отображения в DataGrid
        public string AuthorsDisplay => Authors != null && Authors.Any()
            ? string.Join(", ", Authors.Select(a => a.FullName))
            : "—";

        public string GenresDisplay => Genres != null && Genres.Any()
            ? string.Join(", ", Genres.Select(g => g.Name))
            : "—";
    }
}
