using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManager.Models;
using LibraryManager.Services;

namespace LibraryManager.ViewModels
{
    public class GenresWindowViewModel : ViewModelBase
    {
        private readonly GenreService _genreService;
        private ObservableCollection<Genre> _genres;
        private Genre? _selectedGenre;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isEditMode;

        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                SetProperty(ref _selectedGenre, value);
                if (value != null)
                {
                    Name = value.Name;
                    Description = value.Description;
                    IsEditMode = true;
                }
                (DeleteGenreCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                (SaveGenreCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public ICommand AddGenreCommand { get; }
        public ICommand SaveGenreCommand { get; }
        public ICommand DeleteGenreCommand { get; }
        public ICommand CancelCommand { get; }

        public GenresWindowViewModel(GenreService genreService)
        {
            _genreService = genreService;
            Genres = new ObservableCollection<Genre>();

            AddGenreCommand = new RelayCommand(_ => NewGenre());
            SaveGenreCommand = new RelayCommand(_ => SaveGenre(), _ => !string.IsNullOrWhiteSpace(Name));
            DeleteGenreCommand = new RelayCommand(_ => DeleteGenre(), _ => SelectedGenre != null);
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadGenresAsync();
        }

        private async void LoadGenresAsync()
        {
            var genres = await _genreService.GetAllGenresAsync();
            Genres = new ObservableCollection<Genre>(genres);
        }

        private void NewGenre()
        {
            _selectedGenre = null;
            OnPropertyChanged(nameof(SelectedGenre));
            Name = string.Empty;
            Description = string.Empty;
            IsEditMode = false;
        }

        private async void SaveGenre()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await DialogService.ShowAsync("Название не может быть пустым", "Ошибка");
                return;
            }

            try
            {
                if (_selectedGenre == null)
                {
                    var newGenre = new Genre { Name = Name, Description = Description };
                    await _genreService.AddGenreAsync(newGenre);
                }
                else
                {
                    _selectedGenre.Name = Name;
                    _selectedGenre.Description = Description;
                    await _genreService.UpdateGenreAsync(_selectedGenre);
                }

                LoadGenresAsync();
                NewGenre();
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void DeleteGenre()
        {
            if (_selectedGenre == null) return;

            var confirmed = await DialogService.ConfirmAsync(
                $"Удалить '{_selectedGenre.Name}'?", "Подтверждение");

            if (confirmed)
            {
                if (_selectedGenre.Books?.Count > 0)
                {
                    await DialogService.ShowAsync("Невозможно удалить жанр с книгами", "Ошибка");
                    return;
                }

                await _genreService.DeleteGenreAsync(_selectedGenre.Id);
                LoadGenresAsync();
                NewGenre();
            }
        }

        private void Cancel()
        {
            NewGenre();
        }
    }
}
