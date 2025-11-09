namespace StellarSurvivors.Systems;
using System.Numerics;
using StellarSurvivors.Core;
using Raylib_cs;

public class PodControlSystem : IUpdateSystem
{
    private EntityManager _entityManager;
    private Boolean _isThrusting = false;
    private float _rotationInput;
    
    public PodControlSystem(EventManager eventManager, EntityManager entityManager)
    {
        _entityManager = entityManager;
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
        // Find the controlled entity
        if (_entityManager.PlayerInputs.Count == 0) return;
        int controlledEntityId = _entityManager.PlayerInputs.Keys.First();

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

            // 2. Apply thrust to velocity
            velocity.Velocity += forwardDirection * pod.thrustForce * deltaTime;
            
            // 3. Drain energy (if it has an energy component)
            if (_entityManager.Fuels.ContainsKey(controlledEntityId))
            {
                
                fuel.CurrentFuel -= pod.thrustEnergyDrain * deltaTime;
                fuel.CurrentFuel = Math.Max(0, fuel.CurrentFuel); // Don't go below 0
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