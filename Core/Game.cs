namespace StellarSurvivors.Core;
using Raylib_cs; 
using System.Numerics;
using StellarSurvivors.Components;
using StellarSurvivors.Interfaces;
using StellarSurvivors.States;
using StellarSurvivors.Systems;
using StellarSurvivors.WorldGen;
using StellarSurvivors.Enums;
using StellarSurvivors.WorldGen.TileData;

public class Game
{  
    private int _screenWidth;
    private int _screenHeight;
    public float DeltaTime =  0.0f;
    
    private StateManager _stateManager;
    
    public CameraSystem CameraSystem;
    public EventManager EventManager { get; private set; }
    public EntityManager EntityManager { get; private set; }
    public WorldGenerator WorldGenerator;

    public GameData GameData {get; private set;}

    public WorldData WorldData;
    public List<IGenerator> Generators {get; private set;}

    
    public Game(int width, int height)
    {
        this._screenWidth = width;
        this._screenHeight = height;
        EventManager = new EventManager();
        _stateManager = new StateManager();
        EntityManager  = new EntityManager();
        GameData = new GameData();
        WorldData = new WorldData(_screenWidth, _screenHeight, -600);
        CameraSystem = new CameraSystem(EntityManager, _screenWidth, _screenHeight);
    }

    public void Initialize()
    {
        Raylib.InitWindow(_screenWidth, _screenHeight, "Game Engine");
        Raylib.SetWindowPosition(0, 0);
        Raylib.InitAudioDevice();
        //Raylib.SetTargetFPS(60);
        EntityManager.InitializeGameAssets();
        WorldGenerator = new WorldGenerator(this);
        TileRegistry.Initialize();
        AnimationManager.InitializeAnimations();
    }
    
    public void Run()
    {
        // Initialization
        Initialize();
        _stateManager.PushState(new MainMenuState(this, _screenWidth, _screenHeight), this);

        // --- Game Loop ---
        while (!Raylib.WindowShouldClose()) // This checks for pressing the 'X' button
        {
            var dt =  Raylib.GetFrameTime();
            DeltaTime = dt;
            _stateManager.Update(this, dt);   
            CameraSystem.Update(dt);
            // --- Draw (Rendering) ---
            Raylib.BeginDrawing();
            //Raylib.ClearBackground(Color.Black);  // moved to RenderSystem
            _stateManager.Draw(this);

            Raylib.EndDrawing();
        }
        
        Shutdown();
        Raylib.CloseWindow();
    } // RUN
    
    // ---------  Helpers  ----------
    
    public void Shutdown()
    {
        Raylib.CloseAudioDevice();
        AssetManager.UnloadAllAssets();
    }
    
}
