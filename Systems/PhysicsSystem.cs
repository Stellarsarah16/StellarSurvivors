namespace StellarSurvivors.Systems;
using StellarSurvivors.Core;

public class PhysicsSystem
{
    private EntityManager _entityManager;
    private WorldData _worldData;
    private static float _gravity = 200f;
    private const int TILE_SIZE = 16;

    public PhysicsSystem(EntityManager entityManager, WorldData worldData)
    {
        _entityManager = entityManager;
        _worldData = worldData;
    }
    
    public void Update(float deltaTime)
{
    foreach (int entityId in _entityManager.Velocities.Keys)
    {
        if (!_entityManager.Transforms.ContainsKey(entityId)) continue;
        
        var velocity = _entityManager.Velocities[entityId];
        var transform = _entityManager.Transforms[entityId];

        // --- 1. APPLY GRAVITY ---
        if (_entityManager.Pods.ContainsKey(entityId))
        {
            velocity.Velocity.Y += (_gravity / 8) * deltaTime;
        }
        else
        {
            velocity.Velocity.Y += _gravity * deltaTime;
        }

        // --- 2. APPLY FRICTION/DAMPING ---
        
        // This is the float-to-int conversion you asked for
        int tileX = (int)(transform.Position.X / TILE_SIZE);
        
        // Don't forget your Y-offset!
        // We check the tile at the entity's center...
        int tileY = (int)((transform.Position.Y - _worldData.Yoffset) / TILE_SIZE);
        
        // ...to get the tile definition for the ground *below* it.
        TileType groundTileType = _worldData.GetTileType(tileX, tileY + 1);
        TileDefinition groundDef = _worldData.GetTileDef(groundTileType);

        float damping;
        if (groundDef.IsSolid)
        {
            // We're on solid ground. Use friction.
            // Let's say friction 1.0 = 90% slow down per sec
            // friction 0.1 (ice) = 9% slow down
            // friction 2.0 (mud) = 180% slow down
            damping = 1.0f - (0.9f * groundDef.Friction * deltaTime);
        }
        else
        {
            // We're in the air. Use basic air resistance.
            damping = 1.0f - (0.1f * deltaTime); // Much less friction
        }

        // Clamp damping to prevent velocity from reversing
        if (damping < 0) damping = 0;

        velocity.Velocity.X *= damping;
        
        // Optional: Apply air resistance to Y velocity too
        // velocity.Velocity.Y *= (1.0f - (0.1f * deltaTime));
        

        _entityManager.Velocities[entityId] = velocity;
    }
}
}