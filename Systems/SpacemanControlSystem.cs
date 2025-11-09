namespace StellarSurvivors.Systems;
using System.Numerics;
using StellarSurvivors.Core; 

public class SpacemanControlSystem : IUpdateSystem
{
    private EntityManager _entityManager;

    private float _horizontalInput = 0f;
    private bool _isThrusting = false;

    // 1. Constructor: Subscribe to the event
    public SpacemanControlSystem(EventManager eventManager, EntityManager entityManager)
    {
        _entityManager = entityManager;

        // We subscribe to the *same* event as the pod!
        eventManager.Subscribe<MoveInputEvent>(OnMove);
        eventManager.Subscribe<ThrustInputEvent>(OnThrust);
    }

    // 2. Event Handler: This just saves the input
    private void OnMove(MoveInputEvent e)
    {
        // We ONLY care about the X component. We ignore Y.
        _horizontalInput = e.Direction;
    }

    private void OnThrust(ThrustInputEvent e)
    {
        _isThrusting = true;
    }

    // 3. Update Method: This applies all physics
    public void Update(Game world, float deltaTime)
    {
        // Find the controlled entity
        if (world.EntityManager.Players.Count == 0) return;
        int playerId = world.EntityManager.Players.First();

        // Check if it's a spaceman
        if (world.EntityManager.Spacemen.ContainsKey(playerId))
        {
            // Get the spaceman's components
            var spaceman = world.EntityManager.Spacemen[playerId];
            var velocity = world.EntityManager.Velocities[playerId];
            var fuel = world.EntityManager.Fuels[playerId];

            // --- Apply Logic ---

            if (_isThrusting &&  fuel.CurrentFuel > 0)
            {
                velocity.Velocity.Y -= spaceman.Thrust * deltaTime;
                fuel.CurrentFuel -= spaceman.ThrustEnergyDrain * deltaTime;
                fuel.CurrentFuel = Math.Max(0, fuel.CurrentFuel); // Don't go below 0
                world.EntityManager.Fuels[playerId] = fuel;
            }


            // B) Apply the horizontal control (if there was any)
            if (_horizontalInput != 0)
            {
                velocity.Velocity.X += _horizontalInput * spaceman.HorizontalSpeed * deltaTime;
            }

            // --- Write changes back ---
            world.EntityManager.Velocities[playerId] = velocity;
            world.EntityManager.Fuels[playerId] = fuel;
            


            // --- Reset the flag for the next frame ---
            _horizontalInput = 0f;
            _isThrusting = false;
        }
    }
}

  
