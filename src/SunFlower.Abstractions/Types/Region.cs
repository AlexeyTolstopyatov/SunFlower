using System.Data;

namespace SunFlower.Abstractions.Types;

public class Region(string head, string content, string tableName)
{
    public string Head { get; set; } = head;
    public string Content { get; set; } = content;
    public DataTable Table { get; set; } = new(tableName);
}