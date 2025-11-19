namespace StellarSurvivors.Systems;

using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Components;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;
using StellarSurvivors.WorldGen.TileData;

public class RenderSystem
{
    private RenderLayer _layerToDraw;
    private const int TILE_SIZE = GameConstants.TILE_SIZE;

    public RenderSystem(RenderLayer layerToDraw)
    {
        _layerToDraw = layerToDraw;
    }
    
    public void Draw(Game world, Camera2D camera)
    {
        if (_layerToDraw == RenderLayer.Background)
        {
            DrawTiles(world.WorldData, camera);
        }
        
        if (_layerToDraw == RenderLayer.Entities)
        {
            DrawEntities(world.EntityManager, camera);
        }
        
        // You can add other layers like RenderLayer.UI here,
        // which would not use the camera.
    }
    
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
                TileDefinition tileDefinition = TileRegistry.GetDefinition(tileType);
                TileRenderDefinition renderInfo = tileDefinition.RenderInfo;
                
                //if (tileType == TileType.None) continue; // Skip air

                // 5. Calculate the tile's world position (re-applying offset)
                float worldX = x * TILE_SIZE;
                float worldY = (y * TILE_SIZE) + yOffset;
                Rectangle destRect = new Rectangle(worldX, worldY, TILE_SIZE, TILE_SIZE);
                
                switch (renderInfo.Type)
                {
                    case RenderType.Shape:
                        // For tiles, we just draw the simple rectangle.
                        // We use DrawRectangleRec for non-rotated rects.
                        Raylib.DrawRectangleRec(destRect, renderInfo.Color);
                        break;
                    
                    case RenderType.Texture:
                        if (renderInfo.Texture.Id == 0) 
                        {
                            // Fallback for missing texture
                            Raylib.DrawRectangleRec(destRect, Color.Magenta);
                            break;
                        }
                        
                        Raylib.DrawTexturePro(
                            renderInfo.Texture,   // The texture to draw
                            renderInfo.SourceRect, // Which part of the texture to use
                            destRect,          // Where on the screen to draw it
                            Vector2.Zero,      // Origin (not needed for 0 rotation)
                            0f,                // Rotation (tiles don't rotate)
                            renderInfo.Tint    // Tint (Color.White for no tint)
                        );
                        break;
                }
                
                // --- HIGHLIGHT LOGIC ---
        
                // Check for Selected Tile (Yellow)
                if (worldData.Interaction.SelectedTile != null && 
                    worldData.Interaction.SelectedTile.Value.X == x && 
                    worldData.Interaction.SelectedTile.Value.Y == y)
                {
                    // Draw a thick yellow border for the "held" item
                    Raylib.DrawRectangleLinesEx(
                        new Rectangle(worldX, worldY, TILE_SIZE, TILE_SIZE), 
                        2, Color.Yellow);
                }
                // Check for Highlighted Tile (White)
                if (worldData.Interaction.HoveredTile != null && 
                         worldData.Interaction.HoveredTile.Value.X == x && 
                         worldData.Interaction.HoveredTile.Value.Y == y)
                {
                    // Draw a thin white border for the "hover/target"
                    Raylib.DrawRectangleLinesEx(
                        new Rectangle(worldX, worldY, TILE_SIZE, TILE_SIZE), 
                        1, worldData.Interaction.HoveredColor);
                }
            }
        }
    }
    
    private void DrawEntities(EntityManager entityManager, Camera2D camera)
    {
        foreach (var entityId in entityManager.Renderables.Keys)
        {
            var ren = entityManager.Renderables[entityId];
            if (ren.Layer != _layerToDraw) continue;
            
            if (entityManager.Transforms.ContainsKey(entityId))
            {
                var transform = entityManager.Transforms[entityId];
                var render = entityManager.Renderables[entityId];
                float degrees = transform.Rotation * (180f / MathF.PI);
                
                entityManager.PlayerInputs.TryGetValue(entityId, out var input);

                Vector2 pos2d = new Vector2(transform.Position.X, transform.Position.Y);
                Vector2 scaledSize = new Vector2(render.Size.X * transform.Scale.X, render.Size.Y * transform.Scale.Y);
                Vector2 origin = new Vector2(scaledSize.X / 2.0f, scaledSize.Y / 2.0f);
                Rectangle destRect = new Rectangle(pos2d.X, pos2d.Y, scaledSize.X, scaledSize.Y);

                Rectangle finalSourceRect = render.SourceRect;
                if (input != null)
                {
                    finalSourceRect.Width *= input.FacingDirection;
                }
                
                switch (render.Type)
                {
                    case RenderType.Shape:
                        switch (render.Shape)
                        {
                            case ShapeType.PodTriangle:
                            {
                                //Refactor pod Drawing Logic to here
                                Vector2 podCenter = new Vector2(transform.Position.X, transform.Position.Y);
                                float size = render.Size.X;
                                
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
                                break;
                            }    
                            case ShapeType.Rectangle:
                            {
                                //Refactor Rectangle Drawing Logic to here

                                Raylib.DrawRectanglePro(
                                    destRect,
                                    origin,
                                    degrees,
                                    ren.Color
                                );
                                
                                break;
                            }    
                        }
                        break;
                    
                    case RenderType.Texture:
                        if (render.Texture.Id == 0) {
                            Raylib.DrawRectanglePro(destRect, origin, degrees, Color.Magenta);
                            break;
                        }
                        
                        Raylib.DrawTexturePro(
                            render.Texture,   // The texture to draw
                            finalSourceRect, // Which part of the texture to use
                            destRect,          // Where on the screen to draw it (pos, size)
                            origin,            // The center of rotation
                            degrees,           // The rotation
                            render.Tint        // The color tint (use Color.White for no tint)
                        );
                        break; // End RenderType.Texture
                }
            }
        }
    }
}
