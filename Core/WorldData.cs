namespace StellarSurvivors.Core;

using Raylib_cs;
using System.Drawing;
using StellarSurvivors.WorldGen.Blueprints;
using StellarSurvivors.Enums;
using Color = Raylib_cs.Color;
using StellarSurvivors.Components;


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
    private readonly TileDefinition _defaultAirDef;

    public WorldData(int width, int height, int yOffset)
    {
        Width = width;
        Height = height;
        Yoffset = yOffset;
        
        TileMap = new int[width, height];
        SurfaceHeightMap = new int[width];
        
        // Init Tile Registry
        _tileRegistry = new Dictionary<TileType, TileDefinition>();
        _defaultAirDef = new TileDefinition
        { 
            Name = "Air", 
            IsSolid = false, 
            Friction = 1.0f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = Color.Blank
            }
        };
        
    }
    
    public void InitializeTileRegistry()
    {
        Texture2D goldTexture = AssetManager.GetTexture("gold");
        Texture2D ironTexture = AssetManager.GetTexture("iron");
        Texture2D stoneTexture = AssetManager.GetTexture("stone");
        Texture2D coalTexture = AssetManager.GetTexture("coal");
        Texture2D grassTexture = AssetManager.GetTexture("grass");
        
        // Air / None
        RegisterTile(TileType.None, _defaultAirDef);

        // Solid Blocks
        RegisterTile(TileType.Dirt, new TileDefinition 
        { 
            Name = "Dirt", 
            IsSolid = true, 
            Friction = 2f, // Slightly slower to walk on dirt?
            Hardness = 1.0f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = new Color(130, 80, 50, 255)
            }
        });

        RegisterTile(TileType.Stone, new TileDefinition 
        { 
            Name = "Stone", 
            IsSolid = true, 
            Hardness = 5.0f,
            DropsResource = ResourceType.Stone,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Texture,
                Texture = stoneTexture,
                // Grabs the first 16x16 square from the 'terrain' texture
                SourceRect = new Raylib_cs.Rectangle(0, 0, GameConstants.TILE_SIZE, GameConstants.TILE_SIZE),
                Tint = Color.White
            }
        });

        // Special interactions
        RegisterTile(TileType.Water, new TileDefinition 
        { 
            Name = "Water", 
            IsSolid = false, // Not solid, but maybe we add 'IsLiquid' later for swimming logic
            Friction = 4f, // Slippery!
            Hardness = 0f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = new Color(50, 80, 200, 255)
            }
        });
        
        RegisterTile(TileType.Grass, new TileDefinition 
        { 
            Name = "Grass", 
            IsSolid = false,
            IsFlammable = true,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Texture,
                Texture = grassTexture,
                // Grabs the first 16x16 square from the 'terrain' texture
                SourceRect = new Raylib_cs.Rectangle(0, 0, GameConstants.TILE_SIZE, GameConstants.TILE_SIZE),
                Tint = Color.White
            }
        });
        RegisterTile(TileType.Sand, new TileDefinition 
        { 
            Name = "Sand", 
            IsSolid = false,
            IsFlammable = false,
            Friction = 3f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = new Color(194, 178, 128, 255)
            }
        });
        
        RegisterTile(TileType.Iron, new TileDefinition 
        { 
            Name = "Iron", 
            IsSolid = true,
            IsFlammable = false,
            Hardness = 5.0f,
            DropsResource = ResourceType.IronOre,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Texture,
                Texture = ironTexture,
                // Grabs the first 16x16 square from the 'terrain' texture
                SourceRect = new Raylib_cs.Rectangle(0, 0, GameConstants.TILE_SIZE, GameConstants.TILE_SIZE),
                Tint = Color.White
            }
        });
        RegisterTile(TileType.Gold, new TileDefinition 
        { 
            Name = "Gold", 
            IsSolid = true,
            IsFlammable = false,
            Hardness = 7.0f,
            DropsResource = ResourceType.GoldOre,
            RenderInfo = new TileRenderDefinition
            {
            Type = RenderType.Texture,
            Texture = goldTexture,
            // Grabs the first 16x16 square from the 'terrain' texture
            SourceRect = new Raylib_cs.Rectangle(0, 0, GameConstants.TILE_SIZE, GameConstants.TILE_SIZE),
            Tint = Color.White
            }
        });
        RegisterTile(TileType.Coal, new TileDefinition 
        { 
            Name = "Coal", 
            IsSolid = true,
            IsFlammable = true,
            Hardness = 4.0f,
            DropsResource = ResourceType.Coal,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Texture,
                Texture = coalTexture,
                // Grabs the first 16x16 square from the 'terrain' texture
                SourceRect = new Raylib_cs.Rectangle(0, 0, GameConstants.TILE_SIZE, GameConstants.TILE_SIZE),
                Tint = Color.White
            }
        });
        RegisterTile(TileType.Hard, new TileDefinition 
        { 
            Name = "Hard", 
            IsSolid = true,
            IsFlammable = false,
            Hardness = 100f,
            DropsResource = ResourceType.Coal,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = new Color(100, 80, 60, 255)
            }
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
            case TileIDs.TILE_GRASS:     return TileType.Grass;
            case TileIDs.TILE_DIRT:      return TileType.Dirt;
            case TileIDs.TILE_WATER:     return TileType.Water;
            case TileIDs.TILE_SAND:      return TileType.Sand;
            case TileIDs.TILE_STONE:     return TileType.Stone;
            case TileIDs.TILE_IRON_ORE:  return TileType.Iron;
            case TileIDs.TILE_GOLD_ORE:  return TileType.Gold;
            case TileIDs.TILE_COAL:      return TileType.Coal;
            case TileIDs.TILE_HARD:      return TileType.Hard;

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
            case TileType.Coal:   newId = TileIDs.TILE_COAL;    break;
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
