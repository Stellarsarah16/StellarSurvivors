// In Systems/CleanupSystem.cs
using System.Collections.Generic;
using System.Linq; // For .ToList()
using StellarSurvivors.Core;
using StellarSurvivors.Components;

namespace StellarSurvivors.Systems
{
    public class CleanupSystem : IUpdateSystem
    {
        public void Update(Game world,  float deltaTime)
        {
            // Query: "Give me all entities with a DestroyComponent"
            // We use .ToList() to create a *copy* of the keys,
            // so we can safely modify the original dictionaries.
            foreach (var entityId in world.EntityManager.DestroyTags.Keys.ToList())
            {
                // We just call a helper method on the world
                // to remove this entity from *all* dictionaries.
                world.RemoveEntity(entityId);
            }
        }
    }
}