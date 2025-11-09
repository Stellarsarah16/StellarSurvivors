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
        private readonly float _selectableRange = 124f;

        public TileInteractionSystem(WorldData worldData)
        {
            _worldData = worldData;
        }

        public void Update(Game world, float deltaTime)
        {   
            int playerId = world.EntityManager.Players.First();
            if (!world.EntityManager.Spacemen.ContainsKey(playerId))
            {
                _worldData.HoveredTile = null;
                _worldData.SelectedTile = null;
                return;
            }
            
            Camera2D camera = world.Camera;
            Vector2 playerPosition = camera.Target;
            Point hoveredTile = GetHoveredTile(camera);
            Vector2 hoveredWorldPosition = GetWorldPosFromTile(hoveredTile);
            float distanceToPlayer = Vector2.Distance(playerPosition, hoveredWorldPosition);
            TileType hoveredTileType = _worldData.GetTileType(hoveredTile.X, hoveredTile.Y);

            _worldData.HoveredTile = null;  // Clear Each frame
            
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            // --- 4. STATE MACHINE ---

            // ====== STATE 1: No tile is selected (Looking to pick one up) ======
            if (_worldData.SelectedTile == null)
            {
                // Is the hovered tile in range AND not empty?
                if (distanceToPlayer <= _selectableRange && hoveredTileType != TileType.None)
                {
                    // If so, highlight it
                    _worldData.HoveredTile = hoveredTile;

                    // If we click the highlighted tile, select it
                    if (isClick)
                    {
                        _worldData.SelectedTile = hoveredTile;
                        _worldData.HoveredTile = null; // Clear highlight
                    }
                }
            }
            else
            {
                Point selectedTile = _worldData.SelectedTile.Value;

                // Is the hovered tile in range AND empty?
                if (distanceToPlayer <= _selectableRange && hoveredTileType == TileType.None)
                {
                    // If so, highlight it as a valid "drop target"
                    _worldData.HoveredTile = hoveredTile;

                    // If we click this valid drop target...
                    if (isClick)
                    {
                        // 1. Get the type of the tile we're "holding"
                        TileType selectedTileType = _worldData.GetTileType(selectedTile.X, selectedTile.Y);

                        // 2. "Move" it by swapping the tile data
                        _worldData.SetTileType(hoveredTile.X, hoveredTile.Y, selectedTileType);
                        _worldData.SetTileType(selectedTile.X, selectedTile.Y, TileType.None);

                        // 3. Clear all state
                        _worldData.SelectedTile = null;
                        _worldData.HoveredTile = null;
                    }
                }

                // If we click anywhere else (on a wall, out of range, etc.)...
                else if (isClick)
                {
                    // ...just cancel the selection and drop the tile.
                    _worldData.SelectedTile = null;
                }
            }
        }
        
        // --- Helper to get the tile under the mouse ---
        private Point GetHoveredTile(Camera2D camera)
        {
            Vector2 mouseScreenPos = Raylib.GetMousePosition();
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mouseScreenPos, camera);
            int x = (int)(mouseWorldPos.X / TILE_SIZE);
            int y = (int)((mouseWorldPos.Y - _worldData.Yoffset) / TILE_SIZE);
            return new Point(x, y);
        }
        
        private Vector2 GetWorldPosFromTile(Point tile)
        {
            return new Vector2(
                (tile.X * TILE_SIZE) + (TILE_SIZE / 2f), // Get center of tile
                (tile.Y * TILE_SIZE) + _worldData.Yoffset + (TILE_SIZE / 2f)
            );
        }
    }
}