namespace StellarSurvivors.WorldGen.TileData;
using Raylib_cs;
using StellarSurvivors.Enums;
using StellarSurvivors.Components;
using StellarSurvivors.Core;

public static class TileRegistry
{
    private static Dictionary<TileType, TileDefinition> _registry = new();
    private static readonly TileDefinition _defaultAirDef;

    static TileRegistry()
    {
        // Define default air immediately
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

    public static void Initialize()
    {
        Texture2D goldTexture = AssetManager.GetTexture("gold");
        Texture2D ironTexture = AssetManager.GetTexture("iron");
        Texture2D stoneTexture = AssetManager.GetTexture("stone");
        Texture2D coalTexture = AssetManager.GetTexture("coal");
        Texture2D grassTexture = AssetManager.GetTexture("grass");
        
        // Register Definitions (moved from WorldData)
        Register(TileType.None, _defaultAirDef);

        // Dirt
        Register(TileType.Dirt, new TileDefinition 
        { 
            Name = "Dirt", 
            IsSolid = true, 
            Friction = 2f, 
            Hardness = 1.0f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = new Color(130, 80, 50, 255)
            }
        });
        
        // Stone
        Register(TileType.Stone, new TileDefinition 
        { 
            Name = "Stone", 
            IsSolid = true, 
            Hardness = 5.0f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Texture,
                Texture = stoneTexture,
                // Grabs the first 16x16 square from the 'terrain' texture
                SourceRect = new Raylib_cs.Rectangle(0, 0, GameConstants.TILE_SIZE, GameConstants.TILE_SIZE),
                Tint = Color.White
            }
        });
        
        // Water
        Register(TileType.Water, new TileDefinition 
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
        
        // Grass
        Register(TileType.Grass, new TileDefinition 
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
        
        // Sand
        Register(TileType.Sand, new TileDefinition 
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
        
        // Iron
        Register(TileType.Iron, new TileDefinition 
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
        
        // Gold
        Register(TileType.Gold, new TileDefinition 
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
        
        // Coal
        Register(TileType.Coal, new TileDefinition 
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
        
        // Hard
        Register(TileType.Hard, new TileDefinition 
        { 
            Name = "Hard", 
            IsSolid = true,
            IsFlammable = false,
            Hardness = 100f,
            RenderInfo = new TileRenderDefinition
            {
                Type = RenderType.Shape,
                Shape = ShapeType.Rectangle,
                Color = new Color(100, 80, 60, 255)
            }
        });
    }

    public static void Register(TileType type, TileDefinition def)
    {
        _registry[type] = def;
    }

    public static TileDefinition GetDefinition(TileType type)
    {
        if (_registry.TryGetValue(type, out var def)) return def;
        return _defaultAirDef;
    }
}