using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.WorldGen; // <-- Added this for TileType

namespace StellarSurvivors.Systems
{
    public class RenderSystem
    {
        private RenderLayer _layerToDraw;
        private const int TILE_SIZE = 16; // <-- Make sure this matches your game's tile size

        public RenderSystem(RenderLayer layerToDraw)
        {
            _layerToDraw = layerToDraw;
        }
        
        public void Draw(Game world, Camera2D camera)
        {
            // --- NEW LOGIC SWITCH ---
            
            // If this is the background renderer, draw the visible tiles from data
            if (_layerToDraw == RenderLayer.Background)
            {
                DrawTiles(world.WorldData, camera);
            }
            
            // If this is the main renderer, draw the entities (Pod, Spaceman, etc.)
            // We assume your tiles DO NOT have this layer, so the loop is small and fast.
            if (_layerToDraw == RenderLayer.Entities)
            {
                DrawEntities(world.EntityManager, camera);
            }
            
            // You can add other layers like RenderLayer.UI here,
            // which would not use the camera.
        }

        /// <summary>
        /// NEW: Draws the tilemap data based on camera position.
        /// </summary>
        private void DrawTiles(WorldData worldData, Camera2D camera)
        {
            int yOffset = worldData.Yoffset;

            // 1. Get the top-left and bottom-right corners of the camera's view
            Vector2 camTopLeft = Raylib.GetScreenToWorld2D(Vector2.Zero, camera);
            Vector2 camBottomRight = Raylib.GetScreenToWorld2D(new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), camera);

            // 2. Convert these world coordinates into tile indices
            // We "undo" the y-offset to get the correct array index
            int minTileX = (int)(camTopLeft.X / TILE_SIZE) - 1;
            int maxTileX = (int)(camBottomRight.X / TILE_SIZE) + 1;
            int minTileY = (int)((camTopLeft.Y - yOffset) / TILE_SIZE) - 1;
            int maxTileY = (int)((camBottomRight.Y - yOffset) / TILE_SIZE) + 1;

            // 3. Clamp the indices to be inside the map array
            minTileX = Math.Max(0, minTileX);
            minTileY = Math.Max(0, minTileY);
            maxTileX = Math.Min(worldData.Width - 1, maxTileX);
            maxTileY = Math.Min(worldData.Height - 1, maxTileY);

            // 4. Loop ONLY over the visible tiles
            for (int y = minTileY; y <= maxTileY; y++)
            {
                for (int x = minTileX; x <= maxTileX; x++)
                {
                    TileType tileType = worldData.GetTileType(x, y); // Assumes GetTileType is on WorldData
                    if (tileType == TileType.None) continue; // Skip air

                    // 5. Calculate the tile's world position (re-applying offset)
                    float worldX = x * TILE_SIZE;
                    float worldY = (y * TILE_SIZE) + yOffset;
                    
                    // 6. Draw the tile
                    // (Replace this with a texture-lookup if you have one)
                    Raylib.DrawRectangle((int)worldX, (int)worldY, TILE_SIZE, TILE_SIZE, GetColorForTile(tileType));
                    if (worldData.SelectedTile != null)
                    {
                        if (worldData.SelectedTile.Value.X == x && worldData.SelectedTile.Value.Y == y)
                        {
                            // Draw a bright yellow border around the selected tile
                            Raylib.DrawRectangleLines((int)worldX, (int)worldY, TILE_SIZE, TILE_SIZE, Color.Yellow);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is your original code, now in its own method.
        /// </summary>
        private void DrawEntities(EntityManager entityManager, Camera2D camera)
        {
            foreach (var entityId in entityManager.Renderables.Keys)
            {
                var ren = entityManager.Renderables[entityId];
                if (ren.Layer == _layerToDraw)
                {
                    if (entityManager.Transforms.ContainsKey(entityId))
                    {
                        var transform = entityManager.Transforms[entityId];
                        var render = entityManager.Renderables[entityId];
                        float degrees = transform.Rotation * (180f / MathF.PI);
                        
                        // --- Pod Drawing Logic ---
                        if (entityManager.Pods.ContainsKey(entityId))
                        {
                            Vector2 podCenter = new Vector2(transform.Position.X, transform.Position.Y);
                            float size = 15f;
                            Vector2 p1_local = new Vector2(0, -size * 1.5f);
                            Vector2 p2_local = new Vector2(-size, size);
                            Vector2 p3_local = new Vector2(size, size);

                            Vector2 RotateAndTranslate(Vector2 localPoint, float angle, Vector2 translation)
                            {
                                float cos = MathF.Cos(angle);
                                float sin = MathF.Sin(angle);
                                float rotatedX = localPoint.X * cos - localPoint.Y * sin;
                                float rotatedY = localPoint.X * sin + localPoint.Y * cos;
                                return new Vector2(rotatedX + translation.X, rotatedY + translation.Y);
                            }

                            Vector2 p1 = RotateAndTranslate(p1_local, transform.Rotation, podCenter);
                            Vector2 p2 = RotateAndTranslate(p2_local, transform.Rotation, podCenter);
                            Vector2 p3 = RotateAndTranslate(p3_local, transform.Rotation, podCenter);
                            
                            Raylib.DrawTriangle(p1, p2, p3, render.Color);
                        }
                        // --- Spaceman/Other Entity Drawing Logic ---
                        else 
                        {
                            Vector2 pos2d = new Vector2(transform.Position.X, transform.Position.Y);
                            Vector2 size = ren.Size;
                            Vector2 scaledSize = new Vector2(size.X * transform.Scale.X, size.Y * transform.Scale.Y);
                            Rectangle destRect = new Rectangle(pos2d.X, pos2d.Y, scaledSize.X, scaledSize.Y);
                            Vector2 origin = new Vector2(scaledSize.X / 2.0f, scaledSize.Y / 2.0f);

                            Raylib.DrawRectanglePro(
                                destRect,
                                origin,
                                degrees,
                                ren.Color
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// NEW: Helper to draw colors. You can change this to
        /// return textures from a TextureManager.
        /// </summary>
        private Color GetColorForTile(TileType type)
        {
            switch (type)
            {
                // --- Ground Colors ---
                case TileType.Grass: 
                    return new Color(30, 120, 40, 255);
        
                // --- THIS WAS YOUR MISSING TILE ---
                case TileType.Dirt: 
                    return new Color(130, 80, 50, 255);
        
                // --- THIS IS THE CORRECT SAND COLOR ---
                case TileType.Sand: 
                    return new Color(194, 178, 128, 255);

                // --- THIS IS THE CORRECT WATER COLOR ---
                case TileType.Water: 
                    return new Color(50, 80, 200, 255);

                // --- Ore Colors ---
                case TileType.Stone: 
                    return new Color(140, 140, 140, 255);
                case TileType.Iron: 
                    return new Color(110, 110, 130, 255);
                case TileType.Gold: 
                    return new Color(210, 180, 20, 255);
        
                // --- Default (Air) ---
                default: 
                    return Color.Blank;
            }
        }
    }
}