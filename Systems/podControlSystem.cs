namespace StellarSurvivors.Systems;
using System.Numerics;
using StellarSurvivors.Core;
using Raylib_cs;

public class PodControlSystem : IUpdateSystem
{
    private EntityManager _entityManager;
    private Boolean _isThrusting = false;
    private float _rotationInput;

    // Constructor: Subscribe to events
    public PodControlSystem(EventManager eventManager, EntityManager entityManager)
    {
        _entityManager = entityManager;
        // Listen for RotationInputEvent
        eventManager.Subscribe<RotationInputEvent>(OnRotate);
        // Listen for ThrustInputEvent
        eventManager.Subscribe<ThrustInputEvent>(OnThrust); 
    }

    // Event Handler
    private void OnRotate(RotationInputEvent e)
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
        
        // Apply Rotation Logic
        if (_rotationInput != 0)
        {
            transform.Rotation += _rotationInput * pod.rotationSpeed * deltaTime;
            System.Console.WriteLine(transform.Rotation);
        }

        // --- APPLY THRUST LOGIC ---
        if (_isThrusting)
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
            if (_entityManager.Energies.ContainsKey(controlledEntityId))
            {
                var energy = _entityManager.Energies[controlledEntityId];
                energy.Current -= pod.thrustEnergyDrain * deltaTime;
                energy.Current = Math.Max(0, energy.Current); // Don't go below 0
                _entityManager.Energies[controlledEntityId] = energy; // Write back
            }
            
        }
        // --- END THRUST LOGIC ---
        
        // Save modified components (structs)
        _entityManager.Transforms[controlledEntityId] = transform;
        _entityManager.Velocities[controlledEntityId] = velocity; // Write back velocity

        // Reset flags for next frame
        _rotationInput = 0;
        _isThrusting = false;
    }
}