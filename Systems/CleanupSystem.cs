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
            // Destroys all entities with the Destroy Component
            foreach (var entityId in world.EntityManager.DestroyTags.Keys.ToList())
            {
                // We just call a helper method on the world
                // to remove this entity from *all* dictionaries.
                world.EntityManager.DestroyEntity(entityId);
            }
        }
    }
}