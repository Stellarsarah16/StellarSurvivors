using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Components;

namespace StellarSurvivors.Systems;
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
            int podId = _entityManager.GetPodId(); 

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
        System.Console.WriteLine($"Exit pod: {e.PodId}");
        _world.EntityManager.SetPodId(e.PodId);
        _entityManager.PlayerInputs.Remove(e.PodId);
        _entityManager.Players.Remove(e.PodId);
        Vector3 podPosition = new Vector3(e.PodPosition.X, e.PodPosition.Y, 0);
        int spacemanId = _world.CreateSpaceman(e.PodId, podPosition, new Vector2(16,16), Color.Orange);
    }
    
    private void OnReturnToPod(ReturnToPodEvent e) {
        System.Console.WriteLine($"Return to pod: {e.PodId}");
        _entityManager.PlayerInputs.Remove(e.SpacemanId);
        _entityManager.Players.Remove(e.SpacemanId);
        _entityManager.PlayerInputs.Add(e.PodId, new PlayerInputComponent { Speed = 5.0f });
        _entityManager.Players.Add(e.PodId);
        _entityManager.DestroyEntity(e.SpacemanId);
    }
}