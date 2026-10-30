// CoffeeLake (C) 2026-*
// 
// The FlowerExecutable.cs represents extension of workspace host
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System.Threading.Tasks;
using SunFlower.Client.Service;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Model;
public sealed class FlowerExecutable(PluginAnalysisService analysis) : IFlowerExecutable
{
    public Task<PluginContentView> ExecuteFlowerAsync(FlowerData flower, string filePath) => analysis.AnalyzeAndSaveAsync(flower, filePath);
}