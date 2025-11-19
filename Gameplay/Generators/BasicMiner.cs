namespace StellarSurvivors.Gameplay.Generators;


using StellarSurvivors.Interfaces;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;
using System.Numerics;

// BasicMiner.cs
public class BasicMiner : IGenerator
{
    public string Name => "Basic Miner";
    public double ProductionPerSecond { get; set; }
    public Vector2 Position { get; set; }
    public ResourceType TargetResource { get; private set; }

    public BasicMiner(Vector2 position, double productionPerSecond, ResourceType targetResource)
    {
        this.Position = position;
        this.ProductionPerSecond = productionPerSecond;
        this.TargetResource = targetResource;
    }

    public void Update(float deltaTime, GameData state)
    {
        // Use the specific target resource type
        if (TargetResource != ResourceType.None)
        {
            state.AddResource(TargetResource, ProductionPerSecond * deltaTime);
        }
    }
}