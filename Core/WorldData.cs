using System.Drawing;
using StellarSurvivors.WorldGen.Blueprints;
public enum TileType
{
    None,   // Or Empty
    Grass,
    Dirt,
    Water,
    Stone,
    Wood,   
    Iron,
    Gold,
    Sand
}

public class WorldData
{
    public int[,] TileMap { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    public int Yoffset { get; set; }

    public Point? SelectedTile { get; set; } = null;
    // NEW: A 1D array to store the surface Y-coordinate for each column
    public int[] SurfaceHeightMap { get; set; }

    public WorldData(int width, int height, int yOffset)
    {
        Width = width;
        Height = height;
        Yoffset = yOffset;
        
        TileMap = new int[width, height];
        SurfaceHeightMap = new int[width]; 
    }

    public TileType GetTileType(int x, int y)
    {
        // Safety check (this part is perfect)
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return TileType.None; 
        }

        // 1. Read the int ID from the map
        int tileId = TileMap[x, y];

        // 2. Use a switch to map the ID to the correct enum type
        switch (tileId)
        {
            case TileIDs.TILE_GRASS:
                return TileType.Grass;
            
            case TileIDs.TILE_DIRT:
                return TileType.Dirt;

            case TileIDs.TILE_WATER:
                return TileType.Water;

            case TileIDs.TILE_SAND:
                return TileType.Sand;
            
            case TileIDs.TILE_STONE:
                return TileType.Stone;

            case TileIDs.TILE_IRON_ORE:
                return TileType.Iron;

            case TileIDs.TILE_GOLD_ORE:
                return TileType.Gold;

            case TileIDs.TILE_AIR:
            default:
                return TileType.None;
        }
    }
    
    public void SetTileType(int x, int y, TileType type)
    {
        // Safety check
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        // We must map the enum back to the correct ID
        int newId;
        switch (type)
        {
            case TileType.None:   newId = TileIDs.TILE_AIR;     break;
            case TileType.Grass:  newId = TileIDs.TILE_GRASS;   break;
            case TileType.Dirt:   newId = TileIDs.TILE_DIRT;    break;
            case TileType.Water:  newId = TileIDs.TILE_WATER;   break;
            case TileType.Sand:   newId = TileIDs.TILE_SAND;    break;
            case TileType.Stone:  newId = TileIDs.TILE_STONE;   break;
            case TileType.Iron:   newId = TileIDs.TILE_IRON_ORE;  break;
            case TileType.Gold:   newId = TileIDs.TILE_GOLD_ORE;  break;
            default:              newId = TileIDs.TILE_AIR;     break;
        }

        TileMap[x, y] = newId;
    }
    
}