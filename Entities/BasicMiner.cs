using StellarSurvivors.Interfaces;
using StellarSurvivors.Core;

namespace StellarSurvivors.Entities;

// BasicMiner.cs
public class BasicMiner : IGenerator
{
    public string Name => "Basic Miner";
    public double ProductionPerSecond { get; set; }

    public BasicMiner(double productionPerSecond)
    {
        this.ProductionPerSecond = productionPerSecond;
    }

    public void Update(float deltaTime, GameData state)
    {
        // Add this generator's production to the total gold
        state.CurrentGold += ProductionPerSecond * deltaTime;
    }
}