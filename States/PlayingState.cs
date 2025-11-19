using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.Gameplay.Generators;
using StellarSurvivors.Gameplay.Tools;
using StellarSurvivors.Systems;
using StellarSurvivors.WorldGen;
using StellarSurvivors.WorldGen.Strategies;
using StellarSurvivors.WorldGen.Steps;
using StellarSurvivors.WorldGen.Blueprints;
using StellarSurvivors.UI;
using StellarSurvivors.Enums;

namespace StellarSurvivors.States;

public class PlayingState : IGameState
{

    private int _screenWidth;
    private int _screenHeight;
    private Random _random = new Random();
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
    private ResourceSystem _resourceSystem;
    private MothershipInteractionSystem _mothershipInteractionSystem;
    
    public PlayingState(Game world, int screenWidth, int screenHeight)
    {
        _world = world;
        _screenWidth  = screenWidth;
        _screenHeight = screenHeight;
        
        //Init Systems
        _uiManager = new UIManager(_world, _screenWidth, _screenHeight);
        InitSystems();
    }

    public void InitSystems()
    {
        // Input System goes first
        _inputSystem = new InputSystem(_world.EntityManager, _world.EventManager,
            _uiManager.getTopPanelBounds(),
            _uiManager.getSidebarBounds(), () => _uiManager.isSidebarOpen());        

        // Physics Based Systems
        _physicsSystem = new PhysicsSystem(_world.EntityManager, _world.WorldData);
        _movementSystem = new MovementSystem(_world.EntityManager);
        _collisionSystem = new CollisionSystem(_world.EntityManager, _world.EventManager, _world.WorldData);
        _resourceSystem = new ResourceSystem(_world);
        _mothershipInteractionSystem = new MothershipInteractionSystem(_world);
        
        // Logical Systems
        _updateSystems = new List<IUpdateSystem>
        {
            new SpacemanControlSystem(_world),
            new PodControlSystem(_world.EventManager, _world.EntityManager),
            new HealthSystem(),
            //new FuelSystem(),
            new AnimationSystem(_world.EntityManager),
            new CleanupSystem(),
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
        //_world.Generators.Add(new BasicMiner(0.3));
    }

    // --------------------- UPDATE ---------------------
    
    public void Update(StateManager stateManager, float dt)
    {
        _uiManager.Update();
        
        // 1. Update the camera target
        if (_world.EntityManager.ControlledEntityId != -1)
        {
            int playerId = _world.EntityManager.ControlledEntityId;
            if (_world.EntityManager.Transforms.ContainsKey(playerId))
            {
                var playerTransform = _world.EntityManager.Transforms[playerId];
                //_world.CameraSystem.Camera.Target = new Vector2(playerTransform.Position.X, playerTransform.Position.Y);
                //_world.CameraSystem.Camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
            }
        }
        
        //Update Systems
        _inputSystem.Update(_world);
        _physicsSystem.Update(dt);
        _movementSystem.UpdateX(dt);
        _collisionSystem.UpdateX(dt);
        _movementSystem.UpdateY(dt);
        _collisionSystem.UpdateY(dt);
        _collisionSystem.UpdateEntityCollisions();
        
        foreach (var system in _updateSystems) { system.Update(_world, dt); }
        
        _world.WorldData.Interaction.UpdateFlashTimer(dt);
    }

    // -------------------  DRAW --------------------
    public void Draw()
    {
        Raylib.BeginMode2D(_world.CameraSystem.Camera);
        Raylib.ClearBackground(Color.Black);

        // 4. --- Draw Each layer ---
        _backgroundRenderer.Draw(_world, _world.CameraSystem.Camera); // <-- Draws tiles FIRST
        _entityRenderer.Draw(_world, _world.CameraSystem.Camera);     // <-- Draws player SECOND
        
        Raylib.EndMode2D();
        
        _uiManager.Draw();
    }

    public void Exit()
    {    
    }
}