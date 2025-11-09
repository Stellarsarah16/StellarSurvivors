// In Systems/HealthSystem.cs
using System.Collections.Generic; // For List
using System.Linq;
using StellarSurvivors.Core;
using StellarSurvivors.Components; // So we can see HealthComponent

namespace StellarSurvivors.Systems
{
    public class HealthSystem : IUpdateSystem
    {
        public void Update(Game world, float deltaTime)
        {
            List<int> entitiesToDestroy = new List<int>();

            // Query: "Give me all entities with a HealthComponent"
            foreach (var entityId in world.EntityManager.Healths.Keys)
            {
                var health = world.EntityManager.Healths[entityId];

                //  Is there any damage in the buffer?
                if (health.DamageToApply.Count > 0)
                {
                    // Apply the damage
                    int totalDamage = health.DamageToApply.Sum();
                    health.CurrentHealth -= totalDamage;

                    // Clear the buffer so we don't apply damage twice
                    health.DamageToApply.Clear();

                    // Optional: Log the hit
                    // System.Console.WriteLine($"Entity {entityId} took {totalDamage} damage. Health: {health.CurrentHealth}");
                }

                //  Check for death
                if (health.CurrentHealth <= 0)
                {
                    // Don't destroy it yet! Just add it to the list.
                    entitiesToDestroy.Add(entityId);
                }
            }

            // --- Now, outside the main loop, we can safely tag entities ---
            foreach (var entityId in entitiesToDestroy)
            {
                // Only add the tag if it's not already dead
                if (!world.EntityManager.DestroyTags.ContainsKey(entityId))
                {
                    // System.Console.WriteLine($"Entity {entityId} has died.");
                    world.EntityManager.DestroyTags.Add(entityId, new DestroyComponent());
                }
            }
        }
    }
}