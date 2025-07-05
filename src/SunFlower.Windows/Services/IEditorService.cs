namespace SunFlower.Windows.Services;

public interface IEditorService
{
    Task UpdateEditorContentAsync(string content);
    Task<string> GetEditorContentAsync();
}