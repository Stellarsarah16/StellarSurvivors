namespace StellarSurvivors.Components;
using StellarSurvivors.Enums;

public class GeneratorComponent
{
    public string Name => "Basic Miner";
    public double ProductionPerSecond { get; set; }
    public ResourceType TargetResource { get; private set; }
    
    public GeneratorComponent(double productionPerSecond, ResourceType targetResource)
    {
        ProductionPerSecond = 1.0;
        TargetResource = targetResource;
    }
}