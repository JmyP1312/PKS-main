using Avalonia.Controls;
using LibraryManager.Data;
using LibraryManager.Services;
using LibraryManager.ViewModels;

namespace LibraryManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DialogService.Initialize(this);
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var context = new LibraryDbContext();
            await context.Database.EnsureCreatedAsync();
            context.SeedData();

            var bookService = new BookService(context);
            var authorService = new AuthorService(context);
            var genreService = new GenreService(context);

            var viewModel = new MainWindowViewModel(bookService, authorService, genreService);
            DataContext = viewModel;

            await viewModel.LoadData();
        }
    }
}
