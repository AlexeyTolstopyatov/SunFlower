using System.Runtime.InteropServices;
using SunFlower.Windows.ViewModels;
using SunFlower.Windows.Views;
using Window = HandyControl.Controls.Window;

namespace SunFlower.Windows.Services;

/// <summary>
/// They say: ViewModel doesn't know about Views...
/// But I can't do it like this.
/// </summary>
public class WindowManager : NotifyPropertyChanged
{
    private Dictionary<object, object> _openWindowsDictionary = new();
    /// <summary>
    /// Observable property of opened windows
    /// </summary>
    public Dictionary<object, object> OpenedWindowsDictionary
    {
        get => _openWindowsDictionary;
        set => SetField(ref _openWindowsDictionary, value);
    }

    /// <summary>
    /// Appends window to <see cref="OpenedWindowsDictionary"/> list
    /// Shows it like independent <see cref="DataGridWindow"/> (calls <c>Show</c> method)
    /// </summary>
    /// <param name="viewModel">DataContext for DataGridWindow</param>
    /// <param name="windowInstance">Not initialized <see cref="Window"/> generic instance </param>
    /// <param name="isDialog">Optional field: initialize window and show it like Dialog </param>
    /// <param name="title">Window title</param>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TView"></typeparam>
    public void Show<TView, TViewModel>(TViewModel viewModel, TView windowInstance, [Optional] bool isDialog, [Optional] string title) where TView : Window, new()
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(windowInstance);
        
        if (OpenedWindowsDictionary.TryGetValue(viewModel, out var openWindow))
        {
            ((TView)openWindow).Activate();
            return;
        }
        
        windowInstance = new()
        {
            DataContext = viewModel
        };

        windowInstance.Title = title;
        windowInstance.Closed += (s, e) => OpenedWindowsDictionary.Remove(viewModel);
        
        OpenedWindowsDictionary.Add(viewModel, windowInstance);
        
        if (!isDialog)
            windowInstance.Show();
        else
            windowInstance.ShowDialog();
        
    }
    /// <summary>
    /// Force shows window
    /// </summary>
    /// <param name="windowInstance">window instance</param>
    /// <typeparam name="TView">type of window instance</typeparam>
    public void ShowUnmanaged<TView>(TView windowInstance, [Optional] bool isDialog, [Optional] string title) where TView : Window
    {
        windowInstance.Title = title;

        if (isDialog)
            windowInstance.ShowDialog();
        else
            windowInstance.Show();
    }
    /// <summary>
    /// Closes window by expected instance type
    /// </summary>
    /// <param name="viewModel">DataContext</param>
    /// <typeparam name="TView">expected type of window</typeparam>
    public void Close<TView>(object viewModel) where TView : Window, new()
    {
        if (!OpenedWindowsDictionary.TryGetValue(viewModel, out var window)) 
            return;

        ((TView)window).Close();
        OpenedWindowsDictionary.Remove(viewModel);
    }

    /// <summary>
    /// Destroys all window states when class
    /// deinitialize
    /// </summary>
    ~WindowManager()
    {
        OpenedWindowsDictionary.Clear();
    }
}