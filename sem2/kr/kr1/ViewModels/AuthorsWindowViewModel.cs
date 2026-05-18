using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManager.Models;
using LibraryManager.Services;

namespace LibraryManager.ViewModels
{
    public class AuthorsWindowViewModel : ViewModelBase
    {
        private readonly AuthorService _authorService;
        private ObservableCollection<Author> _authors;
        private Author? _selectedAuthor;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private DateTime? _birthDate;
        private string _country = string.Empty;
        private bool _isEditMode;

        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set => SetProperty(ref _authors, value);
        }

        public Author? SelectedAuthor
        {
            get => _selectedAuthor;
            set
            {
                SetProperty(ref _selectedAuthor, value);
                if (value != null)
                {
                    FirstName = value.FirstName;
                    LastName = value.LastName;
                    BirthDate = value.BirthDate;
                    Country = value.Country;
                    IsEditMode = true;
                }
                (DeleteAuthorCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
                (SaveAuthorCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }

        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public ICommand AddAuthorCommand { get; }
        public ICommand SaveAuthorCommand { get; }
        public ICommand DeleteAuthorCommand { get; }
        public ICommand CancelCommand { get; }

        public AuthorsWindowViewModel(AuthorService authorService)
        {
            _authorService = authorService;
            Authors = new ObservableCollection<Author>();
            BirthDate = DateTime.Now;

            AddAuthorCommand = new RelayCommand(_ => NewAuthor());
            SaveAuthorCommand = new RelayCommand(_ => SaveAuthor(), _ => !string.IsNullOrWhiteSpace(FirstName));
            DeleteAuthorCommand = new RelayCommand(_ => DeleteAuthor(), _ => SelectedAuthor != null);
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadAuthorsAsync();
        }

        private async void LoadAuthorsAsync()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            Authors = new ObservableCollection<Author>(authors);
        }

        private void NewAuthor()
        {
            _selectedAuthor = null;
            OnPropertyChanged(nameof(SelectedAuthor));
            FirstName = string.Empty;
            LastName = string.Empty;
            BirthDate = DateTime.Now;
            Country = string.Empty;
            IsEditMode = false;
        }

        private async void SaveAuthor()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                await DialogService.ShowAsync("Имя автора не может быть пустым", "Ошибка");
                return;
            }

            try
            {
                if (_selectedAuthor == null)
                {
                    var newAuthor = new Author
                    {
                        FirstName = FirstName,
                        LastName = LastName,
                        BirthDate = BirthDate ?? DateTime.Now,
                        Country = Country
                    };
                    await _authorService.AddAuthorAsync(newAuthor);
                }
                else
                {
                    _selectedAuthor.FirstName = FirstName;
                    _selectedAuthor.LastName = LastName;
                    _selectedAuthor.BirthDate = BirthDate ?? DateTime.Now;
                    _selectedAuthor.Country = Country;
                    await _authorService.UpdateAuthorAsync(_selectedAuthor);
                }

                LoadAuthorsAsync();
                NewAuthor();
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void DeleteAuthor()
        {
            if (_selectedAuthor == null) return;

            var confirmed = await DialogService.ConfirmAsync(
                $"Удалить автора {_selectedAuthor.FirstName} {_selectedAuthor.LastName}?",
                "Подтверждение");

            if (confirmed)
            {
                if (_selectedAuthor.Books.Count > 0)
                {
                    await DialogService.ShowAsync(
                        "Невозможно удалить автора, у которого есть книги в системе.", "Ошибка");
                    return;
                }

                await _authorService.DeleteAuthorAsync(_selectedAuthor.Id);
                LoadAuthorsAsync();
                NewAuthor();
            }
        }

        private void Cancel()
        {
            NewAuthor();
        }
    }
}
