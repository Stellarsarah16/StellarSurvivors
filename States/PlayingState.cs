using System.Numerics;
using System.Runtime.CompilerServices;
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

    private int _screenWidth;
    private int _screenHeight;
    
    private Game _world;
    
    private List<IUpdateSystem> _updateSystems;
    private RenderSystem _backgroundRenderer;
    private RenderSystem _entityRenderer;
    private UIManager _uiManager;
    private PlayerStateSystem _playerStateSystem;
    private InputSystem _inputSystem;
    private PhysicsSystem _physicsSystem;
    private MovementSystem _movementSystem;
    private CollisionSystem _collisionSystem;
    
    private WorldGenerator _worldGenerator;
    
    private Random _random = new Random();
    

    public PlayingState(Game world, int screenWidth, int screenHeight)
    {
        _world = world;
        _screenWidth  = screenWidth;
        _screenHeight = screenHeight;
        

         // added the Yoffset -600
        world.Camera = new Camera2D()
        {
            Offset = new Vector2(screenWidth / 2.0f, screenHeight / 2.0f),
            Target = Vector2.Zero,
            Rotation = 0.0f,
            Zoom = 1.0f
        };
        
        //Init Systems
        InitSystems();
    }

    public void InitSystems()
    {
        _uiManager = new UIManager(_world, _screenWidth, _screenHeight);
        
        // Input System goes first
        _inputSystem = new InputSystem(_world.EntityManager, _world.EventManager,
            _uiManager.getTopPanelBounds(),
            _uiManager.getSidebarBounds(), () => _uiManager.isSidebarOpen());        

        // Physics Based Systems
        _physicsSystem = new PhysicsSystem(_world.EntityManager, _world.WorldData);
        _movementSystem = new MovementSystem(_world.EntityManager);
        _collisionSystem = new CollisionSystem(_world.EntityManager, _world.EventManager, _world.WorldData);

        // Logical Systems
        _updateSystems = new List<IUpdateSystem>
        {
            new SpacemanControlSystem(_world.EventManager, _world.EntityManager),
            new PodControlSystem(_world.EventManager, _world.EntityManager),
            new HealthSystem(),
            new CleanupSystem(),
            new FuelSystem(),
            new TileInteractionSystem(_world.WorldData),
        };
        
        // Event only Systems
        _playerStateSystem = new PlayerStateSystem(_world.EventManager, _world.EntityManager, _world);

        // Rendering Systems
        _backgroundRenderer = new RenderSystem(RenderLayer.Background);
        _entityRenderer = new RenderSystem(RenderLayer.Entities);
    }
    
    public void Enter()
    {
        System.Console.WriteLine("Entering PlayingState");
        _worldGenerator = new WorldGenerator(_world);
        _world.Generators.Add(new BasicMiner(0.3));
    }

    public void Exit()
    {
        System.Console.WriteLine("Exiting PlayingState");
        // Cleanup Entities
        //_world.RemoveEntity(_playerId);
    }

    // --------------------- UPDATE ---------------------
    
    public void Update(StateManager stateManager, float dt)
    {
        _uiManager.Update();
        
        // 1. Update the camera target
        if (_world.EntityManager.Players.Count > 0)
        {
            int playerId = _world.EntityManager.Players.First();
            if (_world.EntityManager.Transforms.ContainsKey(playerId))
            {
                var playerTransform = _world.EntityManager.Transforms[playerId];
                _world.Camera.Target = new Vector2(playerTransform.Position.X, playerTransform.Position.Y);
                _world.Camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
            }
        }
        
        //Update Systems
        _inputSystem.Update(_world);
        _physicsSystem.Update(dt);
        _movementSystem.UpdateX(dt);
        _collisionSystem.UpdateX(dt);
        _movementSystem.UpdateY(dt);
        _collisionSystem.UpdateY(dt);
        
        foreach (var system in _updateSystems)
        {
            system.Update(_world, dt);
        }

        // Update Generators and UI Buttons
        foreach (var generator in _world.Generators)
        {
            generator.Update(dt, _world.GameData);
        }
    }

    // -------------------  DRAW --------------------
    public void Draw()
    {
        // 2. Begin 2D mode
        Raylib.BeginMode2D(_world.Camera);
        
        // 3. Clear background
        Raylib.ClearBackground(Color.Black);

        // 4. --- Draw Each layer ---
        _backgroundRenderer.Draw(_world, _world.Camera); // <-- Draws tiles FIRST
        _entityRenderer.Draw(_world, _world.Camera);     // <-- Draws player SECOND
        
        // In your RenderSystem, after drawing the pod
        int controlledEntityId = _world.EntityManager.PlayerInputs.Keys.First();
        
        // 5. End 2D mode
        Raylib.EndMode2D();
        
        _uiManager.Draw();
    }
}