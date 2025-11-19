namespace StellarSurvivors.Components;
using StellarSurvivors.Enums;

public struct ResourceComponent
{
    public ResourceType Type;
    public int Quantity;

    public ResourceComponent(ResourceType type, int quantity)
    {
        Type = type;
        Quantity = quantity;
    }
}