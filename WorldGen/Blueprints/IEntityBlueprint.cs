using System.Numerics;
using StellarSurvivors.Core;

namespace StellarSurvivors.WorldGen.Blueprints;

public interface IEntityBlueprint
{
    void Apply(EntityManager entityManager, int entityId, BlueprintSettings settings);
}