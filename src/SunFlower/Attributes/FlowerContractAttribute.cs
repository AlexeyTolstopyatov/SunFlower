using SunFlower.Abstractions;

namespace SunFlower.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class FlowerContractAttribute : Attribute
{
    public string RequiredInterface { get; } = typeof(IFlowerSeed).FullName;
}