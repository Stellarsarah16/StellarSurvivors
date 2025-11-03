using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.Entities;
using StellarSurvivors.Systems;
using StellarSurvivors.WorldGen;
using StellarSurvivors.WorldGen.Strategies;
using StellarSurvivors.WorldGen.Steps;
using StellarSurvivors.WorldGen.Blueprints;
using StellarSurvivors.UI;

namespace StellarSurvivors.States;

public class PlayingState : IGameState
{
    private WorldGenerator _worldGenerator;
    private EntityFactory _entityFactory;
    private float _tileSize = 16.0f;
    private int _screenWidth;
    private int _screenHeight;
    private Camera2D _camera;
    private Game _world;
    
    private Dictionary<int, string> _biomeMap;
    
    private List<IUpdateSystem> _updateSystems;
    private RenderSystem _backgroundRenderer;
    private RenderSystem _entityRenderer;
    
    private PlayerStateSystem _playerControlSystem;

    private int _playerId;
    
    private Random _random = new Random();
    private const double GOLD_CHANCE = 0.01;
    
    // UI
    private bool _isSidebarOpen = true;
    private const int _sidebarWidth = 200;
    private const int _topPanelHeight = 60;
    
    private Button _buyMinerButton;
    private Rectangle _topPanelBounds;
    private Rectangle _sidebarBounds;
    private Rectangle _mainContentBounds;
    private Boolean _mouseOverUI;
    
    

    public PlayingState(Game world, int screenWidth, int screenHeight)
    {
        _world = world;
        _screenWidth  = screenWidth;
        _screenHeight = screenHeight;
        
        _camera = new Camera2D()
        {
            Offset = new Vector2(screenWidth / 2.0f, screenHeight / 2.0f),
            Target = Vector2.Zero,
            Rotation = 0.0f,
            Zoom = 1.0f
        };
        
        _updateSystems = new List<IUpdateSystem>
        {
            new InputSystem(world.EntityManager, world.EventManager, _topPanelBounds, _sidebarBounds, () => _isSidebarOpen),
            new MovementSystem(_world.EventManager),
            new PodControlSystem(_world.EventManager, _world.EntityManager),
            new PhysicsSystem(_world.EntityManager),
            new HealthSystem(),
            new CleanupSystem()
        };
        
        _playerControlSystem = new PlayerStateSystem(_world.EventManager, _world.EntityManager, _world);
            
        _backgroundRenderer = new RenderSystem(RenderLayer.Background);
        _entityRenderer = new RenderSystem(RenderLayer.Entities);
    }

    public void Enter()
    {
        //Subscrive to Events
        _world.EventManager.Subscribe<ToggleSidebarEvent>(OnToggleSidebar);
        _world.EventManager.Subscribe<ManualGoldClickEvent>(OnManualGoldClick);
        
        System.Console.WriteLine("Entering PlayingState");
        
        // Create Player
        _playerId = _world.CreatePlayer(
            new Vector3(400, 200, 0),
            new Vector2(24, 24),
            Color.DarkGreen
        );

        InitMap();
        
        InitUi();
        
        _world.Generators.Add(new BasicMiner(0.3));
    }

    public void Exit()
    {
        System.Console.WriteLine("Exiting PlayingState");
        _world.RemoveEntity(_playerId);
    }

    // --------------------- UPDATE ---------------------
    
    public void Update(StateManager stateManager, float dt)
    {
        // Mouse Controls
        Vector2 mousePosition = Raylib.GetMousePosition();
        _mouseOverUI = Raylib.CheckCollisionPointRec(mousePosition, _topPanelBounds) ||
                           (_isSidebarOpen && Raylib.CheckCollisionPointRec(mousePosition, _sidebarBounds));

        //Update Systems
        foreach (var system in _updateSystems)
        {
            system.Update(_world, dt);
        }
        

        // Update Generators and UI Buttons
        foreach (var generator in _world.Generators)
        {
            generator.Update(dt, _world.gameData);
        }
        if (_isSidebarOpen)
        {
            _buyMinerButton.Update(mousePosition);
        }
    }

    // -------------------  DRAW --------------------
    public void Draw()
    {
        Vector2 playerCoords;
        // 1. Update the camera target (we do this once)
        if (_world.EntityManager.Players.Count > 0)
        {
            int playerId = _world.EntityManager.Players.First();
            if (_world.EntityManager.Transforms.ContainsKey(playerId))
            {
                var playerTransform = _world.EntityManager.Transforms[playerId];
                _camera.Target = new Vector2(playerTransform.Position.X, playerTransform.Position.Y);
                playerCoords = _camera.Target;
            }
        }

        // 2. Begin 2D mode
        Raylib.BeginMode2D(_camera);
        
        // 3. Clear background
        Raylib.ClearBackground(Color.Black);

        // 4. --- Draw Each layer ---
        _backgroundRenderer.Draw(_world, _camera); // <-- Draws tiles FIRST
        _entityRenderer.Draw(_world, _camera);     // <-- Draws player SECOND
        
        // 5. End 2D mode
        Raylib.EndMode2D();
        
        // A. Draw Panels
        Raylib.DrawRectangleRec(_topPanelBounds, Raylib.ColorAlpha(Color.Black, 0.7f));
        Raylib.DrawRectangleLinesEx(_topPanelBounds, 1, Color.DarkGray);

        if (_isSidebarOpen)
        {
            Raylib.DrawRectangleRec(_sidebarBounds, Raylib.ColorAlpha(Color.LightGray, 0.7f));
            Raylib.DrawRectangleLinesEx(_sidebarBounds, 1, Color.DarkGray);
        }

        // B. Draw Buttons
        if (_isSidebarOpen)
        {
            _buyMinerButton.Draw();
        }

        // 6. Draw UI (outside camera)
        string goldText = $"Gold: {_world.gameData.CurrentGold:F1}"; // "F1" = 1 decimal place
        Raylib.DrawText(goldText, _screenWidth - 120, 10, 20, Color.Red);
        
        Raylib.DrawText("Health: 100", _screenWidth - 120, 40, 20, Color.White);
        Raylib.DrawFPS(10, 40);
        
        if (_world.EntityManager.Pods.Count > 0)
        {
            int playerId = _world.EntityManager.Players.First();
            if (_world.EntityManager.Transforms.ContainsKey(playerId))
            {
                var playerTransform = _world.EntityManager.Transforms[playerId];
                playerCoords = new Vector2(playerTransform.Position.X, playerTransform.Position.Y);
                string coordsText = $"x: {playerCoords.X:F0}, y: {playerCoords.Y:F0}";
                Raylib.DrawText(coordsText, 10, 10, 20, Color.White);
            }
        }


    }

    private void InitMap()
    {// Create Map
        _entityFactory = new EntityFactory(_world);
        _entityFactory.Register("grassTile", new GrassTileBlueprint());
        _entityFactory.Register("waterTile", new WaterTileBlueprint());
        _entityFactory.Register("dirtTile", new DirtTileBlueprint());
        _entityFactory.Register("stoneTile", new StoneTileBlueprint());
        _entityFactory.Register("sandTile", new SandTileBlueprint());
        _entityFactory.Register("goldTile", new GoldTileBlueprint());
        _entityFactory.Register("ironTile", new IronTileBlueprint());

        _worldGenerator = new WorldGenerator();
        int seed = _random.Next();
        _worldGenerator.AddStep(new SurfaceGenerationStep(new PerlinNoiseStrategy()));
        _worldGenerator.AddStep(new BaseTerrainStep());
        _worldGenerator.AddStep(new CaveGenerationStep());
        _worldGenerator.AddStep(new MineralGenerationStep(_random, 0.01, 0.003));
        
        int mapWidth = 200;
        int mapHeight = 200;
        WorldData map = _worldGenerator.Generate(mapWidth, mapHeight, seed);
        
        // Tile Creation Loop
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                // Use a clearer variable name
                int tileId = map.TileMap[x, y]; 
            
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
                        blueprintId = "goldTile"; // Matches your new blueprint
                        break;
                    case TileIDs.TILE_IRON_ORE:
                        blueprintId = "ironTile"; // Matches your new blueprint
                        break;
                
                    // --- CRITICAL ---
                    // By default (which includes TILE_AIR), we do nothing.
                    case TileIDs.TILE_AIR:
                    default:
                        break; // blueprintId remains null
                }

                // If a blueprint was found (i.e., it's not TILE_AIR), create it.
                if (blueprintId != null)
                {
                    int yOffset = -600;
                    BlueprintSettings settings = new BlueprintSettings
                    {
                        Position = new Vector3(x * _tileSize, (y * _tileSize) + yOffset, 0),
                        Size = new Vector2(_tileSize, _tileSize),
                        Scale = Vector3.One
                    };
                    System.Console.WriteLine(blueprintId);
                    // We don't need _random or GOLD_CHANCE here anymore!
                    _entityFactory.CreateEntity(blueprintId, settings);
                }
            }
        }
    }
    
    private void InitUi()
    {
        _topPanelBounds = new Rectangle(0, 0, _screenWidth, _topPanelHeight);
        _sidebarBounds = new Rectangle(
            _screenWidth - _sidebarWidth, 
            _topPanelHeight, 
            _sidebarWidth, 
            _screenHeight - _topPanelHeight
        );
        _mainContentBounds = new Rectangle(
            0, 
            _topPanelHeight, 
            _screenWidth - (_isSidebarOpen ? _sidebarWidth : 0), 
            _screenHeight - _topPanelHeight
        );
        
        // --- UI Element Creation ---
            
        // 1. Toggle Button for Sidebar
        // Note the Action: () => { ... } is a "lambda function".
        // We pass a function directly into the button's constructor.
        Action toggleSidebarAction = () => 
        { 
            _isSidebarOpen = !_isSidebarOpen; 
                
            // Update main content bounds when sidebar toggles
            _mainContentBounds.Width = _screenWidth - (_isSidebarOpen ? _sidebarWidth : 0);
        };

        // 2. Buy Miner Button (inside the sidebar)
        Action buyMinerAction = () =>
        {
            // When clicked, add a new generator to our list
            _world.Generators.Add(new BasicMiner(0.5));
        };
        _buyMinerButton = new Button(
            new Rectangle(_screenWidth - 190, _topPanelHeight + 10, 180, 30),
            "Buy Miner (0.5/s)",
            buyMinerAction
        );
    }
    
    public void OnToggleSidebar(ToggleSidebarEvent e){
        _isSidebarOpen = !_isSidebarOpen; 
        _mainContentBounds.Width = _screenWidth - (_isSidebarOpen ? _sidebarWidth : 0);
    }

    public void OnManualGoldClick(ManualGoldClickEvent e)
    {
        if (!_mouseOverUI)
        {
            _world.gameData.CurrentGold += 1;
        }
    }
}