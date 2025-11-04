namespace StellarSurvivors.Systems;
using StellarSurvivors.Core;

public class PhysicsSystem: IUpdateSystem
{
    private EntityManager _entityManager;
    private static float _gravity = 9.81f;

    public PhysicsSystem(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    public void Update(Game world, float deltaTime)
    {
        // Loop over every entity that has BOTH velocity and transform
        foreach (int entityId in _entityManager.Velocities.Keys)
        {
            if (_entityManager.Transforms.ContainsKey(entityId))
            {
                var velocity = _entityManager.Velocities[entityId];
                var transform = _entityManager.Transforms[entityId];

                // --- THIS IS THE CRITICAL LINE ---
                transform.Position.X += velocity.Velocity.X * deltaTime;
                transform.Position.Y += velocity.Velocity.Y * deltaTime;
                
                // Optional: Apply damping/friction
                velocity.Velocity *= 0.995f; 
                transform.Position.Y += _gravity * deltaTime;

                // Write the changes back
                _entityManager.Transforms[entityId] = transform;
                _entityManager.Velocities[entityId] = velocity;
            }
        }
    }
}