// CoffeeLake (C) 2026-*
// 
// The WorkspaceLayoutFactory.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace SunFlower.Client.ViewModel.Dock;

public class WorkspaceLayoutFactory : Factory
{
    public const string RootId = "Root";
    public const string MainLayoutId = "MainLayout";
    public const string LeftToolId = "LeftTool";
    public const string DocumentsId = "Documents";
    public const string ExplorerId = "SolutionExplorer";

    public override IRootDock CreateLayout()
    {
        var leftTool = new DocumentDock
        {
            Id = LeftToolId,
            Proportion = 0.25,
            CanDrag = true,
            CanDrop = true,
            VisibleDockables = CreateList<IDockable>(
                // new Document
                // {
                //     Id = "SolutionExplorer",
                //     Title = "Solution Explorer",
                //     CanDrag = true,
                //     CanDrop = true,
                //     CanFloat = false,
                //     CanDockAsDocument = true,
                //     CanPin = true
                // }
            )
        };

        var documentPane = new DocumentDock
        {
            Id = DocumentsId,
            CanDrag = true,
            CanDrop = true,
            VisibleDockables = CreateList<IDockable>()
        };
        var mainLayout = new ProportionalDock
        {
            Id = "MainLayout",
            VisibleDockables = CreateList<IDockable>(
                leftTool,
                new ProportionalDockSplitter(),
                documentPane
            ),
            CanDrop = true,
            CanDrag = true,
            CanDockAsDocument = true,
            CanFloat = false,
            AllowedDockOperations =
                DockOperationMask.Bottom
                | DockOperationMask.Fill
                | DockOperationMask.Left
                | DockOperationMask.Right
                | DockOperationMask.Top,
            AllowedDropOperations = DockOperationMask.All,
            DockingState = DockingWindowState.Document
        };

        var root = new RootDock
        {
            Id = "Root",
            CanClose = false,
            VisibleDockables = CreateList<IDockable>(mainLayout),
            ActiveDockable = mainLayout,
            AllowedDockOperations =
                DockOperationMask.Bottom
                | DockOperationMask.Fill
                | DockOperationMask.Left
                | DockOperationMask.Right
                | DockOperationMask.Top
        };

        return root;
    }
}