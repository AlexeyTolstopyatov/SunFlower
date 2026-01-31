using System.Collections.ObjectModel;
using System.Data;
using SunFlower.Abstractions.Types;

namespace SunFlower.Windows.ViewModels.Tree;
public class ResultNode : NotifyPropertyChanged
{
    private string _name;
    private object _value;
    private ObservableCollection<ResultNode> _children;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public object Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public ObservableCollection<ResultNode> Children
    {
        get => _children;
        set => SetField(ref _children, value);
    }

    public ResultNode(string name, object value = null!)
    {
        _name = name;
        _value = value;
        _children = [];
    }
}

public class PluginResultsTreeBuilder
{
    public static ObservableCollection<ResultNode> BuildTree(List<FlowerSeedResult> results)
    {
        var rootNodes = new ObservableCollection<ResultNode>();
        
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var resultNode = new ResultNode($"Result #{i + 1}");
            
            // Boxed type result
            resultNode.Children.Add(new ResultNode("Type", result.Type.ToString()));
            
            // Handle content depending on unboxed type 
            ProcessResultContent(resultNode, result);
            
            rootNodes.Add(resultNode);
        }
        
        return rootNodes;
    }

    private static void ProcessResultContent(ResultNode parent, FlowerSeedResult result)
    {
        if (result.BoxedResult == null)
        {
            parent.Children.Add(new ResultNode("Content", "<null>"));
            return;
        }

        switch (result.Type)
        {
            case FlowerSeedEntryType.Bytes:
                ProcessBytes(parent, result.BoxedResult);
                break;
                
            case FlowerSeedEntryType.Strings:
                ProcessStrings(parent, result.BoxedResult);
                break;
                
            case FlowerSeedEntryType.DataTables:
                ProcessDataTables(parent, result.BoxedResult);
                break;
                
            case FlowerSeedEntryType.Regions:
                ProcessRegions(parent, result.BoxedResult);
                break;
                
            case FlowerSeedEntryType.Empty:
                parent.Children.Add(new ResultNode("Content", "<empty>"));
                break;
        }
    }

    private static void ProcessBytes(ResultNode parent, object boxedBytes)
    {
        if (boxedBytes is not byte[] bytes) 
            return;
        
        var bytesNode = new ResultNode($"Bytes [{bytes.Length}]");
        var showCount = Math.Min(bytes.Length, 100);
        var preview = string.Join(" ", bytes.Take(showCount).Select(b => b.ToString("X2")));
            
        if (bytes.Length > 100)
            preview += "...";
            
        bytesNode.Children.Add(new ResultNode("Preview", preview));
        bytesNode.Children.Add(new ResultNode("Length", $"{bytes.Length} bytes"));
            
        if (bytes.Length > 20)
        {
            for (var i = 0; i < Math.Min(5, bytes.Length); i += 20)
            {
                var chunkSize = Math.Min(20, bytes.Length - i);
                var chunk = bytes.Skip(i).Take(chunkSize).ToArray();
                var chunkNode = new ResultNode($"Offset 0x{i:X4}");
                chunkNode.Children.Add(new ResultNode("Hex", 
                    string.Join(" ", chunk.Select(b => b.ToString("X2")))));
                    
                var ascii = new string(chunk.Select(b => 
                    b >= 32 && b <= 126 ? (char)b : '.').ToArray());
                chunkNode.Children.Add(new ResultNode("ASCII", ascii));
                    
                bytesNode.Children.Add(chunkNode);
            }
        }
            
        parent.Children.Add(bytesNode);
    }

    private static ResultNode ProcessDataTable(DataTable table, string name)
    {
        var tableNode = new ResultNode($"{name}: {table.TableName}");
        
        tableNode.Children.Add(new ResultNode("Name", table.TableName));
        tableNode.Children.Add(new ResultNode("Columns", table.Columns.Count));
        tableNode.Children.Add(new ResultNode("Rows", table.Rows.Count));
        
        if (table.Columns.Count > 0)
        {
            var schemaNode = new ResultNode("Schema");
            foreach (DataColumn col in table.Columns)
            {
                var colNode = new ResultNode(col.ColumnName);
                colNode.Children.Add(new ResultNode("Type", col.DataType.Name));
                colNode.Children.Add(new ResultNode("Nullable", col.AllowDBNull.ToString()));
                schemaNode.Children.Add(colNode);
            }
            tableNode.Children.Add(schemaNode);
        }

        if (table.Rows.Count <= 0) 
            return tableNode;
    
        var sampleNode = new ResultNode("Sample Data");
        
        var rowsToShow = Math.Min(10, table.Rows.Count);
        var colsToShow = Math.Min(10, table.Columns.Count);
        
        for (var r = 0; r < rowsToShow; r++)
        {
            var row = table.Rows[r];
            var rowNode = new ResultNode($"Row {r + 1}");
            
            for (var c = 0; c < colsToShow; c++)
            {
                var col = table.Columns[c];
                var value = row[col];
                var valueStr = Convert.IsDBNull(value) 
                    ? "<null>" 
                    : value.ToString();
                
                // Trim huge object metadata
                if (valueStr != null && valueStr.Length > 50)
                {
                    valueStr = valueStr.Substring(0, 47) + "...";
                }

                if (valueStr != null) 
                    rowNode.Children.Add(new ResultNode(col.ColumnName, valueStr));
            }
            
            sampleNode.Children.Add(rowNode);
        }
        
        if (table.Rows.Count > rowsToShow)
        {
            sampleNode.Children.Add(new ResultNode("...", 
                $"and {table.Rows.Count - rowsToShow} more rows"));
        }
        
        if (table.Columns.Count > colsToShow)
        {
            sampleNode.Children.Add(new ResultNode("...", 
                $"and {table.Columns.Count - colsToShow} more columns"));
        }
        
        tableNode.Children.Add(sampleNode);
        
        return tableNode;
    }
    
    private static void ProcessStrings(ResultNode parent, object boxedStrings)
    {
        if (boxedStrings is not System.Collections.IEnumerable stringEnumerable) 
            return;
        var stringsNode = new ResultNode("Strings");
        var count = 0;
            
        foreach (var str in stringEnumerable)
        {
            if (str is string s)
            {
                stringsNode.Children.Add(new ResultNode($"String #{++count}", 
                    s.Length > 50 ? s[..47] + "..." : s));
            }
        }
            
        stringsNode.Children.Insert(0, new ResultNode("Count", count));
        parent.Children.Add(stringsNode);
    }

    private static void ProcessDataTables(ResultNode parent, object boxedTables)
    {
        if (boxedTables is not DataTable[] tables) 
            return;
        
        var tablesNode = new ResultNode($"DataTables [{tables.Length}]");
            
        for (var i = 0; i < tables.Length; i++)
        {
            var table = tables[i];
            var tableNode = new ResultNode($"Table {i + 1}: {table.TableName}");
                
            tableNode.Children.Add(new ResultNode("Columns", table.Columns.Count));
            tableNode.Children.Add(new ResultNode("Rows", table.Rows.Count));
                
            var schemaNode = new ResultNode("Schema");
            foreach (DataColumn col in table.Columns)
            {
                schemaNode.Children.Add(new ResultNode(col.ColumnName, col.DataType.Name));
            }
            tableNode.Children.Add(schemaNode);
                
            if (table.Rows.Count > 0)
            {
                var sampleNode = new ResultNode("Sample Data (first 3 rows)");
                for (var r = 0; r < Math.Min(3, table.Rows.Count); r++)
                {
                    var row = table.Rows[r];
                    var rowNode = new ResultNode($"Row {r + 1}");
                        
                    for (var c = 0; c < Math.Min(5, table.Columns.Count); c++)
                    {
                        var col = table.Columns[c];
                        rowNode.Children.Add(new ResultNode(col.ColumnName, 
                            row[col].ToString() ?? "<null>"));
                    }
                        
                    sampleNode.Children.Add(rowNode);
                }
                tableNode.Children.Add(sampleNode);
            }
                
            tablesNode.Children.Add(tableNode);
        }
            
        parent.Children.Add(tablesNode);
    }
    private static void ProcessRegions(ResultNode parent, object boxedRegions)
    {
        var regionsNode = new ResultNode("Regions");
        
        switch (boxedRegions)
        {
            case System.Collections.IEnumerable regionEnumerable:
            {
                var count = 0;
                foreach (var regionObj in regionEnumerable)
                {
                    if (regionObj is not Region region) 
                        continue;
                    
                    count++;
                    var regionNode = new ResultNode($"Region #{count}: {region.Head}");
                    
                    // Firstly, prepare section Heading
                    regionNode.Children.Add(new ResultNode("Header", region.Head));
                    if (!string.IsNullOrWhiteSpace(region.Content))
                    {
                        var contentNode = new ResultNode("Content");
                        
                        // If content not so long, let it see
                        // 100..200 rows can be ok, I suppose
                        var preview = region.Content.Length > 200 
                            ? region.Content[..197] + "..." 
                            : region.Content;
                        
                        contentNode.Children.Add(new ResultNode("Preview", preview));
                        contentNode.Children.Add(new ResultNode("Length", $"{region.Content.Length} characters"));
                        
                        // If content very huge -> next tree node must be initialized
                        if (region.Content.Length > 200)
                        {
                            var fullContentNode = new ResultNode("Full Content");
                            
                            var lines = region.Content.Split('\n');
                            for (var i = 0; i < Math.Min(lines.Length, 50); i++)
                            {
                                fullContentNode.Children.Add(new ResultNode($"Line {i + 1}", 
                                    lines[i].Length > 100 ? lines[i][..97] + "..." : lines[i]));
                            }
                            if (lines.Length > 50)
                            {
                                fullContentNode.Children.Add(new ResultNode("...", 
                                    $"and {lines.Length - 50} more lines"));
                            }
                            contentNode.Children.Add(fullContentNode);
                        }
                        
                        regionNode.Children.Add(contentNode);
                    }
                    else
                    {
                        regionNode.Children.Add(new ResultNode("Content", "Empty"));
                    }
                    
                    if (region.Table != null)
                    {
                        var tableNode = ProcessDataTable(region.Table, "Table");
                        regionNode.Children.Add(tableNode);
                    }
                    else
                    {
                        regionNode.Children.Add(new ResultNode("Table", "null"));
                    }
                    
                    regionsNode.Children.Add(regionNode);
                }
            
                regionsNode.Children.Insert(0, new ResultNode("Count", count));
                break;
            }
            case Region singleRegion:
            {
                var regionNode = new ResultNode($"Region: {singleRegion.Head}");
                regionNode.Children.Add(new ResultNode("Header", singleRegion.Head));
            
                if (!string.IsNullOrWhiteSpace(singleRegion.Content))
                {
                    regionNode.Children.Add(new ResultNode("Content", 
                        singleRegion.Content.Length > 100 
                            ? singleRegion.Content[..97] + "..." 
                            : singleRegion.Content));
                }
            
                if (singleRegion.Table != null)
                {
                    var tableNode = ProcessDataTable(singleRegion.Table, "Table");
                    regionNode.Children.Add(tableNode);
                }
            
                regionsNode.Children.Add(regionNode);
                regionsNode.Children.Insert(0, new ResultNode("Count", 1));
                break;
            }
        }
        
        parent.Children.Add(regionsNode);
    }
}