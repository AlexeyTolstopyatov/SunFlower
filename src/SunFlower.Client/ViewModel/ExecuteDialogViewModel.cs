//
// CoffeeLake (C) 2026-*
//
// ExecuteDialogViewModel - ViewModel for the Command Palette (exactly ExecuteDialogView).
// Provides filtering, keyboard navigation and execution of palette entries
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Model;

namespace SunFlower.Client.ViewModel;

public partial class ExecuteDialogViewModel : DialogViewModel
{
    private readonly List<CommandPaletteItem> _all;

    [ObservableProperty] private bool _hasResults;

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private CommandPaletteItem? _selectedCommand;

    private int _selectedIndex = -1;

    public ExecuteDialogViewModel(IEnumerable<CommandPaletteItem>? commands = null)
    {
        _all = commands?.ToList() ?? [];
        Filter(string.Empty);
    }

    public ObservableCollection<CommandPaletteItem> FilteredCommands { get; } = [];

    partial void OnSearchTextChanged(string value)
    {
        // Magic .NET tricks what I've never known before this day had come....
        Filter(value);
    }

    partial void OnSelectedCommandChanged(CommandPaletteItem? value)
    {
        _selectedIndex = value is null
            ? -1
            : FilteredCommands.IndexOf(value);
    }

    /// <summary>
    ///     Call once the view is loaded: clears the search and selects the first entry.
    /// </summary>
    public void Prepare()
    {
        SearchText = string.Empty;
        SelectedCommand = FilteredCommands.Count > 0 ? FilteredCommands[0] : null;
    }

    private void Filter(string? query)
    {
        FilteredCommands.Clear();

        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var item in _all)
                FilteredCommands.Add(item);
        }
        else
        {
            var q = query;
            foreach (var item in _all
                         .Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                                     || (c.Description != null
                                         && c.Description.Contains(q, StringComparison.OrdinalIgnoreCase)))
                         .OrderBy(c => c.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1)
                         .ThenBy(c => c.Name.Length)
                         .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
                FilteredCommands.Add(item);
        }

        SelectedCommand = FilteredCommands.Count > 0
            ? FilteredCommands[0]
            : null;

        HasResults = FilteredCommands.Count > 0;
    }

    [RelayCommand]
    private void NavigateUp()
    {
        if (FilteredCommands.Count == 0)
            return;

        _selectedIndex = (_selectedIndex - 1 + FilteredCommands.Count) % FilteredCommands.Count;
        SelectedCommand = FilteredCommands[_selectedIndex];
    }

    [RelayCommand]
    private void NavigateDown()
    {
        if (FilteredCommands.Count == 0)
            return;

        _selectedIndex = (_selectedIndex + 1) % FilteredCommands.Count;
        SelectedCommand = FilteredCommands[_selectedIndex];
    }

    /// <summary>
    ///     Closes the palette first (so the UI stays snappy) and then runs the
    ///     selected entry's action. The action itself decides whether it needs
    ///     the UI thread (file pickers) or the thread pool (CPU-heavy plugins).
    /// </summary>
    [RelayCommand]
    private void ExecuteSelected()
    {
        var item = SelectedCommand;
        DialogService?.CloseDialog();

        if (item?.Action is { } action) _ = action();
    }

    [RelayCommand]
    private void Close()
    {
        DialogService?.CloseDialog();
    }
}