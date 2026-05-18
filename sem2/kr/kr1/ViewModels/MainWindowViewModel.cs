using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManager.Models;
using LibraryManager.Services;
using LibraryManager.Views;

namespace LibraryManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly BookService _bookService;
        private readonly AuthorService _authorService;
        private readonly GenreService _genreService;

        private ObservableCollection<Book> _books;
        private ObservableCollection<Author> _authors;
        private ObservableCollection<Genre> _genres;
        private Book? _selectedBook;
        private Author? _selectedAuthor;
        private Genre? _selectedGenre;
        private Author? _filterAuthor;
        private Genre? _filterGenre;
        private string _searchTitle = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Book> Books
        {
            get => _books;
            set => SetProperty(ref _books, value);
        }

        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set => SetProperty(ref _authors, value);
        }

        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        public Book? SelectedBook
        {
            get => _selectedBook;
            set
            {
                SetProperty(ref _selectedBook, value);
                (EditBookCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteBookCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public Author? SelectedAuthor
        {
            get => _selectedAuthor;
            set => SetProperty(ref _selectedAuthor, value);
        }

        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set => SetProperty(ref _selectedGenre, value);
        }

        public Author? FilterAuthor
        {
            get => _filterAuthor;
            set
            {
                SetProperty(ref _filterAuthor, value);
                FilterByAuthor();
            }
        }

        public Genre? FilterGenre
        {
            get => _filterGenre;
            set
            {
                SetProperty(ref _filterGenre, value);
                FilterByGenre();
            }
        }

        public string SearchTitle
        {
            get => _searchTitle;
            set => SetProperty(ref _searchTitle, value);
        }

        public ICommand AddBookCommand { get; }
        public ICommand EditBookCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand SearchBooksCommand { get; }
        public ICommand FilterByGenreCommand { get; }
        public ICommand FilterByAuthorCommand { get; }
        public ICommand LoadBooksCommand { get; }
        public ICommand ManageAuthorsCommand { get; }
        public ICommand ManageGenresCommand { get; }

        public MainWindowViewModel(BookService bookService, AuthorService authorService, GenreService genreService)
        {
            _bookService = bookService;
            _authorService = authorService;
            _genreService = genreService;

            Books = new ObservableCollection<Book>();
            Authors = new ObservableCollection<Author>();
            Genres = new ObservableCollection<Genre>();

            AddBookCommand = new RelayCommand(_ => AddBook());
            EditBookCommand = new RelayCommand(_ => EditBook(), _ => SelectedBook != null);
            DeleteBookCommand = new RelayCommand(_ => DeleteBook(), _ => SelectedBook != null);
            SearchBooksCommand = new RelayCommand(_ => SearchBooks());
            FilterByGenreCommand = new RelayCommand(_ => FilterByGenre());
            FilterByAuthorCommand = new RelayCommand(_ => FilterByAuthor());
            LoadBooksCommand = new RelayCommand(_ => LoadBooks());
            ManageAuthorsCommand = new RelayCommand(_ => ManageAuthors());
            ManageGenresCommand = new RelayCommand(_ => ManageGenres());
        }

        public async Task LoadData()
        {
            _isLoading = true;
            try
            {
                await LoadBooks();
                await LoadAuthors();
                await LoadGenres();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            Books = new ObservableCollection<Book>(books);
        }

        private async Task LoadAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            Authors = new ObservableCollection<Author>(authors);
        }

        private async Task LoadGenres()
        {
            var genres = await _genreService.GetAllGenresAsync();
            Genres = new ObservableCollection<Genre>(genres);
        }

        private async void AddBook()
        {
            var viewModel = new BookEditWindowViewModel(null, _bookService, _authorService, _genreService);
            var window = new BookEditWindow { DataContext = viewModel };
            var result = await window.ShowDialog<bool>(DialogService.MainWindow!);
            if (result) await LoadBooks();
        }

        private async void EditBook()
        {
            if (SelectedBook == null) return;
            var viewModel = new BookEditWindowViewModel(SelectedBook, _bookService, _authorService, _genreService);
            var window = new BookEditWindow { DataContext = viewModel };
            var result = await window.ShowDialog<bool>(DialogService.MainWindow!);
            if (result) await LoadBooks();
        }

        private async void DeleteBook()
        {
            if (SelectedBook == null) return;

            var confirmed = await DialogService.ConfirmAsync(
                $"Вы уверены, что хотите удалить книгу '{SelectedBook.Title}'?",
                "Подтверждение удаления");

            if (confirmed)
            {
                await _bookService.DeleteBookAsync(SelectedBook.Id);
                await LoadBooks();
            }
        }

        private async void SearchBooks()
        {
            if (string.IsNullOrWhiteSpace(SearchTitle))
            {
                await LoadBooks();
                return;
            }
            var books = await _bookService.SearchBooksByTitleAsync(SearchTitle);
            Books = new ObservableCollection<Book>(books);
        }

        private async void FilterByGenre()
        {
            if (_filterGenre == null) { await LoadBooks(); return; }
            var books = await _bookService.FilterByGenreAsync(_filterGenre.Id);
            Books = new ObservableCollection<Book>(books);
        }

        private async void FilterByAuthor()
        {
            if (_filterAuthor == null) { await LoadBooks(); return; }
            var books = await _bookService.FilterByAuthorAsync(_filterAuthor.Id);
            Books = new ObservableCollection<Book>(books);
        }

        private async void ManageAuthors()
        {
            var viewModel = new AuthorsWindowViewModel(_authorService);
            var window = new AuthorsWindow { DataContext = viewModel };
            await window.ShowDialog<object>(DialogService.MainWindow!);
            await LoadAuthors();
        }

        private async void ManageGenres()
        {
            var viewModel = new GenresWindowViewModel(_genreService);
            var window = new GenresWindow { DataContext = viewModel };
            await window.ShowDialog<object>(DialogService.MainWindow!);
            await LoadGenres();
        }
    }
}
