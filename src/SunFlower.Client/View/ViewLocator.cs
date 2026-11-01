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
    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "<null>" };

        var type = data.GetType();

        var viewType = type.Namespace?.Replace("ViewModel", "View") + "." +
                       type.Name.Replace("ViewModel", "View");
        var typeOfView = Type.GetType(viewType);

        if (typeOfView == null)
            return new TextBlock { Text = $"The {viewType} not found" };

        if (!typeOfView.IsSubclassOf(typeof(Window)))
            return (Control)Activator.CreateInstance(typeOfView)!;

        return new TextBlock { Text = $"The {viewType} initialized" };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}