using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class VbImageView
{
    public List<Region> Regions { get; } = [];

    public VbImageView(Vb5Header header, Vb5ProjectTablesManager vb)
    {
        Regions.Add(new Vb5StructVisualizer(header).ToRegion());
        Regions.Add(new Vb5HeaderDetailsManager(vb).ToRegion());
        
        Regions.Add(new Vb5ProjectInfoVisualizer(vb.ProjectInfo).ToRegion());
        Regions.Add(new Vb5ComDataVisualizer(vb.Registration).ToRegion());
        Regions.Add(new Vb5ComInfoVisualizer(vb.RegistrationInfo).ToRegion());
    }

    public VbImageView(Vb4Header header)
    {
        // TODO: VB 4.0 runtime definitions and visualizers
    }
}