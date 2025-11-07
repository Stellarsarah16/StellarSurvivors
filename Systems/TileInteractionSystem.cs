using Raylib_cs;
using System.Numerics;
using System.Drawing; // For Point
using StellarSurvivors.Core; // Or wherever WorldData is
using StellarSurvivors.WorldGen; // For TileType

namespace StellarSurvivors.Systems
{
    public class TileInteractionSystem : IUpdateSystem
    {
        private WorldData _worldData;
        private const int TILE_SIZE = 16; // Make sure this matches!

        public TileInteractionSystem(WorldData worldData)
        {
            _worldData = worldData;
        }

        // --- Helper to get the tile under the mouse ---
        private Point GetHoveredTile(Camera2D camera)
        {
            // 1. Get mouse position in SCREEN space
            Vector2 mouseScreenPos = Raylib.GetMousePosition();
            
            // 2. Convert to WORLD space
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mouseScreenPos, camera);
            // 3. Convert to TILE index (using your offset logic)
            int x = (int)(mouseWorldPos.X / TILE_SIZE);
            int y = (int)((mouseWorldPos.Y - _worldData.Yoffset) / TILE_SIZE);
            Point hoveredTile = new Point(x, y);
            
            //System.Console.WriteLine($"Hovering tile: [{hoveredTile.X}, {hoveredTile.Y}]");
            
            return hoveredTile;
        }

        public void Update(Game world, float deltaTime)
        {   
            int playerId = world.EntityManager.Players.First();
            if (!world.EntityManager.Spacemen.ContainsKey(playerId))
            {
                return;
            }
            Camera2D camera = world.Camera;

            Point hoveredTile = GetHoveredTile(camera);
            TileType hoveredTileType = _worldData.GetTileType(hoveredTile.X, hoveredTile.Y);

            // --- LOGIC 1: Nothing is selected. Try to select. ---
            if (_worldData.SelectedTile == null)
            {
                // We can't select air
                if (hoveredTileType != TileType.None)
                {
                    _worldData.SelectedTile = hoveredTile;
                    //System.Console.WriteLine($"Selected tile: [{hoveredTile.X}, {hoveredTile.Y}]");
                }
            }
            // --- LOGIC 2: A tile IS selected. Try to move. ---
            else
            {
                Point selectedTile = _worldData.SelectedTile.Value;
                TileType selectedTileType = _worldData.GetTileType(selectedTile.X, selectedTile.Y);

                // Check 1: Is the target tile (hovered) empty?
                if (hoveredTileType == TileType.None)
                {
                    // Check 2: Is the target tile "nearby" (1-tile radius)?
                    int deltaX = Math.Abs(selectedTile.X - hoveredTile.X);
                    int deltaY = Math.Abs(selectedTile.Y - hoveredTile.Y);
                    
                    if (deltaX <= 1 && deltaY <= 1)
                    {
                        // --- SUCCESS! Perform the move ---
                        System.Console.WriteLine($"Moving tile from {selectedTile} to {hoveredTile}");
                        
                        // 1. Place the selected tile in the new empty spot
                        _worldData.SetTileType(hoveredTile.X, hoveredTile.Y, selectedTileType);
                        
                        // 2. Make the old spot empty
                        _worldData.SetTileType(selectedTile.X, selectedTile.Y, TileType.None);
                    }
                }
                
                // Finally, deselect the tile, whether the move was successful or not
                _worldData.SelectedTile = null;
            }
        }
    }
}