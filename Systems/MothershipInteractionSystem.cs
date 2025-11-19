namespace StellarSurvivors.Systems;
using StellarSurvivors.Core;
using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Enums;

// In Systems/MothershipInteractionSystem.cs
public class MothershipInteractionSystem
{
    private EntityManager _entityManager;
    private EventManager _eventManager;
    private const float INTERACT_RANGE_SQ = 100 * 100; // 100 pixel range
    private float _fuelRate;

    public MothershipInteractionSystem(Game world)
    {
        _entityManager = world.EntityManager;
        _eventManager = world.EventManager;
        
        // Subscribe to the new events
        _eventManager.Subscribe<RefineFuelEvent>(OnRefineFuel);
        _eventManager.Subscribe<RechargePodEvent>(OnRechargePod);
    }
    
    private void OnRefineFuel(RefineFuelEvent e)
    {
        // Get the IDs
        int podId = _entityManager.PodId;
        int mothershipId = _entityManager.MothershipId;

        // Check if pod and ship exist
        if (podId == -1 || mothershipId == -1) return; 
        
        // Check if we are in range
        if (!IsPodInRange(podId, mothershipId)) return;
        
        // --- Run the refine logic ---
        var podInv = _entityManager.Inventories[podId];
        var shipFuel = _entityManager.Fuels[mothershipId];
        var mothership = _entityManager.MotherShips[mothershipId];

        if (shipFuel.CurrentFuel < shipFuel.FuelCapacity && 
            podInv.TryRemoveItem(ResourceType.Coal, 1))
        {
            AudioManager.PlaySfx("reload", 1, .9f);
            shipFuel.CurrentFuel += mothership.RechargeRate ;
            _entityManager.Fuels[mothershipId] = shipFuel;
        }
    }
    
    private void OnRechargePod(RechargePodEvent e)
    {
        // Get the IDs
        int podId = _entityManager.PodId;
        int mothershipId = _entityManager.MothershipId;

        // Check if pod and ship exist
        if (podId == -1 || mothershipId == -1) return;

        // Check if we are in range
        if (!IsPodInRange(podId, mothershipId)) return;
        
        // --- Run the recharge logic ---
        var podFuel = _entityManager.Fuels[podId];
        var shipFuel = _entityManager.Fuels[mothershipId];
        var mothership = _entityManager.MotherShips[mothershipId];

        if (shipFuel.CurrentFuel > 0 && podFuel.CurrentFuel < podFuel.FuelCapacity)
        {
            shipFuel.CurrentFuel--;
            podFuel.CurrentFuel += mothership.RechargeRate;
            AudioManager.PlaySfx("reload", 1, 1);
            
            _entityManager.Fuels[podId] = podFuel;
            _entityManager.Fuels[mothershipId] = shipFuel;
        }
    }

    // Helper to check for range
    private bool IsPodInRange(int podId, int mothershipId)
    {
        var podPos = _entityManager.Transforms[podId].Position;
        var shipPos = _entityManager.Transforms[mothershipId].Position;
        return Vector3.DistanceSquared(podPos, shipPos) < INTERACT_RANGE_SQ;
    }
}