namespace StellarSurvivors.Gameplay.Tools;
using System.Numerics;
using StellarSurvivors.Core;

public interface ITool
{
    float FuelCostPerSecond { get; }
    float FuelCostPerClick { get; }

    void Update(int ownerId, Vector2 mouseWorldPos, Game world);

    void Use(int ownerId, Vector2 targetWorldPos, float deltaTime, Game world, bool justPressed);
    
    void StopUse(int ownerId, Game world);
}