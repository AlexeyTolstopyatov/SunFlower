// CoffeeLake (C) 2026-*
//
// ExecuteDialogView.axaml.cs - view for the Command Palette.
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.View;

public partial class ExecuteDialogView : UserControl
{
    public ExecuteDialogView()
    {
        InitializeComponent();

        // Observe KeyDown even after the ListBox marked it handled (arrow keys
        // move the ListBox selection and write it back through the TwoWay
        // binding); everything else is handled here.
        AddHandler(KeyDownEvent, OnKeyDownRouted, RoutingStrategies.Bubble, true);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ExecuteDialogViewModel vm)
        {
            vm.Prepare();
            SearchBox.Focus();
        }
    }

    private void OnKeyDownRouted(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ExecuteDialogViewModel vm)
            return;

        // ListBox handles arrows itself and syncs the selection back via binding.
        if (e.Handled)
            return;

        switch (e.Key)
        {
            case Key.Up:
            case Key.Down:
                e.Handled = true;
                if (e.Key == Key.Up)
                    vm.NavigateUpCommand.Execute(null);
                else
                    vm.NavigateDownCommand.Execute(null);
                CommandsList.Focus();
                break;

            case Key.Enter:
                e.Handled = true;
                vm.ExecuteSelectedCommand.Execute(null);
                break;

            case Key.Escape:
                e.Handled = true;
                vm.CloseCommand.Execute(null);
                break;
        }
    }
}
