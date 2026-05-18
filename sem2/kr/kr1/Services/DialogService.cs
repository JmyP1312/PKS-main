using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace LibraryManager.Services
{
    public static class DialogService
    {
        private static Window? _mainWindow;

        public static void Initialize(Window window) => _mainWindow = window;

        public static Window? MainWindow => _mainWindow;

        public static async Task ShowAsync(string message, string title = "Информация")
        {
            if (_mainWindow == null) return;
            var box = MessageBoxManager.GetMessageBoxStandard(title, message);
            await box.ShowWindowDialogAsync(_mainWindow);
        }

        public static async Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
        {
            if (_mainWindow == null) return false;
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo);
            var result = await box.ShowWindowDialogAsync(_mainWindow);
            return result == ButtonResult.Yes;
        }
    }
}
