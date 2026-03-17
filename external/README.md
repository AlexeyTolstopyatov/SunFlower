This is an official build of `HandyControl`s assembly.

### To reproduce
Before those steps install .NET 7.0 and higher 

1. Fork or download HandyControl repository
2. Open project solution
3. Goto `HandyControls\src\Avalonia\HandyControl_Avalonia\bin\Debug\net7.0`
4. Copy resources (language name folder) and `HandyControl.dll`
5. Make a reference in your project

Avalonia logic differs with WPF. All included resources already contains in `.axaml`.
HandyControl features in Avalonia project are _styles_. To using them

6. Connect styles like this:

```xaml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="AvaloniaApp1.App"
             xmlns:local="using:AvaloniaApp1"
             RequestedThemeVariant="Default">
    <!-- "Default" ThemeVariant follows system theme variant. "Dark" or "Light" are other available options. -->
    ...

    <Application.Styles>
        <StyleInclude Source="avares://HandyControl/Themes/Theme.axaml"/>
    </Application.Styles>
    
    ...
</Application>
```

