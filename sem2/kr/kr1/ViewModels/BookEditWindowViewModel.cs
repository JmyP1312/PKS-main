using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManager.Models;
using LibraryManager.Services;

namespace LibraryManager.ViewModels
{
    public class BookEditWindowViewModel : ViewModelBase
    {
        private readonly BookService _bookService;
        private readonly AuthorService _authorService;
        private readonly GenreService _genreService;
        private readonly Book? _originalBook;

        private int _bookId;
        private string _title = string.Empty;
        private string _isbn = string.Empty;
        private int _publishYear;
        private int _quantityInStock;

        // Авторы и жанры как список с флагом выбора
        public ObservableCollection<SelectableItem<Author>> SelectableAuthors { get; } = new();
        public ObservableCollection<SelectableItem<Genre>>  SelectableGenres  { get; } = new();

        public int BookId
        {
            get => _bookId;
            set => SetProperty(ref _bookId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ISBN
        {
            get => _isbn;
            set => SetProperty(ref _isbn, value);
        }

        public int PublishYear
        {
            get => _publishYear;
            set => SetProperty(ref _publishYear, value);
        }

        public int QuantityInStock
        {
            get => _quantityInStock;
            set => SetProperty(ref _quantityInStock, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<DialogResultEventArgs>? CloseRequested;

        public BookEditWindowViewModel(Book? book, BookService bookService, AuthorService authorService, GenreService genreService)
        {
            _bookService = bookService;
            _authorService = authorService;
            _genreService = genreService;
            _originalBook = book;

            SaveCommand   = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            var genres  = await _genreService.GetAllGenresAsync();

            // Выбранные авторы/жанры у редактируемой книги
            var selectedAuthorIds = _originalBook?.Authors.Select(a => a.Id).ToHashSet() ?? new HashSet<int>();
            var selectedGenreIds  = _originalBook?.Genres.Select(g => g.Id).ToHashSet()  ?? new HashSet<int>();

            foreach (var a in authors)
                SelectableAuthors.Add(new SelectableItem<Author>(a, selectedAuthorIds.Contains(a.Id)));

            foreach (var g in genres)
                SelectableGenres.Add(new SelectableItem<Genre>(g, selectedGenreIds.Contains(g.Id)));

            if (_originalBook != null)
            {
                BookId          = _originalBook.Id;
                Title           = _originalBook.Title;
                ISBN            = _originalBook.ISBN;
                PublishYear     = _originalBook.PublishYear;
                QuantityInStock = _originalBook.QuantityInStock;
            }
            else
            {
                PublishYear     = DateTime.Now.Year;
                QuantityInStock = 1;
            }
        }

        private async void Save()
        {
            var authorIds = SelectableAuthors.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            var genreIds  = SelectableGenres.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();

            if (string.IsNullOrWhiteSpace(Title) || authorIds.Count == 0 || genreIds.Count == 0)
            {
                await DialogService.ShowAsync("Укажите название, хотя бы одного автора и хотя бы один жанр", "Ошибка");
                return;
            }

            try
            {
                if (_originalBook == null)
                {
                    var newBook = new Book
                    {
                        Title           = Title,
                        ISBN            = ISBN,
                        PublishYear     = PublishYear,
                        QuantityInStock = QuantityInStock,
                    };
                    await _bookService.AddBookAsync(newBook, authorIds, genreIds);
                }
                else
                {
                    _originalBook.Title           = Title;
                    _originalBook.ISBN            = ISBN;
                    _originalBook.PublishYear     = PublishYear;
                    _originalBook.QuantityInStock = QuantityInStock;
                    await _bookService.UpdateBookAsync(_originalBook, authorIds, genreIds);
                }

                CloseRequested?.Invoke(this, new DialogResultEventArgs { DialogResult = true });
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync($"Ошибка при сохранении: {ex.Message}", "Ошибка");
            }
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, new DialogResultEventArgs { DialogResult = false });
        }
    }

    public class DialogResultEventArgs : EventArgs
    {
        public bool DialogResult { get; set; }
    }
}
