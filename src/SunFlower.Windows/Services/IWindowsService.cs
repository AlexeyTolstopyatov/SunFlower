using System.Windows;
using Window = HandyControl.Controls.Window;

namespace SunFlower.Windows.Services;
/// <summary>
/// Represents bridge between parent-child entities
/// </summary>
public interface IWindowsService
{
    /// <summary>
    /// Contains list of Opened Child windows
    /// </summary>
    public Dictionary<object, Window> OpenedWindowsDictionary { get; set; }
    /// <summary>
    /// Initializes Window by targeting ViewModel
    /// </summary>
    /// <param name="viewModel">Targeting DataContext</param>
    /// <typeparam name="TViewModel">Type of DataContext</typeparam>
    public void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : class;
    /// <summary>
    /// Closes window and removes it from <see cref="OpenedWindowsDictionary"/>
    /// </summary>
    /// <param name="viewModel"></param>
    public void CloseWindow(object viewModel);
}