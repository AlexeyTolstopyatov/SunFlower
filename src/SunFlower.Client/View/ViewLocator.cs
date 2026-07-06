// CoffeeLake (C) 2026-*
// 
// The ViewLocator.cs represents View detection by given ViewModel  
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.View;

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        var view = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(view);
        
        if (type == null)
            return new TextBlock { Text = $"The {view} not found" };

        if (!type.IsSubclassOf(typeof(Window))) 
            return (Control)Activator.CreateInstance(type)!;
        
        var window = (Window)Activator.CreateInstance(type)!; // why parent closes and new window appears???
        window.Show();
        
        return new TextBlock { Text = $"The {view} initialized" };
    }

    public bool Match(object data)
    {
        return data is ObservableObject;
    }
}