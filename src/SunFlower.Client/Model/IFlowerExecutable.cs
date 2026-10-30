// CoffeeLake (C) 2026-*
// 
// The IFlowerExecutable.cs represents possible extension for workspaces
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System.Threading.Tasks;
using SunFlower.Client.Service;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Model;

public interface IFlowerExecutable
{
    Task<PluginContentView> ExecuteFlowerAsync(FlowerData flower, string filePath);
}