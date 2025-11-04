namespace StellarSurvivors.WorldGen;
using System.Collections.Generic;
using System.Numerics;
using StellarSurvivors.Core;
using StellarSurvivors.WorldGen.Blueprints; // For the interface

public class EntityFactory
{
    // The "cookbook"
    private Dictionary<string, IEntityBlueprint> _blueprints;

    // The factory *needs* a reference to the world
    // so it can pass it to the blueprints.
    private Game _world;

    public EntityFactory(Game world)
    {
        _world = world;
        _blueprints = new Dictionary<string, IEntityBlueprint>();
    }

    // This is how we add "recipes" to the "cookbook"
    public void Register(string blueprintId, IEntityBlueprint blueprint)
    {
        _blueprints[blueprintId] = blueprint;
        // Or: _blueprints.Add(blueprintId, blueprint);
    }

    // This is the main method!
    public int CreateMapEntity(string blueprintId, BlueprintSettings settings)
    {
        if (!_blueprints.ContainsKey(blueprintId))
        {
            System.Console.WriteLine($"Error: No blueprint registered for ID '{blueprintId}'");
            return -1; // Return an invalid ID
        }

        // 1. Get the blueprint (recipe)
        IEntityBlueprint blueprint = _blueprints[blueprintId];

        // 2. Get a new ID from the world
        int newEntityId = _world.EntityManager.CreateNewEntityId();

        // 3. Tell the blueprint to "apply" itself
        blueprint.Apply(_world, newEntityId, settings);

        // 4. Return the new entity's ID
        return newEntityId;
    }
}

