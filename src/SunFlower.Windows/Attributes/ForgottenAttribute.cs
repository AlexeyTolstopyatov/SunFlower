namespace SunFlower.Windows.Attributes;

/// <summary>
/// If part of code stays too long, set it. 
/// </summary>
public class ForgottenAttribute : Attribute
{
    public override object TypeId { get; } = 1000;
}