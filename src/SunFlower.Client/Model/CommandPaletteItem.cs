// CoffeeLake (C) 2026-*
//
// The CommandPaletteItem.cs represents a single entry of the Command Palette
// shown by ExecuteDialogView: name, description, optional hotkey and the
// action invoked when the entry is executed.
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Threading.Tasks;

namespace SunFlower.Client.Model;

public sealed class CommandPaletteItem
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Hotkey { get; init; }
    public Func<Task>? Action { get; init; }
}
