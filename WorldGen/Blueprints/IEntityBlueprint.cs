using System.Numerics;
using StellarSurvivors.Core;

namespace StellarSurvivors.WorldGen.Blueprints;

public interface IEntityBlueprint
{
    void Apply(Game world, int entityId, BlueprintSettings settings);
}