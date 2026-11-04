// CoffeeLake (C) 2026-*
// 
// The ViewLocator.cs represents View detection by given ViewModel  
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.View;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "ViewModel dereference is <null>", Foreground = SolidColorBrush.Parse("#FFFF0000") };
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type == null) 
            return new TextBlock { Text = "Not Found: " + name };
        
        var control = (Control)Activator.CreateInstance(type)!;
    
        control.DataContext = data; 
        return control;
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}