// CoffeeLake (C) 2026-*
//
// The StructLayoutDialogView.axaml.cs represents the view of the Struct Layout dialog.
// The dialog reinterprets a selected range of bytes as a packed struct described
// by a JSONC template ("Flower Notation" manifest).
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SunFlower.Client.View;

public partial class StructLayoutDialogView : UserControl
{
    public StructLayoutDialogView()
    {
        InitializeComponent();
    }
}