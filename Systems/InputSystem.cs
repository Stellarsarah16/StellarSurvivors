using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Core;

namespace StellarSurvivors.Systems
{
    public class InputSystem
    {
        // We need to know the UI bounds to prevent clicking "through" it.
        // These are passed in by the PlayingState.
        private Rectangle _topPanelBounds;
        private Rectangle _sidebarBounds;
        private Func<bool> _isSidebarOpen;
        private EventManager _eventManager;
        private EntityManager _entityManager;

        public InputSystem(EntityManager entityManager, EventManager eventManager, 
            Rectangle topPanel, Rectangle sidebar, Func<bool> isSidebarOpen)
        {
            _topPanelBounds = topPanel;
            _sidebarBounds = sidebar;
            _isSidebarOpen = isSidebarOpen;
            _eventManager  = eventManager;
            _entityManager  = entityManager;
            
        }

        public void Update(Game world)
        {
            if (_entityManager.PlayerInputs.Count == 0) {return;}
            int controlledEntityId = _entityManager.PlayerInputs.Keys.First();
            
            // --- Handle 'Use' Key ---
            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                // Just publish the event. It doesn't know what 'E' does.
                _eventManager.Publish(new UseButtonPressedEvent());
            }
            
            // --- UI Input ---
            if (Raylib.IsKeyPressed(KeyboardKey.M))
            {
                world.EventManager.Publish(new ToggleSidebarEvent());
            }

            // Check for mouse clicks *not* over the UI
            Vector2 mousePosition = Raylib.GetMousePosition();
            bool mouseOverUI = Raylib.CheckCollisionPointRec(mousePosition, _topPanelBounds) ||
                               (_isSidebarOpen() && Raylib.CheckCollisionPointRec(mousePosition, _sidebarBounds));
            
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && !mouseOverUI)
            {
                _eventManager.Publish(new ManualGoldClickEvent());
            }

            if (_entityManager.Pods.ContainsKey(controlledEntityId) || 
                _entityManager.Spacemen.ContainsKey(controlledEntityId))
            {
                // --- POD MOVEMENT LOGIC ---
                var velocity = _entityManager.Velocities[controlledEntityId];
                var transform = _entityManager.Transforms[controlledEntityId];

                if (Raylib.IsKeyDown(KeyboardKey.W)) //Thrust
                {
                    _eventManager.Publish(new ThrustInputEvent());
                }

                float direction = 0;
                if (Raylib.IsKeyDown(KeyboardKey.A)) direction = -1;
                if (Raylib.IsKeyDown(KeyboardKey.D)) direction = 1;
                if (direction != 0) 
                    _eventManager.Publish(new MoveInputEvent(direction));
                // IMPORTANT: Write changes back (if they are structs)
                _entityManager.Velocities[controlledEntityId] = velocity;
                _entityManager.Transforms[controlledEntityId] = transform;
            }
            
        }
    }
}