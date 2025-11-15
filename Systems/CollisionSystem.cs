namespace StellarSurvivors.Systems;
using System.IO.Enumeration;
using Raylib_cs; // For Vector2
using System;
using System.Numerics;
using System.Collections.Generic;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;

public class CollisionSystem
{
    private EntityManager _entityManager;
    private EventManager _eventManager;
    private WorldData _mapData;
    private int[,] _tileMap;
    private const int TILE_SIZE = GameConstants.TILE_SIZE;

    public CollisionSystem(EntityManager em, EventManager ev, WorldData map)
    {
        _entityManager = em;
        _eventManager = ev;
        _mapData = map;
        _tileMap = map.TileMap;
    }
    
    public void UpdateX(float deltaTime)
    {
        foreach (int entityId in _entityManager.Colliders.Keys)
        {
            CheckWorldCollisionX(entityId);
        }
    }

    public void UpdateY(float deltaTime)
    {
        foreach (var spacemanId in _entityManager.Spacemen.Keys.ToList())
        {
            var spaceman = _entityManager.Spacemen[spacemanId];
            spaceman.IsOnGround = false;
            _entityManager.Spacemen[spacemanId] = spaceman;
        }
        
        foreach (int entityId in _entityManager.Colliders.Keys)
        {
            CheckWorldCollisionY(entityId);
        }
    }

    // --- NEW X-AXIS RESOLUTION ---
   private void CheckWorldCollisionX(int entityId)
{
    var transform = _entityManager.Transforms[entityId];
    var collider = _entityManager.Colliders[entityId];
    Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y);
    float radius = collider.Radius;

    // --- NEW: Collision tracking variables ---
    bool collisionFound = false;
    float maxPenetration = 0;
    Vector2 finalNormal = Vector2.Zero;
    float finalPenetration = 0;
    
    // We still need to store the "winning" tile for the event
    int finalTileX = 0, finalTileY = 0; 
    TileType finalTileType = TileType.None;
    // --- END NEW ---

    int minTileX = (int)((pos.X - radius) / TILE_SIZE);
    int maxTileX = (int)((pos.X + radius) / TILE_SIZE);
    int minTileY = (int)((pos.Y - _mapData.Yoffset - radius) / TILE_SIZE);
    int maxTileY = (int)((pos.Y - _mapData.Yoffset + radius) / TILE_SIZE);

    for (int y = minTileY; y <= maxTileY; y++)
    {
        for (int x = minTileX; x <= maxTileX; x++)
        {
            TileType tileType = _mapData.GetTileType(x, y);
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
                // ... (Case 1 / Case 2 logic to find 'normal' and 'penetration' is UNCHANGED) ...
                Vector2 normal;
                float penetration;
                float distance = (float)Math.Sqrt(distanceSq);

                if (distance > 0.0001f)
                {
                    normal = Vector2.Normalize(pos - closestPoint);
                    penetration = radius - distance;
                }
                else
                {
                    float distLeft = pos.X - tileMinX;
                    float distRight = tileMaxX - pos.X;
                    float distTop = pos.Y - tileMinY;
                    float distBottom = tileMaxY - pos.Y;
                    float minOverlap = distLeft;
                    normal = new Vector2(-1, 0);
                    if (distRight < minOverlap) { minOverlap = distRight; normal = new Vector2(1, 0); }
                    if (distTop < minOverlap) { minOverlap = distTop; normal = new Vector2(0, -1); }
                    if (distBottom < minOverlap) { minOverlap = distBottom; normal = new Vector2(0, 1); }
                    penetration = minOverlap;
                }
                // ... (End of Case 1 / Case 2 logic) ...


                // --- NEW: Find the "worst" collision ---
                // We are in UpdateX, so we ONLY care if it's a horizontal collision
                if (Math.Abs(normal.X) > Math.Abs(normal.Y))
                {
                    collisionFound = true;
                    // Is this penetration worse than the last one we found?
                    if (penetration > maxPenetration)
                    {
                        maxPenetration = penetration;
                        finalNormal = normal;
                        finalPenetration = penetration;
                        finalTileX = x;
                        finalTileY = y;
                        finalTileType = tileType;
                    }
                }
            }
        }
    }

    // --- NEW: Apply a SINGLE resolution *after* all loops ---
    if (collisionFound)
    {
        transform.Position.X += finalNormal.X * finalPenetration;

        if (_entityManager.Velocities.TryGetValue(entityId, out var velocity))
        {
            velocity.Velocity.X = 0;
            _entityManager.Velocities[entityId] = velocity;
        }
        
        _entityManager.Transforms[entityId] = transform;
        PublishWorldCollision(entityId, finalTileX, finalTileY, finalTileType);
    }
}
    
    private void CheckWorldCollisionY(int entityId)
    {
        var transform = _entityManager.Transforms[entityId];
        var collider = _entityManager.Colliders[entityId];
        Vector2 pos = new Vector2(transform.Position.X, transform.Position.Y);
        float radius = collider.Radius;

        // --- NEW: Collision tracking variables ---
        bool collisionFound = false;
        float maxPenetration = 0;
        Vector2 finalNormal = Vector2.Zero;
        float finalPenetration = 0;
        
        int finalTileX = 0, finalTileY = 0;
        TileType finalTileType = TileType.None;
        // --- END NEW ---

        int minTileX = (int)((pos.X - radius) / TILE_SIZE);
        int maxTileX = (int)((pos.X + radius) / TILE_SIZE);
        int minTileY = (int)((pos.Y - _mapData.Yoffset - radius) / TILE_SIZE);
        int maxTileY = (int)((pos.Y - _mapData.Yoffset + radius) / TILE_SIZE);

        for (int y = minTileY; y <= maxTileY; y++)
        {
            for (int x = minTileX; x <= maxTileX; x++)
            {
                TileType tileType = _mapData.GetTileType(x, y);
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
                    // ... (Case 1 / Case 2 logic to find 'normal' and 'penetration' is UNCHANGED) ...
                    Vector2 normal;
                    float penetration;
                    float distance = (float)Math.Sqrt(distanceSq);

                    if (distance > 0.0001f)
                    {
                        normal = Vector2.Normalize(pos - closestPoint);
                        penetration = radius - distance;
                    }
                    else
                    {
                        float distLeft = pos.X - tileMinX;
                        float distRight = tileMaxX - pos.X;
                        float distTop = pos.Y - tileMinY;
                        float distBottom = tileMaxY - pos.Y;
                        float minOverlap = distLeft;
                        normal = new Vector2(-1, 0);
                        if (distRight < minOverlap) { minOverlap = distRight; normal = new Vector2(1, 0); }
                        if (distTop < minOverlap) { minOverlap = distTop; normal = new Vector2(0, -1); }
                        if (distBottom < minOverlap) { minOverlap = distBottom; normal = new Vector2(0, 1); }
                        penetration = minOverlap;
                    }
                    // ... (End of Case 1 / Case 2 logic) ...


                    // --- NEW: Find the "worst" collision ---
                    // We are in UpdateY, so we ONLY care if it's a vertical or diagonal collision
                    if (Math.Abs(normal.Y) >= Math.Abs(normal.X)) // <-- This gets the tie-break
                    {
                        collisionFound = true;
                        // Is this penetration worse than the last one we found?
                        if (penetration > maxPenetration)
                        {
                            maxPenetration = penetration;
                            finalNormal = normal;
                            finalPenetration = penetration;
                            finalTileX = x;
                            finalTileY = y;
                            finalTileType = tileType;
                        }
                    }
                }
            }
        }

        // --- NEW: Apply a SINGLE resolution *after* all loops ---
        if (collisionFound)
        {
            transform.Position.Y += finalNormal.Y * finalPenetration;

            if (_entityManager.Spacemen.ContainsKey(entityId))
            {
                var spaceman =  _entityManager.Spacemen[entityId];
                spaceman.IsOnGround = true;
                _entityManager.Spacemen[entityId] = spaceman;
            }
            
            if (_entityManager.Velocities.TryGetValue(entityId, out var velocity))
            {
                velocity.Velocity.Y = 0;
                _entityManager.Velocities[entityId] = velocity;
            }
            
            _entityManager.Transforms[entityId] = transform;
            PublishWorldCollision(entityId, finalTileX, finalTileY, finalTileType);
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
    
    // ----------- Entity Vs Entity ------------
    public void UpdateEntityCollisions()
    {
        var collidableEntities = new List<int>(_entityManager.Colliders.Keys);
        
        for (int i = 0; i < collidableEntities.Count; i++)
        {
            int entityAId = collidableEntities[i];

            for (int j = i + 1; j < collidableEntities.Count; j++)
            {
                int entityBId = collidableEntities[j];

                // --- 1. FILTER: Ignore specific pairs ---
                if (ShouldIgnorePair(entityAId, entityBId)) { continue; }
                
                // 2. CHECK: See if they are overlapping
                if (CheckEntityCollision(entityAId, entityBId, out Vector2 normal, out float penetration))
                {
                    // 3. PUBLISH: Always publish the event for logic systems
                    _eventManager.Publish(new CollisionEvent 
                    { 
                        EntityA = entityAId, 
                        EntityB = entityBId 
                    });

                    // 4. RESOLVE: Only apply pushback if it's a physical pair
                    if (IsPhysicalPair(entityAId, entityBId))
                    {
                        ResolvePhysicalEntityCollision(entityAId, entityBId, normal, penetration);
                    }
                }
            }
        }
    }
    
   private bool CheckEntityCollision(int entityAId, int entityBId, out Vector2 normal, out float penetration)
    {
        normal = Vector2.Zero;
        penetration = 0;

        if (!_entityManager.Transforms.ContainsKey(entityAId) ||
            !_entityManager.Transforms.ContainsKey(entityBId) ||
            !_entityManager.Colliders.ContainsKey(entityAId) ||
            !_entityManager.Colliders.ContainsKey(entityBId))
        {
            return false;
        }

        var transformA = _entityManager.Transforms[entityAId];
        var colliderA = _entityManager.Colliders[entityAId];
        
        var transformB = _entityManager.Transforms[entityBId];
        var colliderB = _entityManager.Colliders[entityBId];
        
        Vector2 posA = new Vector2(transformA.Position.X, transformA.Position.Y);
        Vector2 posB = new Vector2(transformB.Position.X, transformB.Position.Y);

        Vector2 diff = posA - posB;
        float distanceSq = Vector2.DistanceSquared(posA, posB);
        float sumOfRadii = colliderA.Radius + colliderB.Radius;

        bool isColliding = distanceSq < (sumOfRadii * sumOfRadii);

        if (isColliding)
        {
            float distance = (float)Math.Sqrt(distanceSq);
            
            // Calculate normal and penetration
            if (distance > 0)
            {
                normal = diff / distance;
            }
            else
            {
                // Overlapping perfectly, just pick a direction
                normal = new Vector2(1, 0); 
            }
            penetration = sumOfRadii - distance;
        }
        
        return isColliding;
    }
    
    // --- NEW HELPER 1: Ignore Spaceman-vs-Pod ---
    private bool ShouldIgnorePair(int entityAId, int entityBId)
    {
        bool aIsSpaceman = _entityManager.Spacemen.ContainsKey(entityAId);
        bool bIsSpaceman = _entityManager.Spacemen.ContainsKey(entityBId);

        bool aIsPod = _entityManager.Pods.ContainsKey(entityAId);
        bool bIsPod = _entityManager.Pods.ContainsKey(entityBId);

        if ((aIsSpaceman && bIsPod) || (aIsPod && bIsSpaceman)) { return true; }

        return false;
    }
    
    // --- NEW HELPER 2: Check if pushback should happen ---
    private bool IsPhysicalPair(int entityAId, int entityBId)
    {
        // If either entity is a Resource, it's not a physical collision
        // (The ResourceSystem handles the "collision")
        if (_entityManager.Resources.ContainsKey(entityAId) || 
            _entityManager.Resources.ContainsKey(entityBId))
        {
            return false;
        }


        return true;
    }

    // This is the new method that applies the pushback
    // This method resolves pushback. It will no longer be called for resources.
    private void ResolvePhysicalEntityCollision(int entityAId, int entityBId, Vector2 normal, float penetration)
    {
        // Get transforms (we know they exist from CheckEntityCollision)
        var transformA = _entityManager.Transforms[entityAId];
        var transformB = _entityManager.Transforms[entityBId];

        // We can assume both are physical (e.g., Pod vs Pod, Spaceman vs Spaceman)
        // Push both apart equally
        float pushAmount = penetration / 2.0f;

        transformA.Position.X += normal.X * pushAmount;
        transformA.Position.Y += normal.Y * pushAmount;
        
        transformB.Position.X -= normal.X * pushAmount;
        transformB.Position.Y -= normal.Y * pushAmount;
        
        _entityManager.Transforms[entityAId] = transformA;
        _entityManager.Transforms[entityBId] = transformB;
    }
}