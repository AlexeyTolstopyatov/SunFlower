namespace SunFlower.Abstractions.Types

open System
open System.Data
///
/// CoffeeLake 2025
/// this code licensed under (see GitHub repo) license
///
/// @creator: atolstopyatov2017@vk.com
///

/// <summary>
/// Type of Boxed Result of external plugin
/// combines "Header" as string, string of content before DataTable
/// and DataTable instance for structured as data storage 
/// </summary>
type Region(head : String, content : String, table : DataTable) = class
    /// <summary>
    /// Not stores Heading level yet. (means to be Heading of 3rd level or ### in Markdown)
    /// </summary>
    member val public Head : String = head
    /// <summary>
    /// Big String of description. Use StringBuilder to describe
    /// object of file what you want
    /// </summary>
    member val public Content : String = content
    /// <summary>
    /// Uses for structured data. Must be NOT NULL because
    /// client side checks correction of Region. If you DON't want DataTable
    /// -> use FlowerSeedEntryType::Strings instead.
    /// </summary>
    member val public Table : DataTable = table
    end
