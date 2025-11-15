namespace StellarSurvivors.Core;
using Raylib_cs; 
using System.Numerics;
using StellarSurvivors.Components;
using StellarSurvivors.Interfaces;
using StellarSurvivors.States;
using StellarSurvivors.Systems;
using StellarSurvivors.WorldGen;
using StellarSurvivors.Enums;

public class Game
{  
    private int _screenWidth;
    private int _screenHeight;
    public float DeltaTime =  0.0f;
    
    private StateManager _stateManager;
    public Camera2D Camera;
    
    public EventManager EventManager { get; private set; }
    public EntityManager EntityManager { get; private set; }
    public GameData GameData {get; private set;}

    public WorldData WorldData;
    public List<IGenerator> Generators {get; private set;}

    
    public Game(int width, int height)
    {
        EventManager = new EventManager();
        _stateManager = new StateManager();
        EntityManager  = new EntityManager();
        GameData = new GameData();
        WorldData = new WorldData(_screenWidth, _screenHeight, -600);
        
        
        this._screenWidth = width;
        this._screenHeight = height;
        
        Generators = new List<IGenerator>();
    }
    
    public void Run()
    {
        // Initialization
        Raylib.InitWindow(_screenWidth, _screenHeight, "Game Engine");
        Raylib.SetWindowPosition(0, 0);
        //Raylib.SetTargetFPS(60);
        EntityManager.InitializeGameAssets();
        WorldData.InitializeTileRegistry();
        
        _stateManager.PushState(new MainMenuState(this, _screenWidth, _screenHeight), this);

        // --- Game Loop ---
        while (!Raylib.WindowShouldClose()) // This checks for pressing the 'X' button
        {
            var dt =  Raylib.GetFrameTime();
            DeltaTime = dt;
            _stateManager.Update(this, dt);   
            
            // --- Draw (Rendering) ---
            Raylib.BeginDrawing();
            //Raylib.ClearBackground(Color.Black);  // moved to RenderSystem
            _stateManager.Draw(this);

            Raylib.EndDrawing();
        }
        
        Shutdown();
        Raylib.CloseWindow();
    } // RUN
    
    // ---------- Factory Methods ----------- //
    public int CreatePod(Vector3 position, Vector2 size, Color color)
    {
        int entityId = EntityManager.CreateNewEntityId();
        EntityManager.SetPodId(entityId);

        Texture2D podTexture = AssetManager.GetTexture("pod");
        // "Assemble" the player by adding components to the dictionaries
        EntityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
        EntityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, Type = RenderType.Texture, 
            Texture = podTexture, SourceRect = new Rectangle(0, 0, podTexture.Width, podTexture.Height), Tint = Color.White });
        EntityManager.Velocities.Add(entityId, new VelocityComponent { Velocity = Vector2.Zero , TerminalVelocity = 100f, MaxVelocity = 300f});
        EntityManager.PlayerInputs.Add(entityId, new PlayerInputComponent { Speed = 5.0f });
        EntityManager.Healths.Add(entityId, new HealthComponent(100));
        EntityManager.Colliders.Add(entityId, new ColliderComponent { Radius = 48f });
        EntityManager.Pods.Add(entityId, new PodComponent { rotationSpeed = 10f, thrustForce = 100f, thrustEnergyDrain = 0.5f});
        EntityManager.Fuels.Add(entityId, new FuelComponent { CurrentFuel = 100f, RechargeRate = 2f, FuelCapacity = 50f});
        EntityManager.Inventories.Add(entityId, new InventoryComponent(50,5));
        EntityManager.AddEntityToChunk(entityId, position);
        EntityManager.SetControlledEntity(entityId);
        EntityManager.SetPodId(entityId);
        return entityId;    
    }
    
    public int CreateSpaceman(int podId,Vector3 position, Vector2 size, Color color)
    {
        // Get a new ID
        int entityId = EntityManager.CreateNewEntityId();
        EntityManager.SetSpacemanId(entityId);

        Texture2D spacemanTexture = AssetManager.GetTexture("spaceman");
        
        // "Assemble" the player by adding components to the dictionaries
        EntityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
        EntityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, 
            Type = RenderType.Texture, Texture = spacemanTexture, Tint = Color.White, SourceRect = new Rectangle(0,0, spacemanTexture.Width,spacemanTexture.Height)});
        EntityManager.Velocities.Add(entityId, new VelocityComponent { Velocity = Vector2.Zero, TerminalVelocity = 150f, MaxVelocity = 200});
        EntityManager.PlayerInputs.Add(entityId, new PlayerInputComponent { Speed = 5.0f });
        EntityManager.Healths.Add(entityId, new HealthComponent(100));
        EntityManager.Colliders.Add(entityId, new ColliderComponent { Radius = 26f });
        Vector2 podPosition = new Vector2(position.X, position.Y);
        EntityManager.Fuels.Add(entityId, new FuelComponent { CurrentFuel = 20f, RechargeRate = 0f, FuelCapacity = 50f});
        EntityManager.Spacemen.Add(entityId, new SpacemanComponent(300, 120, 150, 0.5f, podPosition, podId));
        EntityManager.Tools.Add(entityId, new ToolComponent());
        EntityManager.Inventories.Add(entityId, new InventoryComponent(10, 2));
        EntityManager.AddEntityToChunk(entityId, position);
        EntityManager.SetControlledEntity(entityId);
        EntityManager.SetSpacemanId(entityId);
        return entityId;    
    }

    public int CreateMotherShip(Vector3 position, Vector2 size, Color color)
    {
        int entityId = EntityManager.CreateNewEntityId();
        EntityManager.SetMothershipId(entityId);
        Texture2D mothershipTexture = AssetManager.GetTexture("mothership");
        
        EntityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
        EntityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, 
            Type = RenderType.Texture, Texture = mothershipTexture, Tint = Color.White, SourceRect = new Rectangle(0,0, mothershipTexture.Width,mothershipTexture.Height)});
        EntityManager.MotherShips.Add(entityId, new MotherShipComponent(80, 100, 120, 80));
        EntityManager.Inventories.Add(entityId, new InventoryComponent(200, 50));
        EntityManager.Fuels.Add(entityId, new FuelComponent(20, 10, 1000));
        EntityManager.AddEntityToChunk(entityId, position);
        return entityId;
    }
    
    public int CreateResourceEntity(Vector2 position, ResourceType type, int quantity, Color color)
    {
        int entityId = EntityManager.CreateNewEntityId();
        Vector3 pos = new Vector3(position.X, position.Y, 0);
    
        EntityManager.Transforms[entityId] = new TransformComponent { Position = pos, Scale = new Vector3(1, 1, 1) };
        EntityManager.Resources[entityId] = new ResourceComponent { Type = type, Quantity = quantity };
        EntityManager.Colliders[entityId] = new ColliderComponent { Radius = 4f };
         EntityManager.Renderables[entityId] = new RenderComponent 
        { 
            Color = color,
            Size = new Vector2(8, 8),
            Type = RenderType.Shape,
            SourceRect = new Rectangle(0, 0, 8, 8),
            Layer = RenderLayer.Entities
        };
        // --- END FIX ---
        
        return entityId;
    }
    
    // ---------  Helpers  ----------
    
    public void Shutdown()
    {
        AssetManager.UnloadAllTextures();
    }
    
}
