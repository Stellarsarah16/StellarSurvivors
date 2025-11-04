using System.IO.Enumeration;

namespace StellarSurvivors.Systems;
using Raylib_cs; // For Vector2
using System;
using System.Numerics;
using System.Collections.Generic;
using StellarSurvivors.Core;

public class CollisionSystem : IUpdateSystem
{
    private EntityManager _entityManager;
    private EventManager _eventManager;
    private WorldData _mapData; // We need the map for world collisions
    private int[,] _tileMap;

    // We need to know the tile size. You might store this elsewhere.
    private const int TILE_SIZE = 16; 

    public CollisionSystem(EntityManager em, EventManager ev, WorldData map)
    {
        _entityManager = em;
        _eventManager = ev;
        _mapData = map;
        _tileMap = map.TileMap;
    }

    public void Update(Game world,  float deltaTime)
    {
        // Get all collidable entities
        var collidableEntities = new List<int>(_entityManager.Colliders.Keys);
        
        // --- 1. Entity-vs-Entity (N^2 check) ---
        // This is fine for a few entities, but slow for hundreds
        for (int i = 0; i < collidableEntities.Count; i++)
        {
            int entityAId = collidableEntities[i];

            for (int j = i + 1; j < collidableEntities.Count; j++)
            {
                int entityBId = collidableEntities[j];

                // Run the check
                if (CheckEntityCollision(entityAId, entityBId))
                {
                    // Publish an event so other systems can react
                    _eventManager.Publish(new CollisionEvent 
                    { 
                        EntityA = entityAId, 
                        EntityB = entityBId 
                    });
                }
            }
        }

        // --- 2. Entity-vs-World ---
        // We only check entities that can move
        foreach (int entityId in collidableEntities)
        {
            if (_entityManager.Pods.ContainsKey(entityId) || _entityManager.Spacemen.ContainsKey(entityId))
            {
                CheckWorldCollision(entityId);
            }
        }
    }

    /// <summary>
    /// Checks for Circle-vs-Circle collision between two entities.
    /// </summary>
    private bool CheckEntityCollision(int entityAId, int entityBId)
    {
        // Make sure both entities still exist
        if (!_entityManager.Transforms.ContainsKey(entityAId) || 
            !_entityManager.Transforms.ContainsKey(entityBId))
        {
            return false;
        }

        var transformA = _entityManager.Transforms[entityAId];
        var colliderA = _entityManager.Colliders[entityAId];
        
        var transformB = _entityManager.Transforms[entityBId];
        var colliderB = _entityManager.Colliders[entityBId];

        // We only care about 2D position
        Vector2 posA = new Vector2(transformA.Position.X, transformA.Position.Y);
        Vector2 posB = new Vector2(transformB.Position.X, transformB.Position.Y);

        // 1. Get the distance between centers
        float distance = Vector2.Distance(posA, posB);

        // 2. Get the sum of their radii
        float sumOfRadii = colliderA.Radius + colliderB.Radius;

        // 3. If distance is less than sum of radii, they overlap
        return distance < sumOfRadii;
    }

    /// <summary>
    /// Checks for Circle-vs-Solid-Tile collision and resolves it.
    /// </summary>
    
    private void CheckWorldCollision(int entityId)
{
    var transform = _entityManager.Transforms[entityId];
    var collider = _entityManager.Colliders[entityId];
    
    Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y);
    float radius = collider.Radius;
    
    // 1. Find tile coordinates (This part is now correct)
    int minTileX = (int)((pos.X - radius) / TILE_SIZE);
    int maxTileX = (int)((pos.X + radius) / TILE_SIZE);
    int minTileY = (int)((pos.Y - _mapData.Yoffset - radius) / TILE_SIZE);
    int maxTileY = (int)((pos.Y - _mapData.Yoffset + radius) / TILE_SIZE);

    for (int y = minTileY; y <= maxTileY; y++)
    {
        for (int x = minTileX; x <= maxTileX; x++)
        {
            TileType tileType = _mapData.GetTileType(x, y);

            if (IsTileSolid(tileType))
            {
                
                // 4. --- Narrowphase: Circle-vs-Square ---
                // Find the tile's AABB in the *correct* coordinate space
                float tileMinX = x * TILE_SIZE;
                float tileMaxX = tileMinX + TILE_SIZE;
                
                // --- FIX #1: Apply the yOffset to the tile's world position ---
                float tileMinY = (y * TILE_SIZE) + _mapData.Yoffset; 
                float tileMaxY = tileMinY + TILE_SIZE;

                // 5. Find the closest point
                float closestX = Math.Clamp(pos.X, tileMinX, tileMaxX);
                float closestY = Math.Clamp(pos.Y, tileMinY, tileMaxY);
                Vector2 closestPoint = new Vector2(closestX, closestY);

                // 6. Check distance
                float distance = Vector2.Distance(pos, closestPoint);


                if (distance < radius)
                {
                    // --- COLLISION! ---
                    
                    // 1. Calculate pushback (applies to both pod and spaceman)
                    float penetration = radius - distance;
                    Vector2 normal = Vector2.Normalize((pos - closestPoint) + new Vector2(0.001f)); 
                    
                    transform.Position.X += normal.X * penetration;
                    transform.Position.Y += normal.Y * penetration;

                    // --- FIX #2: Only modify velocity if the entity HAS it ---
                    if (_entityManager.Velocities.ContainsKey(entityId))
                    {
                        var velocity = _entityManager.Velocities[entityId];

                        // Project the velocity onto the collision normal (the pushback direction)
                        float dot = Vector2.Dot(velocity.Velocity, normal);

                        // We only want to remove velocity that's heading *into* the wall (dot < 0)
                        if (dot < 0)
                        {
                            // Remove the velocity component that is penetrating the wall
                            velocity.Velocity -= normal * dot;
                        }

                        _entityManager.Velocities[entityId] = velocity;
                    }

                    // Write the transform back (applies to both)
                    _entityManager.Transforms[entityId] = transform;

                    // Publish an event
                    _eventManager.Publish(new WorldCollisionEvent 
                    { 
                        EntityId = entityId, 
                        TileX = x, 
                        TileY = y,
                        TileType = tileType
                    });

                }
            }
        }
    }
}
    
    /// <summary>
    /// Helper method to define what's solid.
    /// </summary>
    private bool IsTileSolid(TileType type)
    {
        switch (type)
        {
            case TileType.Stone:
            case TileType.Iron:
            case TileType.Gold:
            case TileType.Dirt:
                return true;
            
            case TileType.None:
            case TileType.Water:
            case TileType.Grass:
            default:
                return false;
        }
    }
}