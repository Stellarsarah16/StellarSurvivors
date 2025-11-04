using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Core;

namespace StellarSurvivors.Systems
{
    /// <summary>
    /// Implements IUpdateSystem.
    /// Reads raw hardware input and publishes events.
    /// It has NO game logic.
    /// </summary>
    public class InputSystem : IUpdateSystem
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

        public void Update(Game world, float deltaTime)
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

            if (_entityManager.Pods.ContainsKey(controlledEntityId))
            {
                // --- POD MOVEMENT LOGIC ---
                // (This is your existing code)
                var velocity = _entityManager.Velocities[controlledEntityId];
                var transform = _entityManager.Transforms[controlledEntityId];

                if (Raylib.IsKeyDown(KeyboardKey.Space)) //Thrust
                {
                    _eventManager.Publish(new ThrustInputEvent());
                }

                float rotationDir = 0;
                if (Raylib.IsKeyDown(KeyboardKey.A)) rotationDir = -1;
                if (Raylib.IsKeyDown(KeyboardKey.D)) rotationDir = 1;
                if (rotationDir != 0) 
                    _eventManager.Publish(new RotationInputEvent { Direction = rotationDir });
                // IMPORTANT: Write changes back (if they are structs)
                _entityManager.Velocities[controlledEntityId] = velocity;
                _entityManager.Transforms[controlledEntityId] = transform;
            }
            
            else if (_entityManager.Spacemen.ContainsKey(controlledEntityId))
            {
                // --- Player Movement Input ---
                Vector2 moveDirection = Vector2.Zero;
                if (Raylib.IsKeyDown(KeyboardKey.W)) moveDirection.Y -= 1;
                if (Raylib.IsKeyDown(KeyboardKey.S)) moveDirection.Y += 1;
                if (Raylib.IsKeyDown(KeyboardKey.A)) moveDirection.X -= 1;
                if (Raylib.IsKeyDown(KeyboardKey.D)) moveDirection.X += 1;

                if (moveDirection != Vector2.Zero)
                {
                    // Normalize to prevent faster diagonal movement
                    moveDirection = Vector2.Normalize(moveDirection);
                    _eventManager.Publish(new PlayerMoveInputEvent(moveDirection));
                }
            }
        }
    }
}