using SunFlower.Abstractions;

namespace SunFlower.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class FlowerContractAttribute : Attribute
{
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    public int BuildVersion { get; set; }
}