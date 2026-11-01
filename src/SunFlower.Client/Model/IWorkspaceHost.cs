// CoffeeLake (C) 2026-*
// 
// The IWorkspaceHost.cs represents a public API of workspace host
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Threading.Tasks;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Model;

public interface IWorkspaceHost
{
    /// <summary>
    ///     Opens file in the most matching view
    /// </summary>
    /// <param name="filePath">file path</param>
    /// <param name="isPermanent">Have tab missed close/drag rights</param>
    Task OpenAsync(string filePath, bool isPermanent = false);

    /// <summary>
    ///     Executes flower plugin for a target file
    /// </summary>
    Task ExecuteFlowerAsync(FlowerData flower, string filePath);

    /// <summaies that working set is changed
    /// </summary>
    event EventHandler? OpenedDocumentsChanged;
}