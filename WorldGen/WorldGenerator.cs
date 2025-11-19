namespace StellarSurvivors.WorldGen;
using StellarSurvivors.Data;
using StellarSurvivors.Core;
using System.Collections.Generic;
using StellarSurvivors.WorldGen.Blueprints;
using StellarSurvivors.WorldGen.Steps;
using StellarSurvivors.WorldGen.Strategies;
using System.Numerics;
using Raylib_cs;

public class WorldGenerator
{
    private List<IGenerationStep> _steps = new List<IGenerationStep>();
    private float _tileSize = GameConstants.TILE_SIZE;
    public EntityFactory EntityFactory;
    
    private Random _random = new Random();
    private const double GOLD_CHANCE = 0.01;
    
    private Game _world;

    private RoomLibrary _roomLibrary;

    public WorldGenerator(Game world)
    {
        _world = world;
        _roomLibrary = new  RoomLibrary();
        
        int seed = _random.Next();
        
        EntityFactory = new EntityFactory(world.EntityManager);
        EntityFactory.Register("grassTile", new GrassTileBlueprint());
        EntityFactory.Register("waterTile", new WaterTileBlueprint());
        EntityFactory.Register("dirtTile", new DirtTileBlueprint());
        EntityFactory.Register("stoneTile", new StoneTileBlueprint());
        EntityFactory.Register("sandTile", new SandTileBlueprint());
        EntityFactory.Register("goldTile", new GoldTileBlueprint());
        EntityFactory.Register("ironTile", new IronTileBlueprint());
        EntityFactory.Register("coalTile", new CoalTileBlueprint());
        EntityFactory.Register("hardTile", new HardTileBlueprint());
        

        AddStep(new SurfaceGenerationStep(new PerlinNoiseStrategy()));
        AddStep(new BaseTerrainStep());
        AddStep(new CaveGenerationStep());
        AddStep(new VaultGenerationStep(_random, _roomLibrary, 30));
        AddStep(new MineralGenerationStep(_random, 0.005, 0.001, 0.01));
        
        
        int mapWidth = 1000;
        int mapHeight = 400;
        world.WorldData = Generate(world.WorldData, mapWidth, mapHeight, seed);
        
        // Tile Creation Loop
        for (int y = 0; y < world.WorldData.Height; y++)
        {
            for (int x = 0; x < world.WorldData.Width; x++)
            {
                int tileId = world.WorldData.TileMap[x, y];

                string blueprintId = null; // Default to null (do nothing)

                // Map the ID from the TileMap directly to a blueprint string
                switch (tileId)
                {
                    case TileIDs.TILE_GRASS:
                        blueprintId = "grassTile";
                        break;
                    case TileIDs.TILE_DIRT:
                        blueprintId = "dirtTile";
                        break;
                    case TileIDs.TILE_STONE:
                        blueprintId = "stoneTile";
                        break;
                    case TileIDs.TILE_WATER:
                        blueprintId = "waterTile";
                        break;
                    case TileIDs.TILE_SAND:
                        blueprintId = "sandTile";
                        break;
                    case TileIDs.TILE_GOLD_ORE:
                        blueprintId = "goldTile"; 
                        break;
                    case TileIDs.TILE_IRON_ORE:
                        blueprintId = "ironTile"; 
                        break;
                    case TileIDs.TILE_COAL:
                        blueprintId = "coalTile"; 
                        break;
                    case TileIDs.TILE_HARD:
                        blueprintId = "hardTile"; 
                        break;
                    // For Air do nothing
                    case TileIDs.TILE_AIR:
                    default:
                        break; // blueprintId remains null
                }

                // If a blueprint was found (i.e., it's not TILE_AIR), create it.
                if (blueprintId != null)
                {
                    Vector3 entityPosition = new Vector3(x + _tileSize, (y + _tileSize) + world.WorldData.Yoffset, 0);
                    BlueprintSettings settings = new BlueprintSettings
                    {
                        Position = entityPosition,
                        Size = new Vector2(_tileSize, _tileSize),
                        Scale = Vector3.One
                    };
                    // We don't need _random or GOLD_CHANCE here anymore!
                    int entityId = EntityFactory.CreateMapEntity(blueprintId, settings);
                }
            }

        }
        Vector3 motherShipPosition = PlaceMotherShip(world.WorldData);
        Vector3 positionAbove = new Vector3(motherShipPosition.X, motherShipPosition.Y - 50, motherShipPosition.Z);
        // Create Player
        Console.WriteLine(positionAbove);
        var podId = EntityFactory.CreatePod(
            positionAbove,
            new Vector2(144, 144),
            Color.Pink
        );
        world.EntityManager.SetPodId(podId);
        
    }
    
    public void AddStep(IGenerationStep step)
    {
        _steps.Add(step);
    }

    public WorldData Generate(WorldData worldData, int width, int height, int seed)
    {
        worldData.Width = width;
        worldData.Height = height;
        worldData.TileMap = new int[width, height];
        worldData.SurfaceHeightMap = new int[width];
        
        foreach (var step in _steps) { step.Process(worldData, seed); }
    
        return worldData;
    }
    
    private Vector3 PlaceMotherShip(WorldData worldData)
    {
        // 1. Define ship properties
        Vector2 shipSize = new Vector2(480, 240);
        int shipWidthInTiles = (int)Math.Ceiling(shipSize.X / _tileSize);

        // 2. Set default spawn position (fallback if no spot is found)
        // Assumes 'worldData' is your instance of the WorldData class
        float defaultX = (worldData.Width * _tileSize) / 2f; 
        float defaultY = worldData.Yoffset - (shipSize.Y / 2f); // yOffset from your code
        Vector3 spawnPosition = new Vector3(defaultX, defaultY, 0);

        bool spotFound = false;

        // 3. Scan for a flat landing spot
        for (int y = 1; y < worldData.Height; y++)
        {
            int startX = worldData.Width / 3;
            int endX = (worldData.Width * 2) / 3;

            for (int x = startX; x < endX; x++)
            {
                if (IsFlatSpot(worldData, x, y, shipWidthInTiles))
                {
                    // We found a spot!
                    float spotCenterX = (x * _tileSize) + (shipWidthInTiles * _tileSize) / 2f;
                    float surfaceWorldY = (y * _tileSize) + worldData.Yoffset;
                    float spawnWorldY = surfaceWorldY - (shipSize.Y / 2f);

                    spawnPosition = new Vector3(spotCenterX, spawnWorldY + GameConstants.TILE_SIZE + 10, 0);

                    spotFound = true;
                    break; 
                }
            }

            if (spotFound) { break; }
        }

        // 4. Create the mothership
        var mothershipId = EntityFactory.CreateMotherShip(
            spawnPosition,
            shipSize,
            Color.Purple
        );
        _world.EntityManager.SetMothershipId(mothershipId);

        return spawnPosition;
    }

    private bool IsFlatSpot(WorldData worldData, int startX, int startY, int widthInTiles)
    {
        for (int i = 0; i < (widthInTiles / 2); i++)  // subtracted 4 - temp fix
        {
            int currentX = startX + i;

            // Make sure ship is within world boundaries
            if (currentX >= worldData.Width || startY >= worldData.Height || startY < 1)
            {
                return false;
            }

            // --- Get the tile types from worldData.TileMap ---
            int groundTile = worldData.TileMap[currentX, startY];
            int skyTile = worldData.TileMap[currentX, startY - 1]; // Check tile above

            // --- Check the logic ---
            bool isGround = (groundTile == TileIDs.TILE_GRASS || groundTile == TileIDs.TILE_DIRT);
            bool isSky = (skyTile == TileIDs.TILE_AIR);

            if (!isGround || !isSky)
            {
                return false;
            }
        }

        // If the loop finished, the spot is flat!
        return true;
    }
}
