using Microsoft.EntityFrameworkCore;
using LibraryManager.Models;
using LibraryManager.Data;

namespace LibraryManager.Services
{
    public class AuthorService
    {
        private readonly LibraryDbContext _context;

        public AuthorService(LibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // TODO: добавить сортировку по фамилии
        // TODO: потом кэширование добавить если нужно будет
        public async Task<List<Author>> GetAllAuthorsAsync()
        {
            return await _context.Authors
                .AsNoTracking()
                .OrderBy(a => a.LastName)
                .ToListAsync();
        }

        // FIXME: может быть стоит Include для Books загружать?
        public async Task<Author> GetAuthorByIdAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAuthorAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));
            
            if (string.IsNullOrWhiteSpace(author.FirstName))
                throw new ArgumentException("Имя обязательно");
                
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAuthorAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
            }
        }
    }
}
