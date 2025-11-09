namespace StellarSurvivors.Systems;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;


using StellarSurvivors.Core;

public class FuelSystem : IUpdateSystem
{
    private readonly float _mothershipRechargeRate = 50.0f; // Fuel per second
    private readonly float _mothershipRechargeRadius = 64f;

    public void Update(Game world, float deltaTime)
    {
        Vector2? mothershipPosition = null;
        if (world.EntityManager.MotherShips.Count > 0)
        {
            int mothershipId = world.EntityManager.MotherShips.Keys.First();
            {
                var transform = world.EntityManager.Transforms[mothershipId];
                mothershipPosition = new Vector2(transform.Position.X, transform.Position.Y);
            }
        }
        
        // 2. Loop through all entities that have fuel
        foreach (var entityId in world.EntityManager.Fuels.Keys.ToList())
        {
            var fuel = world.EntityManager.Fuels[entityId];

            // --- A) Spaceman: Slow passive recharge (as you requested) ---
            if (world.EntityManager.Spacemen.ContainsKey(entityId))
            {
                // Only recharge if fuel is depleted
                if (fuel.CurrentFuel <= 0)
                {
                    // Spaceman no longer gets passive energy recharge
                    //fuel.CurrentFuel += fuel.RechargeRate * deltaTime;
                }
            }
                
            // --- B) Pod: Recharge at Mothership ---
            else if (world.EntityManager.Pods.ContainsKey(entityId) && mothershipPosition.HasValue)
            {
                var podTransform = world.EntityManager.Transforms[entityId];
                Vector2 podPos = new Vector2(podTransform.Position.X, podTransform.Position.Y);

                // Check distance
                if (Vector2.Distance(podPos, mothershipPosition.Value) < _mothershipRechargeRadius)
                {
                    fuel.CurrentFuel += _mothershipRechargeRate * deltaTime;
                }
                fuel.CurrentFuel += deltaTime * .1f;
            }
            
            

            // 3. Clamp fuel and write it back
            fuel.CurrentFuel = Math.Clamp(fuel.CurrentFuel, 0, fuel.FuelCapacity);
            world.EntityManager.Fuels[entityId] = fuel;
        }

    }
}