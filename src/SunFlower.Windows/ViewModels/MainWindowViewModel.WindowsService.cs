using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public partial class MainWindowViewModel
{
    /// <summary>
    /// Bridge between service methods and ViewModel parts
    /// </summary>
    /// <param name="viewModel">DataContext of target window</param>
    /// <typeparam name="TViewModel">Type of DataContext</typeparam>
    private void OpenChildWindowByDataContext<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        _windowsService.ShowWindow(viewModel);
    }
    /// <summary>
    /// Bridge between service methods and ViewModel parts
    /// </summary>
    /// <param name="viewModel">DataContext of target window</param>
    /// <typeparam name="TViewModel">Type of DataContext</typeparam>
    private void OpenChildWindowDialogByDataContext<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        ((WindowsService)_windowsService).ShowDialog(viewModel);
    }
}