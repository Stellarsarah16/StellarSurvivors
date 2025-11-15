namespace StellarSurvivors.Systems;
using StellarSurvivors.Core;
using StellarSurvivors.Components;

public class ResourceSystem
{
    private EntityManager _entityManager;

    public ResourceSystem(Game world)
    {
        _entityManager = world.EntityManager;
        
        // Subscribe to the event we care about
        world.EventManager.Subscribe<CollisionEvent>(OnCollision);
    }
    
    private void OnCollision(CollisionEvent e)
    {
        // 1. Get the current Spaceman's ID
        int spacemanId = _entityManager.SpacemanId;
        if (spacemanId == -1) return; // No spaceman, do nothing

        // 2. Check if this collision involves the spaceman
        if (e.EntityA == spacemanId)
        {
            // Entity A is the spaceman, try to pick up B
            TryPickup(e.EntityA, e.EntityB);
        }
        else if (e.EntityB == spacemanId)
        {
            // Entity B is the spaceman, try to pick up A
            TryPickup(e.EntityB, e.EntityA);
        }
    }
    
    private void TryPickup(int collectorId, int resourceId)
    {
        // 1. Check if we have the right components
        if (_entityManager.Inventories.TryGetValue(collectorId, out var inventory) &&
            _entityManager.Resources.TryGetValue(resourceId, out var resource))
        {
            // 2. Try to add the item to the inventory
            if (inventory.TryAddItem(resource.Type, resource.Quantity))
            {
                // 3. Success! Mark the resource entity for deletion
                // (Assuming your CleanupSystem looks for this component)
                _entityManager.DestroyTags.Add(resourceId, new DestroyComponent());
            }
        }
    }
}