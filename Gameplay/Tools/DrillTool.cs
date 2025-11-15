namespace StellarSurvivors.Gameplay.Tools;
using System.Numerics;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;
using StellarSurvivors.Components;
using System.Drawing;
using Raylib_cs;

public class DrillTool : ITool
{
    public float MiningSpeed { get; set; } = 10f; // "Damage" per second
    private const int TILE_SIZE = GameConstants.TILE_SIZE; // Must match your other systems
    private readonly float _selectableRange = 124f;
    public float FuelCostPerSecond { get; } = 2f; // 0.5 fuel per second
    public float FuelCostPerClick { get; } = 0f;

    public void Update(int ownerId, Vector2 mouseWorldPos, Game world)
    {
        WorldData worldData = world.WorldData;
        
        // Always clear hover first
        worldData.HoveredTile = null;

        // Get Owner Position
        if (!world.EntityManager.Transforms.TryGetValue(ownerId, out var transform)) return;
        Vector2 ownerPos = new Vector2(transform.Position.X, transform.Position.Y);
        
        // Calculate tile under mouse
        Point hoveredTile = GetTileFromWorldPos(mouseWorldPos, worldData.Yoffset);
        Vector2 hoveredWorldCenter = GetWorldPosFromTile(hoveredTile, worldData.Yoffset);
        float distance = Vector2.Distance(ownerPos, hoveredWorldCenter);
        
        // Range check
        if (distance > _selectableRange) return;

        TileType hoveredType = worldData.GetTileType(hoveredTile.X, hoveredTile.Y);
        TileDefinition tileDef = worldData.GetTileDef(hoveredType);
        
        
        // MODE: Looking to pick up (highlight if NOT empty)
        if (tileDef.IsSolid || tileDef.Hardness > 0)
        {
            worldData.HoveredTile = hoveredTile;
        }
    
    }

public void Use(int ownerId, Vector2 targetWorldPos, float deltaTime, Game world, bool justPressed)
    {
        var entityManager = world.EntityManager;
        var worldData = world.WorldData;

        if (justPressed)
        {
            // 1. Get Owner Position and check range
            if (!entityManager.Transforms.TryGetValue(ownerId, out var transform)) return;
            Vector2 ownerPos = new Vector2(transform.Position.X, transform.Position.Y);
            
            Point clickedTile = GetTileFromWorldPos(targetWorldPos, worldData.Yoffset);
            Vector2 clickedWorldCenter = GetWorldPosFromTile(clickedTile, worldData.Yoffset);
            float distance = Vector2.Distance(ownerPos, clickedWorldCenter);

            // 2. Range check
            if (distance > _selectableRange) { return; }
            
            // 3. Get Tile Info
            TileType tileType = worldData.GetTileType(clickedTile.X, clickedTile.Y);
            TileDefinition tileDef = worldData.GetTileDef(tileType);
            
            // 4. Check if tile is minable
            if (!tileDef.IsSolid || tileDef.Hardness <= 0) { return; }

            // 5. Create and save the NEW mining state
            var miningState = new MiningComponent
            {
                TargetTileX = clickedTile.X,
                TargetTileY = clickedTile.Y,
                CurrentHealth = tileDef.Hardness - (MiningSpeed * deltaTime) // Apply first tick of damage
            };
            entityManager.MiningComponents[ownerId] = miningState;
        }
        else
        {
            // Frame 2. button still pressed           
            // 1. Get the existing mining state
            if (!entityManager.MiningComponents.TryGetValue(ownerId, out var miningState))
                return;

            // 2. Apply mining damage
            miningState.CurrentHealth -= MiningSpeed * deltaTime;

            // 3. Check for tile break
            if (miningState.CurrentHealth <= 0)
            {
                
                // Get tile info BEFORE destroying it
                int tileX = miningState.TargetTileX;
                int tileY = miningState.TargetTileY;
                TileType tileType = worldData.GetTileType(tileX, tileY); 
                
                TileDefinition tileDef = worldData.GetTileDef(tileType);

                // Destroy tile
                worldData.SetTileType(tileX, tileY, TileType.None);
                
                if (tileDef.DropsResource.HasValue)
                {
                    // It does! Spawn that resource.
                    ResourceType resource = tileDef.DropsResource.Value;
                    Vector2 tileCenter = GetWorldPosFromTile(new Point(tileX, tileY), worldData.Yoffset);
                    world.CreateResourceEntity(tileCenter, resource, 1, tileDef.RenderInfo.Color);
                }

                // Stop mining (removes the component)
                StopUse(ownerId, world);
            }
            else
            {
                // Save the progress
                entityManager.MiningComponents[ownerId] = miningState;
            }
        }
    }

    public void StopUse(int ownerId, Game world)
    {
        // When mouse is released, remove the mining state
        world.EntityManager.MiningComponents.Remove(ownerId);
    }
    
    
    
    private Point GetTileFromWorldPos(Vector2 pos, float yOffset)
    {
        return new Point(
            (int)(pos.X / TILE_SIZE),
            (int)((pos.Y - yOffset) / TILE_SIZE)
        );
    }

    private Vector2 GetWorldPosFromTile(Point tile, float yOffset)
    {
        return new Vector2(
            (tile.X * TILE_SIZE) + (TILE_SIZE / 2f),
            (tile.Y * TILE_SIZE) + yOffset + (TILE_SIZE / 2f)
        );
    }
}
