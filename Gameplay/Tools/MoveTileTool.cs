namespace StellarSurvivors.Gameplay.Tools;
using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;
using System.Drawing;

public class MoveTileTool : ITool
{
    private const int TILE_SIZE = GameConstants.TILE_SIZE;
    private readonly float _selectableRange = 124f;
    public float FuelCostPerSecond { get; } = 0f; // Stays Zero
    public float FuelCostPerClick { get; } = 0f; // Set to desired amount

    // 1. PASSIVE UPDATE (Handles Hovering)
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
        if (distance > _selectableRange) return;

        TileType hoveredType = worldData.GetTileType(hoveredTile.X, hoveredTile.Y);
        // LOGIC: Decide if we should highlight this tile
        if (worldData.Interaction.SelectedTile == null)
        {
            // MODE: Looking to pick up (highlight if NOT empty)
            if (hoveredType != TileType.None)
            {
                worldData.Interaction.HoveredTile = hoveredTile;
            }
        }
        else
        {
            // MODE: Looking to drop (highlight if IS empty)
            if (hoveredType == TileType.None)
            {
                worldData.Interaction.HoveredTile = hoveredTile;
            }
        }
    }

    // 2. ACTIVE USE (Handles Clicking)
    public void Use(int ownerId, Vector2 mouseWorldPos, float deltaTime, Game world, bool justPressed)
    {
        // We ONLY care about the initial click for this tool, not holding it down.
        if (!justPressed) return;

        WorldData worldData = world.WorldData;

        if (worldData.Interaction.HoveredTile.HasValue)
        {
            Point clickedTile = worldData.Interaction.HoveredTile.Value;
            Console.WriteLine(worldData.Interaction.HoveredTile.Value);
            if (worldData.Interaction.SelectedTile == null)
            {
                // PICK UP
                worldData.Interaction.SelectedTile = clickedTile;
                worldData.Interaction.HoveredTile = null; // Clear immediate hover
            }
            else
            {
                // DROP / SWAP
                Point sourceTile = worldData.Interaction.SelectedTile.Value;
                TileType sourceType = worldData.GetTileType(sourceTile.X, sourceTile.Y);

                worldData.SetTileType(clickedTile.X, clickedTile.Y, sourceType);
                worldData.SetTileType(sourceTile.X, sourceTile.Y, TileType.None);

                // Reset state
                worldData.Interaction.SelectedTile = null;
                worldData.Interaction.HoveredTile = null;
            }
        }
        else
        {
            // Clicked invalid area (out of range, or on a wall while holding a wall)
            // Cancel selection
            worldData.Interaction.SelectedTile = null;
        }
    }

    public void StopUse(int ownerId, Game world) 
    {
        // Not needed for Click-Click behavior
    }

    // --- Helpers ---
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