using Microsoft.EntityFrameworkCore;
using LibraryManager.Models;
using LibraryManager.Data;

namespace LibraryManager.Services
{
    public class BookService
    {
        private readonly LibraryDbContext _context;

        public BookService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .AsNoTracking()
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .ToListAsync();
        }

        public async Task<List<Book>> SearchBooksByTitleAsync(string title)
        {
            if (string.IsNullOrEmpty(title))
                return new List<Book>();
            
            return await _context.Books
                .AsNoTracking()
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Where(b => b.Title.Contains(title))
                .ToListAsync();
        }

        public async Task<List<Book>> FilterByGenreAsync(int genreId)
        {
            return await _context.Books
                .AsNoTracking()
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Where(b => b.Genres.Any(g => g.Id == genreId))
                .ToListAsync();
        }

        public async Task<List<Book>> FilterByAuthorAsync(int authorId)
        {
            return await _context.Books
                .AsNoTracking()
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Where(b => b.Authors.Any(a => a.Id == authorId))
                .ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddBookAsync(Book book, List<int> authorIds, List<int> genreIds)
        {
            var authors = await _context.Authors.Where(a => authorIds.Contains(a.Id)).ToListAsync();
            var genres  = await _context.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            foreach (var a in authors) book.Authors.Add(a);
            foreach (var g in genres)  book.Genres.Add(g);
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book, List<int> authorIds, List<int> genreIds)
        {
            var existing = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == book.Id);
            if (existing == null) return;

            existing.Title           = book.Title;
            existing.ISBN            = book.ISBN;
            existing.PublishYear     = book.PublishYear;
            existing.QuantityInStock = book.QuantityInStock;

            existing.Authors.Clear();
            var authors = await _context.Authors.Where(a => authorIds.Contains(a.Id)).ToListAsync();
            foreach (var a in authors) existing.Authors.Add(a);

            existing.Genres.Clear();
            var genres = await _context.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            foreach (var g in genres) existing.Genres.Add(g);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }
    }
}
