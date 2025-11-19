namespace StellarSurvivors.Gameplay.Tools;
using System.Numerics;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;
using StellarSurvivors.Components;
using System.Drawing;
using Raylib_cs;
using StellarSurvivors.WorldGen.TileData;

public class MinerPlacerTool : ITool
{
    public float MiningSpeed { get; set; } = 10f; // "Damage" per second
    private const int TILE_SIZE = GameConstants.TILE_SIZE; // Must match your other systems
    private readonly float _placeableRange = 124f;
    public float FuelCostPerSecond { get; } = .0f; // 0.5 fuel per second
    public float FuelCostPerClick { get; } = 5f;

    public void Update(int ownerId, Vector2 mouseWorldPos, Game world)
    {
        WorldData worldData = world.WorldData;
        
        // Always clear hover first
        worldData.Interaction.HoveredTile = null;

        // Get Owner Position
        if (!world.EntityManager.Transforms.TryGetValue(ownerId, out var transform)) return;
        Vector2 ownerPos = new Vector2(transform.Position.X, transform.Position.Y);
        
        // Calculate tile under mouse
        Point hoveredTile = GetTileFromWorldPos(mouseWorldPos, worldData.Yoffset);
        Vector2 hoveredWorldCenter = GetWorldPosFromTile(hoveredTile, worldData.Yoffset);
        float distance = Vector2.Distance(ownerPos, hoveredWorldCenter);
        
        // Range check
        if (distance > _placeableRange) return;

        TileType hoveredType = worldData.GetTileType(hoveredTile.X, hoveredTile.Y);
        TileDefinition tileDef = TileRegistry.GetDefinition(hoveredType);
        
        // MODE: Looking to pick up (highlight if NOT empty)
        if (tileDef.IsSolid || tileDef.Hardness > 0)
        {
            worldData.Interaction.HoveredTile = hoveredTile;
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
            if (distance > _placeableRange) { return; }
            
            // 3. Get Tile Info
            TileType tileType = worldData.GetTileType(clickedTile.X, clickedTile.Y);
            TileDefinition tileDef = TileRegistry.GetDefinition(tileType);
            ResourceType resource = worldData.GetResourceAtWorldTile(clickedTile.X, clickedTile.Y);
            
            // 4. Check if tile is minable
            if (tileDef.DropsResource.HasValue)
            {
                Vector2 spot = new  Vector2(clickedTile.X, clickedTile.Y);
                // Destroy old Tile
                worldData.SetTileType(clickedTile.X, clickedTile.Y, TileType.None);
                
                int minerId = world.WorldGenerator.EntityFactory.CreateMinerEntity(spot, resource, 1, Raylib_cs.Color.Green);
                Console.WriteLine($"Placed Miner for {tileType} at {targetWorldPos}");    
            }
            else
            {
                Console.WriteLine("Cannot place miner here: No resources found.");
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
