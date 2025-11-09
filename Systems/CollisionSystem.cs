namespace StellarSurvivors.Systems;
using System.IO.Enumeration;
using Raylib_cs; // For Vector2
using System;
using System.Numerics;
using System.Collections.Generic;
using StellarSurvivors.Core;

public class CollisionSystem
{
    private EntityManager _entityManager;
    private EventManager _eventManager;
    private WorldData _mapData;
    private int[,] _tileMap;
    private const int TILE_SIZE = 16;

    public CollisionSystem(EntityManager em, EventManager ev, WorldData map)
    {
        _entityManager = em;
        _eventManager = ev;
        _mapData = map;
        _tileMap = map.TileMap;
    }

    // You will need to refactor your main loop to call UpdateX and UpdateY
    // instead of the old Update() method.
    
    // You can keep your Entity-vs-Entity check here if you want.
    public void UpdateEntityCollisions()
    {
        // ... (Your original entity-vs-entity AABB or Circle checks)
    }

    /// <summary>
    /// Call this AFTER you move the entity on its X-axis.
    /// </summary>
    public void UpdateX(float deltaTime)
    {
        foreach (int entityId in _entityManager.Colliders.Keys)
        {
            if (_entityManager.Pods.ContainsKey(entityId) || _entityManager.Spacemen.ContainsKey(entityId))
            {
                CheckWorldCollisionX(entityId);
            }
        }
    }

    /// <summary>
    /// Call this AFTER you move the entity on its Y-axis.
    /// </summary>
    public void UpdateY(float deltaTime)
    {
        foreach (int entityId in _entityManager.Colliders.Keys)
        {
            if (_entityManager.Pods.ContainsKey(entityId) || _entityManager.Spacemen.ContainsKey(entityId))
            {
                CheckWorldCollisionY(entityId);
            }
        }
    }

    // --- NEW X-AXIS RESOLUTION ---
    private void CheckWorldCollisionX(int entityId)
    {
        var transform = _entityManager.Transforms[entityId];
        var collider = _entityManager.Colliders[entityId];

        Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y) ;
        float radius = collider.Radius;

        // Get the entity's AABB for broadphase
        int minTileX = (int)((pos.X - radius) / TILE_SIZE);
        int maxTileX = (int)((pos.X + radius) / TILE_SIZE);
        int minTileY = (int)((pos.Y - _mapData.Yoffset - radius) / TILE_SIZE);
        int maxTileY = (int)((pos.Y - _mapData.Yoffset + radius) / TILE_SIZE);

        for (int y = minTileY; y <= maxTileY; y++)
        {
            for (int x = minTileX; x <= maxTileX; x++)
            {
                TileType tileType = _mapData.GetTileType(x, y);
                //if (!IsTileSolid(tileType)) continue;
                if (!_mapData.GetTileDef(tileType).IsSolid) continue;

                // Get tile AABB
                float tileMinX = x * TILE_SIZE;
                float tileMaxX = tileMinX + TILE_SIZE;
                float tileMinY = (y * TILE_SIZE) + _mapData.Yoffset;
                float tileMaxY = tileMinY + TILE_SIZE;

                // Check for overlap (Circle vs AABB)
                float closestX = Math.Clamp(pos.X, tileMinX, tileMaxX);
                float closestY = Math.Clamp(pos.Y, tileMinY, tileMaxY);
                Vector2 closestPoint = new Vector2(closestX, closestY);
                float distanceSq = Vector2.DistanceSquared(pos, closestPoint);

                if (distanceSq < radius * radius)
                {
                    // COLLISION! Now, resolve *only* on the X-axis.
                    
                    // Find X penetration
                    float overlapLeft = (pos.X + radius) - tileMinX;
                    float overlapRight = tileMaxX - (pos.X - radius);

                    // We are only interested in the *smallest* overlap
                    if (overlapLeft < 0 || overlapRight < 0) continue; // Should be impossible if distance check passed, but good sanity check

                    float penetration = Math.Min(overlapLeft, overlapRight);
                    
                    // If we are overlapping, push out along the shallowest axis
                    if (penetration == overlapLeft)
                    {
                        // Pushed to the left
                        transform.Position.X -= penetration;
                    }
                    else
                    {
                        // Pushed to the right
                        transform.Position.X += penetration;
                    }

                    // Stop X-axis velocity
                    if (_entityManager.Velocities.TryGetValue(entityId, out var velocity))
                    {
                        velocity.Velocity.X = 0;
                        _entityManager.Velocities[entityId] = velocity;
                    }
                    
                    _entityManager.Transforms[entityId] = transform;
                    PublishWorldCollision(entityId, x, y, tileType);
                }
            }
        }
    }
    
    // --- NEW Y-AXIS RESOLUTION ---
    private void CheckWorldCollisionY(int entityId)
    {
        var transform = _entityManager.Transforms[entityId];
        var collider = _entityManager.Colliders[entityId];

        Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y);
        float radius = collider.Radius;

        int minTileX = (int)((pos.X - radius) / TILE_SIZE);
        int maxTileX = (int)((pos.X + radius) / TILE_SIZE);
        int minTileY = (int)((pos.Y - _mapData.Yoffset - radius) / TILE_SIZE);
        int maxTileY = (int)((pos.Y - _mapData.Yoffset + radius) / TILE_SIZE);

        for (int y = minTileY; y <= maxTileY; y++)
        {
            for (int x = minTileX; x <= maxTileX; x++)
            {
                TileType tileType = _mapData.GetTileType(x, y);
                //if (!IsTileSolid(tileType)) continue;
                if (!_mapData.GetTileDef(tileType).IsSolid) continue;

                float tileMinX = x * TILE_SIZE;
                float tileMaxX = tileMinX + TILE_SIZE;
                float tileMinY = (y * TILE_SIZE) + _mapData.Yoffset;
                float tileMaxY = tileMinY + TILE_SIZE;

                float closestX = Math.Clamp(pos.X, tileMinX, tileMaxX);
                float closestY = Math.Clamp(pos.Y, tileMinY, tileMaxY);
                Vector2 closestPoint = new Vector2(closestX, closestY);
                float distanceSq = Vector2.DistanceSquared(pos, closestPoint);

                if (distanceSq < radius * radius)
                {
                    // COLLISION! Resolve *only* on the Y-axis.
                    
                    float overlapTop = (pos.Y + radius) - tileMinY;
                    float overlapBottom = tileMaxY - (pos.Y - radius);

                    if (overlapTop < 0 || overlapBottom < 0) continue;

                    float penetration = Math.Min(overlapTop, overlapBottom);
                    
                    if (penetration == overlapTop)
                    {
                        // Pushed up
                        transform.Position.Y -= penetration;
                    }
                    else
                    {
                        // Pushed down
                        transform.Position.Y += penetration;
                    }

                    if (_entityManager.Velocities.TryGetValue(entityId, out var velocity))
                    {
                        velocity.Velocity.Y = 0;
                        _entityManager.Velocities[entityId] = velocity;
                    }
                    
                    _entityManager.Transforms[entityId] = transform;
                    PublishWorldCollision(entityId, x, y, tileType);
                }
            }
        }
    }

    private void PublishWorldCollision(int entityId, int x, int y, TileType tileType)
    {
        _eventManager.Publish(new WorldCollisionEvent
        {
            EntityId = entityId,
            TileX = x,
            TileY = y,
            TileType = tileType
        });
    }
    
    private bool IsTileSolid(TileType type)
    {
        // ... (your method is perfectly fine)
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