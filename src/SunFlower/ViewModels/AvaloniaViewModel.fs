namespace SunFlower.ViewModels

open CommunityToolkit.Mvvm.ComponentModel
// CoffeeLake (C) 2026-*
// MIT
//
// This file contains Avalonia abstractions to the MVVM construct
// Application bases at the ReactiveUI
[<AbstractClass>]
type AvaloniaViewModel() =
    inherit ObservableObject()
