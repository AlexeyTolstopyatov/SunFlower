using System.Windows.Controls;
using SunFlower.Windows.ViewModels;
using SunFlower.Windows.Views;
using Window = HandyControl.Controls.Window;

namespace SunFlower.Windows.Services;

public class WindowsService : NotifyPropertyChanged, IWindowsService
{
    private Dictionary<object, Window> _openWindowsDictionary = new();
    /// <summary>
    /// Observable property of opened windows
    /// </summary>
    public Dictionary<object, Window> OpenedWindowsDictionary
    {
        get => _openWindowsDictionary;
        set => SetField(ref _openWindowsDictionary, value);
    }

    /// <summary>
    /// Appends window to <see cref="OpenedWindowsDictionary"/> list
    /// Shows it like independent <see cref="DataGridWindow"/> (calls <c>Show</c> method)
    /// </summary>
    /// <param name="viewModel">DataContext for DataGridWindow</param>
    /// <typeparam name="TViewModel"></typeparam>
    public void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        if (OpenedWindowsDictionary.TryGetValue(viewModel, out Window? openWindow))
        {
            openWindow.Activate();
            return;
        }
        // be carefully...
        DataGridWindow window = new()
        {
            DataContext = viewModel
        };

        window.Closed += (s, e) => OpenedWindowsDictionary.Remove(viewModel);
        
        OpenedWindowsDictionary.Add(viewModel, window);
        window.Show();
    }
    /// <summary>
    /// Appends window to <see cref="OpenedWindowsDictionary"/> list,
    /// Initializes window like Dialog (calls <c>ShowDialog</c> overriding method)
    /// </summary>
    /// <param name="viewModel">DataContext</param>
    /// <typeparam name="TViewModel">type of DataContext</typeparam>
    public void ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        if (OpenedWindowsDictionary.TryGetValue(viewModel, out Window? openedWindow))
        {
            openedWindow.Activate();
            return;
        }

        Window window = new()
        {
            DataContext = viewModel
        };
        window.Closed += (s, e) =>
        {
            CloseWindow(viewModel);
        };

        window.ShowDialog();
    }
    /// <summary>
    /// Removes window instance from <see cref="OpenedWindowsDictionary"/> collection
    /// </summary>
    /// <param name="viewModel"></param>
    public void CloseWindow(object viewModel)
    {
        if (OpenedWindowsDictionary.TryGetValue(viewModel, out Window? window))
        {
            window.Close();
            OpenedWindowsDictionary.Remove(viewModel);
        }
    }
}