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

    // Constructor: Subscribe
    public PlayerStateSystem(EventManager eventManager, EntityManager entityManager, Game world)
    {
        _eventManager = eventManager;
        _entityManager = entityManager;
        _world = world;

        // Listen for the 'E' key press
        eventManager.Subscribe<UseButtonPressedEvent>(OnUsePressed);
        
        // Listen for the *specific* event we're about to publish
        eventManager.Subscribe<ExitPodEvent>(OnExitPod);
    }

    // 1. 'E' was pressed. Decide what to do.
    private void OnUsePressed(UseButtonPressedEvent e)
    {
        if (_entityManager.PlayerInputs.Count == 0) return;
        int controlledEntityId = _entityManager.PlayerInputs.Keys.First();

        // If we are controlling a Pod...
        if (_entityManager.Pods.ContainsKey(controlledEntityId))
        {
            // ...then 'E' means "Exit Pod".
            // Publish a new, more specific event.
            var transform = _entityManager.Transforms[controlledEntityId];
            Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y);
            _eventManager.Publish(new ExitPodEvent 
            { 
                PodId = controlledEntityId, 
                PodPosition = pos 
            });
        }
        // If we are controlling a Spaceman...
        else if (_entityManager.Spacemen.ContainsKey(controlledEntityId))
        {
            // ...then 'E' means "Enter Pod".
            // TODO: Check if near pod
            // _eventManager.Publish(new EnterPodEvent { ... });
        }
    }

    // 2. The 'ExitPodEvent' was published. Do the logic.
    private void OnExitPod(ExitPodEvent e)
    {
        _entityManager.PlayerInputs.Remove(e.PodId);
        _entityManager.Players.Remove(e.PodId);
        
        
        Vector3 podPosition = new Vector3(e.PodPosition.X, e.PodPosition.Y, 0);
        int spacemanId = _world.CreateSpaceman(e.PodId, podPosition, new Vector2(16,16), Color.Orange);
        
        
        
    }
}