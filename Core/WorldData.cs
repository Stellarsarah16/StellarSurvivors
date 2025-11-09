using System.Drawing;
using StellarSurvivors.WorldGen.Blueprints;


public class WorldData
{
    public int[,] TileMap { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    public int Yoffset { get; set; }

    public Point? SelectedTile { get; set; } = null;
    public Point? HoveredTile { get; set; } =  null;
    // NEW: A 1D array to store the surface Y-coordinate for each column
    public int[] SurfaceHeightMap { get; set; }
    private Dictionary<TileType, TileDefinition> _tileRegistry { get; set; }
    // A fallback definition for unregistered tiles (prevents crashes)
    private readonly TileDefinition _defaultAirDef = new TileDefinition 
    { 
        Name = "Air", 
        IsSolid = false, 
        Friction = 1.0f 
    };

    public WorldData(int width, int height, int yOffset)
    {
        Width = width;
        Height = height;
        Yoffset = yOffset;
        
        TileMap = new int[width, height];
        SurfaceHeightMap = new int[width];
        
        // Init Tile Registry
        _tileRegistry = new Dictionary<TileType, TileDefinition>();
        InitializeTileRegistry();
    }
    
    private void InitializeTileRegistry()
    {
        // Air / None
        RegisterTile(TileType.None, new TileDefinition 
        { 
            Name = "Air", 
            IsSolid = false 
        });

        // Solid Blocks
        RegisterTile(TileType.Dirt, new TileDefinition 
        { 
            Name = "Dirt", 
            IsSolid = true, 
            Friction = 1.1f, // Slightly slower to walk on dirt?
            Hardness = 1.0f 
        });

        RegisterTile(TileType.Stone, new TileDefinition 
        { 
            Name = "Stone", 
            IsSolid = true, 
            Hardness = 5.0f 
        });

        // Special interactions
        RegisterTile(TileType.Water, new TileDefinition 
        { 
            Name = "Water", 
            IsSolid = false, // Not solid, but maybe we add 'IsLiquid' later for swimming logic
            Friction = 0.5f, // Slippery!
            Hardness = 0f 
        });
        
        RegisterTile(TileType.Grass, new TileDefinition 
        { 
            Name = "Grass", 
            IsSolid = false,
            IsFlammable = true 
        });
    }
    
    public void RegisterTile(TileType type, TileDefinition def)
    {
        // We use the indexer [] instead of .Add() so that if we accidentally 
        // register something twice, it just updates it instead of crashing.
        _tileRegistry[type] = def;
    }
    
    public TileType GetTileType(int x, int y)
    {
        // Safety check (this part is perfect)
        if (x < 0 || x >= Width || y < 0 || y >= Height) return TileType.None; 

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
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;

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

    // 3. The Getter Method
    public TileDefinition GetTileDef(TileType type)
    {
        if (_tileRegistry.TryGetValue(type, out TileDefinition def))
        {
            return def;
        }
        // If requested tile isn't registered, return the safe default
        return _defaultAirDef;
    }

}

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

public class TileDefinition
{
    public string Name { get; set; } = "Unknown";
    public bool IsSolid = false;
    public float Friction = 1.0f;
    public float Hardness  = 0f;
    public bool IsFlammable = false;
}