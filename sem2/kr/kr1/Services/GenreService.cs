using Microsoft.EntityFrameworkCore;
using LibraryManager.Models;
using LibraryManager.Data;

namespace LibraryManager.Services
{
    public class GenreService
    {
        private readonly LibraryDbContext _context;

        public GenreService(LibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // TODO: добавить кэширование когда-нибудь
        public async Task<List<Genre>> GetAllGenresAsync()
        {
            return await _context.Genres
                .Include(g => g.Books)
                .ToListAsync();
        }

        public async Task<Genre> GetGenreByIdAsync(int id)
        {
            return await _context.Genres
                .Include(g => g.Books)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddGenreAsync(Genre genre)
        {
            if (genre == null)
                throw new ArgumentNullException(nameof(genre));
            if (string.IsNullOrWhiteSpace(genre.Name))
                throw new ArgumentException("Название обязательно", nameof(genre));

            // проверка дубликатов перенести в БД как констрейнт?
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGenreAsync(Genre genre)
        {
            _context.Genres.Update(genre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGenreAsync(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
            }
        }
    }
}
