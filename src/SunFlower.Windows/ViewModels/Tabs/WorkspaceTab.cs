namespace SunFlower.Windows.ViewModels.Tabs;


public abstract class WorkspaceTab : NotifyPropertyChanged
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; set; } = nameof(WorkspaceTab);
    public string? Icon { get; set; }
    public bool CanClose { get; set; }
}
