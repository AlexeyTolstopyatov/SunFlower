using System.Data;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SunFlower.Windows.Attributes;

namespace SunFlower.Windows.Services;

public sealed class RegistryManager
{
    private string _fileName;
    public static RegistryManager CreateInstance() => new(string.Empty);
    
    /// <param name="fileName"> JUST file name. Without path and extension. </param>
    private RegistryManager(string fileName)
    {
        _fileName = AppDomain.CurrentDomain.BaseDirectory + $"Registry\\{fileName}.json";
    }
    /// <param name="name"> JUST file name. Without path and extension. </param>
    public RegistryManager SetFileName(string name)
    {
        _fileName = AppDomain.CurrentDomain.BaseDirectory + $"Registry\\{name}.json";
        return this;
    }
    /// <summary>
    /// Creates JSON list in \Registry\_fileName
    /// </summary>
    public RegistryManager Create()
    {
        File.WriteAllText("[]", _fileName);
        return this;
    }
    
    [Forgotten]
    public RegistryManager Delete(DataRow row)
    {
        
        return this;
    }
    /// <summary>
    /// Creates entry in current file from DataTable row
    /// </summary>
    public RegistryManager Create<T>(T @struct)
    {
        JArray resultList;
        
        if (File.Exists(_fileName))
        {
            var recentJson = File.ReadAllText(_fileName);
            resultList = JArray.Parse(recentJson);
            
            var openedFileObj = JObject.FromObject(@struct!);
            resultList.Add(openedFileObj);
        
            File.WriteAllText(_fileName, resultList.ToString(Formatting.Indented));
        }
        else
        {
            File.CreateText(_fileName);
            
            var openedFileObj = JObject.FromObject(@struct!);
            resultList = [openedFileObj];

            File.WriteAllText(_fileName, resultList.ToString(Formatting.Indented));
        }
        return this;
    }
    /// <summary>
    /// Deserializes JSON list to <see cref="DataTable"/>
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public RegistryManager Fill<T>(ref T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        string json = File.ReadAllText(_fileName);
        
        obj = JsonConvert.DeserializeObject<T>(json)!;
        
        return this;
    }
    /// <summary>
    /// Seriously. Calls notepad with current file in argument vector
    /// </summary>
    /// <returns></returns>
    public RegistryManager OpenInWindowsNotepad()
    {
        Process.Start("notepad.exe", _fileName);
        return this;
    }
}