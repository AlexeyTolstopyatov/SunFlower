// CoffeeLake (C) 2026-*
// 
// The FileDocument.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Dock.Model.Controls;
using Document = Dock.Model.Mvvm.Controls.Document;

namespace SunFlower.Client.ViewModel.Dock;

public abstract class FileDocument : Document, IDocument
{
    public string FilePath { get; set; } = "";
}