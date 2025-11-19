namespace StellarSurvivors.Core;

using Raylib_cs;
using System.Drawing;
using StellarSurvivors.WorldGen.Blueprints;
using StellarSurvivors.WorldGen.TileData;
using StellarSurvivors.Enums;
using Color = Raylib_cs.Color;
using StellarSurvivors.Components;
using System.Numerics;


public class WorldData
{
    public int[,] TileMap { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    public int Yoffset { get; set; }
    public int[] SurfaceHeightMap { get; set; }
    
    public WorldInteractionState Interaction { get; set; }
    
    public WorldData(int width, int height, int yOffset)
    {
        Width = width;
        Height = height;
        Yoffset = yOffset;
        
        TileMap = new int[width, height];
        SurfaceHeightMap = new int[width];
        
        // Initialize the separate interaction state
        Interaction = new WorldInteractionState();
    }
    
    public TileType GetTileType(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return TileType.None; 
        
        // Delegate to the TileMapper system
        return TileMapper.IdToEnum(TileMap[x, y]);
    }
    
    public void SetTileType(int x, int y, TileType type)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;

        // Delegate to the TileMapper system
        TileMap[x, y] = TileMapper.EnumToId(type);
    }

    public ResourceType GetResourceAtWorldTile(int x, int y)
    {
        int tileX = x;
        int tileY = x;
        TileType tileType = GetTileType(tileX, tileY - 600); 
                
        TileDefinition tileDef =  TileRegistry.GetDefinition(tileType);
        if (tileDef.DropsResource.HasValue)
        {
            ResourceType resource = tileDef.DropsResource.Value;
            return resource;
        }
        else { return ResourceType.None; }
    }
    
    public Point GetPointFromWorldPos(Vector2 pos, float yOffset)
    {
        return new Point(
            (int)(pos.X / GameConstants.TILE_SIZE),
            (int)((pos.Y - yOffset) / GameConstants.TILE_SIZE)
        );
    }

    public Vector2 GetWorldPosFromTile(Point tile, float yOffset)
    {
        return new Vector2(
            (tile.X * GameConstants.TILE_SIZE) + (GameConstants.TILE_SIZE / 2f),
            (tile.Y * GameConstants.TILE_SIZE) + yOffset + (GameConstants.TILE_SIZE / 2f)
        );
    }
}
