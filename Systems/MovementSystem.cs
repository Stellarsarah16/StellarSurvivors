namespace StellarSurvivors.Systems;
using Raylib_cs;
using System.Numerics;
using System.Linq; // We'll use this to find the player
using StellarSurvivors.Core;


// Runs AFTER PhysicsSystem in your game loop

public class MovementSystem
{
    private EntityManager _entityManager;

    public MovementSystem(EntityManager em)
    {
        _entityManager = em;
    }

    public void UpdateX(float deltaTime)
    {
        foreach (var entityId in _entityManager.Velocities.Keys)
        {
            if (!_entityManager.Transforms.ContainsKey(entityId)) continue; 
            
            var velocity = _entityManager.Velocities[entityId];
            var transform = _entityManager.Transforms[entityId];
            
            // This system's ONLY job is to apply X velocity
            transform.Position.X += velocity.Velocity.X * deltaTime;

            _entityManager.Transforms[entityId] = transform;
        }
    }

    public void UpdateY(float deltaTime)
    {
        foreach (var entityId in _entityManager.Velocities.Keys)
        {
            if (!_entityManager.Transforms.ContainsKey(entityId)) continue;
            
            var velocity = _entityManager.Velocities[entityId];
            var transform = _entityManager.Transforms[entityId];
            
            // This system's ONLY job is to apply Y velocity
            transform.Position.Y += velocity.Velocity.Y * deltaTime;

            _entityManager.Transforms[entityId] = transform;
        }
    }
}