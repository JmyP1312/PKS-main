using Avalonia.Controls;
using LibraryManager.ViewModels;

namespace LibraryManager.Views
{
    public partial class BookEditWindow : Window
    {
        public BookEditWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (DataContext is BookEditWindowViewModel vm)
            {
                vm.CloseRequested += (_, args) => Close(args.DialogResult);
            }
        }
    }
}
