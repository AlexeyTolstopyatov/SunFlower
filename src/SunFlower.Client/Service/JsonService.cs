//
// CoffeeLake (C) 2026-*
// 
// The JsonService.cs represents API of JSON service for Sunflower data manipulating
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SunFlower.Client.Service;

public class JsonService<T>
{
    public List<T> Data { get; private set; } = [];

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };
    
    public async Task ReadAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("No such file.", path);

        var json = await File.ReadAllBytesAsync(path);
        using var stream = new MemoryStream(json);
        var data = await JsonSerializer.DeserializeAsync<List<T>>(stream);

        Data = data ?? [];
    }
    
    public async Task WriteAsync(string file)
    {
        // Escape creating new serializer options
        // reuse already initialized for all operations
        var json = JsonSerializer.Serialize(Data, _options);
        var recent = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", $"{file}.json");
        
        await File.WriteAllTextAsync(recent,json);
    }
}