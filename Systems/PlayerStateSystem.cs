namespace StellarSurvivors.Systems;
using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Components;
using StellarSurvivors.Enums;
using StellarSurvivors.Core;

public class PlayerStateSystem
{
    private EventManager _eventManager;
    private EntityManager _entityManager;
    private Game _world;
    
    public PlayerStateSystem(EventManager eventManager, EntityManager entityManager, Game world)
    {
        _eventManager = eventManager;
        _entityManager = entityManager;
        _world = world;

        // Listen for the 'E' key press
        eventManager.Subscribe<UseButtonPressedEvent>(OnUsePressed);
        
        // Listen for the *specific* event we're about to publish
        eventManager.Subscribe<ExitPodEvent>(OnExitPod);
        eventManager.Subscribe<ReturnToPodEvent>(OnReturnToPod);
    }

    // 1. 'E' was pressed. Decide what to do.
    private void OnUsePressed(UseButtonPressedEvent e)
    {
        if (_entityManager.PlayerInputs.Count == 0) return;
        int controlledEntityId = _entityManager.PlayerInputs.Keys.First();

        // Logic for Pod (Your existing code is good)
        if (_entityManager.Pods.ContainsKey(controlledEntityId))
        {
            var transform = _entityManager.Transforms[controlledEntityId];
            Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y);
            _eventManager.Publish(new ExitPodEvent 
            { 
                PodId = controlledEntityId, 
                PodPosition = pos 
            });
        }
        // Logic for Spaceman 
        else if (_entityManager.Spacemen.ContainsKey(controlledEntityId))
        {
            int podId = _entityManager.PodId; 

            // 2. Get both positions
            var spacemanTransform = _entityManager.Transforms[controlledEntityId];
            var podTransform = _entityManager.Transforms[podId];
        
            Vector2 spacemanPos = new Vector2(spacemanTransform.Position.X, spacemanTransform.Position.Y);
            Vector2 podPos = new Vector2(podTransform.Position.X, podTransform.Position.Y);

            // 3. Check the distance
            float distance = Vector2.Distance(spacemanPos, podPos);
            float reEntryRadius = 32f; // How close they need to be (e.g., 32 pixels)

            // 4. ONLY publish if close enough
            if (distance < reEntryRadius)
            {
                _eventManager.Publish(new ReturnToPodEvent
                {
                    SpacemanId = controlledEntityId,
                    PodId = podId
                });
            }
        }
    }

    // 2. The 'ExitPodEvent' was published. Do the logic.
    private void OnExitPod(ExitPodEvent e)
    {
        var fuel = _entityManager.Fuels[e.PodId];
        Console.WriteLine(fuel.CurrentFuel);
        if (fuel.ConsumeFuel(2))
        {
            _world.EntityManager.SetPodId(e.PodId);
            _entityManager.PlayerInputs.Remove(e.PodId);
            Vector3 podPosition = new Vector3(e.PodPosition.X, e.PodPosition.Y, 0);
            int spacemanId = _world.WorldGenerator.EntityFactory.CreateSpaceman(e.PodId, podPosition, new Vector2(52, 52), Color.Orange);
            AudioManager.PlaySfx("pew", .6f, 0.3f);
        }
        else Console.WriteLine("Not enough fuel");

    }
    
    private void OnReturnToPod(ReturnToPodEvent e) {
        var spacemanInventory = _entityManager.Inventories[e.SpacemanId];
        var podInventory =  _entityManager.Inventories[e.PodId];
        
        var spacemanTransform = _entityManager.Transforms[e.SpacemanId];
        Vector2 dropPosition = new Vector2(spacemanTransform.Position.X, spacemanTransform.Position.Y);
        
        // We loop through the spaceman's inventory
        foreach (var item in spacemanInventory.Contents)
        {
            ResourceType type = item.Key;
            int quantity = item.Value;
            Color color = GetColorForResourceType(type);

            // 2. Try to add the items to the pod's inventory
            if (!podInventory.TryAddItem(type, quantity))
            {
                // 3. If it fails (pod is full), drop the items
                //    by creating a new resource entity at the spaceman's location.
                //TODO: handle excess items when entering pod;
                //_world.CreateResourceEntity(dropPosition, type, quantity, color);
            }
        }
        
        spacemanInventory.ClearContents();
        _entityManager.PlayerInputs.Remove(e.SpacemanId);
        _entityManager.PlayerInputs.Add(e.PodId, new PlayerInputComponent(5.0f));
        _entityManager.SetControlledEntity(e.PodId);

        _entityManager.DestroyEntity(e.SpacemanId);
    }

    private Color GetColorForResourceType(ResourceType type)
    {
        // This switch is correct. It looks at the ITEM's type.
        switch (type)
        {
            case ResourceType.Stone:
                return Color.Gray;
            case ResourceType.IronOre:
                return new Color(211, 115, 63, 255); // A rusty orange
            case ResourceType.GoldOre:
                return Color.Gold;
            default:
                return Color.Magenta; // Easy to spot if a type is missing
        }
    }
}