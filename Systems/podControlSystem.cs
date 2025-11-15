namespace StellarSurvivors.Systems;
using System.Numerics;
using StellarSurvivors.Core;
using Raylib_cs;

public class PodControlSystem : IUpdateSystem
{
    private EntityManager _entityManager;
    private EventManager _eventManager;
    private Boolean _isThrusting = false;
    private float _rotationInput;
    
    public PodControlSystem(EventManager eventManager, EntityManager entityManager)
    {
        _entityManager = entityManager;
        _eventManager  = eventManager;
        // Listen for Events
        eventManager.Subscribe<MoveInputEvent>(OnRotate);
        eventManager.Subscribe<ThrustInputEvent>(OnThrust); 
    }

    // Event Handlers
    private void OnRotate(MoveInputEvent e)
    {
        _rotationInput = e.Direction;
    }
    
    private void OnThrust(ThrustInputEvent e)
    {
        _isThrusting = true;
    }
    
    public void Update(Game world, float deltaTime)
    {
        int controlledEntityId = _entityManager.ControlledEntityId;
        if (controlledEntityId != _entityManager.PodId) return;

        // Make sure it's a pod
        if (!_entityManager.Pods.ContainsKey(controlledEntityId))
        {
            _rotationInput = 0;
            _isThrusting = false; // Reset flag if not a pod
            return;
        }

        // Get components (we need Velocity and Energy now, too)
        var pod = _entityManager.Pods[controlledEntityId];
        var transform = _entityManager.Transforms[controlledEntityId];
        var velocity = _entityManager.Velocities[controlledEntityId]; // Assuming you have this
        var render = _entityManager.Renderables[controlledEntityId]; 
        
        // Apply Rotation Logic
        if (_rotationInput != 0)
        {
            transform.Rotation += _rotationInput * pod.rotationSpeed * deltaTime;
        }

        var fuel = _entityManager.Fuels[controlledEntityId];
        // --- APPLY THRUST LOGIC ---
        if (_isThrusting && fuel.CurrentFuel > 0)
        {
            // 1. Calculate the forward direction vector from rotation
            //    (Assumes 0 radians is "up")
            Vector2 forwardDirection = new Vector2(
                MathF.Sin(transform.Rotation), 
                -MathF.Cos(transform.Rotation)
            );
            float currentSpeed = velocity.Velocity.Length();
            // 2. Apply thrust to velocity
            if (currentSpeed <= velocity.MaxVelocity)
            {
                velocity.Velocity += forwardDirection * pod.thrustForce * deltaTime;
            }
            
            // 3. Drain energy (if it has an energy component)
            if (_entityManager.Fuels.ContainsKey(controlledEntityId))
            {
                fuel.CurrentFuel -= pod.thrustEnergyDrain * deltaTime;
                fuel.CurrentFuel = Math.Max(0, fuel.CurrentFuel); // Don't go below 0
            }
            
        }
        
        int controlledId = _entityManager.ControlledEntityId;
        if (controlledId != _entityManager.PodId) return; // Not controlling pod

        // --- ADD THIS SPAWN LOGIC ---
        if (Raylib.IsKeyPressed(KeyboardKey.G))
        {
            // 1. Check if spaceman already exists
            if (_entityManager.SpacemanId != -1)
            {
                // TODO: Play "already deployed" sound
                return;
            }
    
            // 2. Check fuel cost
            if (_entityManager.Fuels.TryGetValue(controlledId, out var podFuel))
            {
                const float SPAWN_COST = 5.0f; // Spaceman costs 5 fuel
                if (podFuel.CurrentFuel >= SPAWN_COST)
                {
                    // 3. Deduct fuel
                    podFuel.CurrentFuel -= SPAWN_COST;
                    _entityManager.Fuels[controlledId] = podFuel;
            
                    // 4. Publish event to create the spaceman
                    _eventManager.Publish(new ExitPodEvent() { PodId = controlledId });
                }
                else
                {
                    // TODO: Play "no fuel" sound
                }
            }
            
            int mothershipId = _entityManager.MothershipId;
            if (mothershipId != -1)
            {
                // Check if we are in range
                var podPos = _entityManager.Transforms[controlledId].Position;
                var shipPos = _entityManager.Transforms[mothershipId].Position;
    
                // (This range must match the one in UIManager and MothershipInteractionSystem)
                if (Vector3.DistanceSquared(podPos, shipPos) < (100 * 100)) 
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Q))
                    {
                        _eventManager.Publish(new RefineFuelEvent());
                    }
        
                    if (Raylib.IsKeyPressed(KeyboardKey.R))
                    {
                        _eventManager.Publish(new RechargePodEvent());
                    }
                }
            }
        }
        // --- END THRUST LOGIC ---
        
        // Save modified components (structs)
        _entityManager.Transforms[controlledEntityId] = transform;
        _entityManager.Velocities[controlledEntityId] = velocity; // Write back velocity
        _entityManager.Fuels[controlledEntityId] = fuel;

        // Reset flags for next frame
        _rotationInput = 0;
        _isThrusting = false;
    }
}