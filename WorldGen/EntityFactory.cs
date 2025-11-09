namespace StellarSurvivors.WorldGen;
using System.Collections.Generic;
using System.Numerics;
using StellarSurvivors.Core;
using StellarSurvivors.WorldGen.Blueprints; // For the interface

public class EntityFactory
{

    private Dictionary<string, IEntityBlueprint> _blueprints;
    private Game _world;

    public EntityFactory(Game world)
    {
        _world = world;
        _blueprints = new Dictionary<string, IEntityBlueprint>();
    }
    
    public void Register(string blueprintId, IEntityBlueprint blueprint)
    {
        _blueprints[blueprintId] = blueprint;
        // Or: _blueprints.Add(blueprintId, blueprint);
    }
    
    public int CreateMapEntity(string blueprintId, BlueprintSettings settings)
    {
        if (!_blueprints.ContainsKey(blueprintId))
        {
            System.Console.WriteLine($"Error: No blueprint registered for ID '{blueprintId}'");
            return -1; // Return an invalid ID
        }

        IEntityBlueprint blueprint = _blueprints[blueprintId];
        int newEntityId = _world.EntityManager.CreateNewEntityId();
        blueprint.Apply(_world, newEntityId, settings);

        return newEntityId;
    }
}

