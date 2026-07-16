// CoffeeLake (C) 2026-*
//
// WorkspaceWindow container for the workspace view.
// ViewModel opens it via a service, not by creating a Window directly.
//
using System;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using SunFlower.Client.Service;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.View;

public partial class WorkspaceWindow : Window
{
    private readonly Animation _dialogOpenAnimation;
    private readonly Animation _dialogCloseAnimation;
    
    public DialogService DialogService { get; }

    public WorkspaceWindow()
    {
        InitializeComponent();
        DialogService = new DialogService(this);
        _dialogOpenAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.1),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0.0),
                        new Setter(ScaleTransform.ScaleYProperty, 0.0),
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                    },
                    KeyTime = TimeSpan.FromSeconds(0.1)
                }
            }
        };
        _dialogCloseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                    },
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleYProperty, 0.0),
                        new Setter(ScaleTransform.ScaleXProperty, 0.0),
                    }
                }
            }
        };
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Clean up project when window is closed
        if (DataContext is WorkspaceViewModel vm)
        {
            // The ViewModel shouldn't know about Window closing,
            // -> handle cleanup here via the service.
            // MainWindow handles the save dialog on app exit.
        }
    }

    private void DialogContainerClosed(object? sender, EventArgs e)
    {
        _dialogCloseAnimation.RunAsync(DialogPresenter);
        DialogLayer.IsVisible = false;
    }

    private void DialogContainerKeyHandled(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) 
            return;

        DialogContainer.IsOpen = false;
    }

    private void DialogContainerOpened(object? sender, EventArgs e)
    {
        DialogLayer.IsVisible = true;
        //_dialogOpenAnimation.RunAsync(DialogPresenter);
    }

    private void DialogPresenterSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _dialogOpenAnimation.RunAsync(DialogPresenter);
    }
}
