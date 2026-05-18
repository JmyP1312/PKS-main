namespace LibraryManager.ViewModels
{
    public class SelectableItem<T> : ViewModelBase
    {
        private bool _isSelected;

        public T Item { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public SelectableItem(T item, bool isSelected = false)
        {
            Item = item;
            _isSelected = isSelected;
        }
    }
}
