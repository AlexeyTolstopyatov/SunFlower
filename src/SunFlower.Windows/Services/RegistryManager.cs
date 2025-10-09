using System.Data;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SunFlower.Readers;

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
    public RegistryManager Of(string name)
    {
        _fileName = AppDomain.CurrentDomain.BaseDirectory + $"Registry\\{name}.json";
        return this;
    }
    /// <summary>
    /// Creates JSON list in \Registry\_fileName
    /// </summary>
    public RegistryManager Create()
    {
        File.WriteAllText(_fileName, "[]");
        return this;
    }
    
    public RegistryManager Delete(DataRow row, out bool success)
    {
        if (File.Exists(_fileName))
        {
            var name = row["Name"];
            var path = row["Path"];
            var type = row["Type"];
            var sign = row["Sign"];
            float size = float.Parse(row["Size"].ToString() ?? "0.0");

            var model = new FlowerBinaryReport((string)name, (string)path, size, (string)sign, (string)type);
            var file = JsonConvert.DeserializeObject<List<FlowerBinaryReport>>(File.ReadAllText(_fileName));

            if (file is null)
            {
                success = false;
                return this;
            }

            // try filter structures by target 
            if (!file.Contains(model))
            {
                success = false;
                return this; // <-- bad request.
            }

            file.Remove(model);

            // save changes
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(file));
        }
        success = true;
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
    /// <param name="obj">field what needs to be filled</param>
    /// <returns></returns>
    public RegistryManager Fill<T>(ref T obj)
    {
        if (obj == null) 
            throw new ArgumentNullException(nameof(obj));

        try
        {
            obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(_fileName))!;
        }
        catch
        {
            Of("recent").Create().Fill(ref obj); // recursive call ?
        }
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