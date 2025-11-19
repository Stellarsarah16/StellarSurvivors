using System.Runtime.InteropServices;
using Raylib_cs;

namespace StellarSurvivors.Systems;
using System.Numerics;
using StellarSurvivors.Core; 
using StellarSurvivors.Components;

public class SpacemanControlSystem : IUpdateSystem
{
    private EntityManager _entityManager;
    private EventManager _eventManager;
    private Game _world; 

    private float _horizontalInput = 0f;
    private bool _thrustHeld = false;
    private bool _jumpPressed = false;


    // 1. Constructor: Subscribe to the event
    public SpacemanControlSystem(Game world)
    {   
        _world  = world;
        _eventManager  = world.EventManager;
        _entityManager = world.EntityManager;

        // We subscribe to the *same* event as the pod!
        _eventManager.Subscribe<MoveInputEvent>(OnMove);
        _eventManager.Subscribe<ThrustInputEvent>(OnThrust);
        _eventManager.Subscribe<JumpInputEvent>(OnJump);
        _eventManager.Subscribe<LeftMouseDownEvent>(OnToolUse);
        _eventManager.Subscribe<LeftMouseReleasedEvent>(OnToolStop);
        _eventManager.Subscribe<EquipToolEvent>(OnEquipTool);
    }

    // 2. Event Handler: This just saves the input
    private void OnMove(MoveInputEvent e) { _horizontalInput = e.Direction; }

    private void OnThrust(ThrustInputEvent e) { _thrustHeld = true; }
    
    private void OnJump(JumpInputEvent e) { _jumpPressed = true; }
    
    // This is called EVERY FRAME the mouse is held down
    private void OnToolUse(LeftMouseDownEvent e)
    {
        if (_entityManager.ControlledEntityId == -1) return;
        int spacemanId = _entityManager.ControlledEntityId;

        // Check if our controlled entity has a tool
        if (_entityManager.Tools.TryGetValue(spacemanId, out var toolComp))
        {
            if (toolComp.CurrentTool == null) return;
            
            var tool = toolComp.CurrentTool;

            // --- NEW FUEL LOGIC ---
    
            // 3. Get the entity's fuel component
            if (!_entityManager.Fuels.TryGetValue(spacemanId, out var fuelComp))
            {
                return; // This spaceman has no fuel tank
            }

            // 4. Determine the cost for this frame
            float cost = 0;
            if (e.JustPressed)
            {
                if (tool.FuelCostPerClick > 0)
                    // This is a "per-click" tool
                    // cost = tool.FuelCostPerClick;
                    cost = 0;
            }
            else if (!e.JustPressed && tool.FuelCostPerSecond > 0)
            {
                // This is a "per-second" (hold) tool
                cost = tool.FuelCostPerSecond * _world.DeltaTime;
            }

            // 5. Check if we have enough fuel
            if (fuelComp.CurrentFuel < cost)
            {
                // Out of fuel!
                // TODO: Play an "empty click" sound
                return; // Stop here, do not use the tool
            }
    
            // 6. We have fuel! Deduct the cost.
            fuelComp.CurrentFuel -= cost;
            _entityManager.Fuels[spacemanId] = fuelComp;
            
            Camera2D currentCamera = _world.CameraSystem.Camera;
            Vector2 worldPos = Raylib.GetScreenToWorld2D(e.MousePosition, _world.CameraSystem.Camera);
            toolComp.CurrentTool.Use(
                spacemanId, 
                worldPos,
                _world.DeltaTime, // Get the most current deltaTime
                _world,
                e.JustPressed
            );
        }
    }

    // This is called ONCE when the mouse is released
    private void OnToolStop(LeftMouseReleasedEvent e)
    {
        if (_entityManager.ControlledEntityId == -1) return;
        int _spacemanId = _entityManager.ControlledEntityId;
        
        // Check if our controlled entity has a tool
        if (_entityManager.Tools.TryGetValue(_spacemanId, out var toolComp))
        {
            if (toolComp.CurrentTool != null)
            {
                toolComp.CurrentTool.StopUse(_spacemanId, _world);
            }
        }
    }
    
    private void OnEquipTool(EquipToolEvent e)
    {
        // Check if this event is for the entity we control
        if (e.EntityId == _entityManager.ControlledEntityId &&
            _entityManager.Tools.TryGetValue(e.EntityId, out var toolComp))
        {
            toolComp.EquipTool(e.ToolName);
        }
    }
    

    // 3. Update Method: This applies all physics
    public void Update(Game world, float deltaTime)
    {
        if (world.EntityManager.ControlledEntityId == -1) return;
        int playerId = world.EntityManager.ControlledEntityId;

        // Check if it's a spaceman
        if (world.EntityManager.Spacemen.ContainsKey(playerId))
        {
            // Get the spaceman's components
            var spaceman = world.EntityManager.Spacemen[playerId];
            var velocity = world.EntityManager.Velocities[playerId];
            var fuel = world.EntityManager.Fuels[playerId];
            
            if (!world.EntityManager.PlayerInputs.TryGetValue(playerId, out var input) ||
                !world.EntityManager.Animations.TryGetValue(playerId, out var anim))
            {
                // Not a controllable, animated Spaceman
                return;
            }
            
            // --- 2. HORIZONTAL PHYSICS ---
            // This logic is now DE-COUPLED and runs EVERY frame.
            if (Math.Abs(_horizontalInput) > 0.1f)
            {
                float airControlFactor = spaceman.IsOnGround ? 1.0f : 0.8f; // Example: 80% control in air
                velocity.Velocity.X += _horizontalInput * spaceman.HorizontalSpeed * airControlFactor * deltaTime;

                // This is also a good place to set facing direction
                input.FacingDirection = (_horizontalInput > 0.1f) ? 1.0f : -1.0f;
            }
            else
            {
                // Apply horizontal friction/damping if on ground
                if (spaceman.IsOnGround)
                {
                    // This is a simple friction model
                    velocity.Velocity.X *= 1.0f - (spaceman.Friction * deltaTime); 
                }
            }
            
            // --- 3. VERTICAL PHYSICS ---
            // This is your jump/thrust logic, now separate.
            if (_jumpPressed)
            {
                if (spaceman.IsOnGround)
                {
                    velocity.Velocity.Y = -spaceman.JumpForce;
                }
            }
            else if (_thrustHeld && !spaceman.IsOnGround && fuel.CurrentFuel > 0)
            {
                velocity.Velocity.Y -= spaceman.Thrust * deltaTime;
                fuel.CurrentFuel -= spaceman.ThrustEnergyDrain * deltaTime;
            }
            
            
            // Animate
            // Priority 1: Is the player thrusting?
            if (_thrustHeld && !spaceman.IsOnGround && fuel.CurrentFuel > 0)
            {
                anim.Play("spaceman_thrust");
            }
            // Priority 2: If not, is the player falling?
            else if (!spaceman.IsOnGround)
            {
                anim.Play("spaceman_fall");
            }
            // Priority 3: If not, is the player walking? (on ground + moving)
            else if (spaceman.IsOnGround && Math.Abs(_horizontalInput) > 0.1f)
            {
                anim.Play("spaceman_walk");
            }
            // Priority 4: If not... the player must be idle.
            else
            {
                anim.Play("spaceman_idle");
            }
            
            
            // --- Write changes back ---
            world.EntityManager.Velocities[playerId] = velocity;
            world.EntityManager.Fuels[playerId] = fuel;
            world.EntityManager.Spacemen[playerId] = spaceman;
            
            // --- Reset the flag for the next frame ---
            _horizontalInput = 0f;
            _thrustHeld = false;
            _jumpPressed = false;
            
            // Update Tools
            if (!_entityManager.Tools.TryGetValue(playerId, out var toolComp)) return;
            
            // --- 3. NEW: Handle Tool Selection Input ---
            if (Raylib.IsKeyPressed(KeyboardKey.One))
            {
                toolComp.EquipTool("Drill");
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Two))
            {
                // This assumes "MoveTileTool" is in your Toolset
                toolComp.EquipTool("MoveTile"); 
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Three))
            {
               toolComp.EquipTool("Miner");
            }
            
            // --- 2. NEW: Handle Tool Selection Input (Scroll Wheel) ---
            float scroll = Raylib.GetMouseWheelMove(); // Returns 1.0f (up), -1.0f (down), or 0
            if (scroll > 0)
            {
                toolComp.CycleToolNext();
            }
            else if (scroll < 0)
            {
                toolComp.CycleToolPrevious();
            }
        
            if (toolComp.CurrentTool != null)
            {
                Vector2 mouseScreen = Raylib.GetMousePosition();
                Vector2 mousePos = Raylib.GetScreenToWorld2D(mouseScreen, _world.CameraSystem.Camera);
                
                toolComp.CurrentTool.Update(playerId, mousePos, _world);
            }
        
        }
    }
}

  
