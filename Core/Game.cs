using Raylib_cs; 

using System.Numerics;
using StellarSurvivors.Components;
using StellarSurvivors.Interfaces;
using StellarSurvivors.States;
using StellarSurvivors.Systems;
using StellarSurvivors.WorldGen;


namespace StellarSurvivors.Core
{
    public class Game
    {  
        private int _screenWidth;
        private int _screenHeight;
        
        private StateManager _stateManager;
        
        public EventManager EventManager { get; private set; }
        public EntityManager EntityManager { get; private set; }
        public GameData gameData {get; private set;}
        public List<IGenerator> Generators {get; private set;}

        
        public Game(int width, int height)
        {
            EventManager = new EventManager();
            _stateManager = new StateManager();
            EntityManager  = new EntityManager();
            gameData = new GameData();
            
            this._screenWidth = width;
            this._screenHeight = height;
            
            Generators = new List<IGenerator>();
        }
        
        public void Run()
        {
            // Initialization
            
            Raylib.InitWindow(_screenWidth, _screenHeight, "Game Engine");
            Raylib.SetWindowPosition(0, 0);
            Raylib.SetTargetFPS(60);
            
            _stateManager.PushState(new MainMenuState(this, _screenWidth, _screenHeight), this);

            // --- Game Loop ---
            while (!Raylib.WindowShouldClose()) // This checks for pressing the 'X' button
            {
                var dt =  Raylib.GetFrameTime(); 
                _stateManager.Update(this, dt);   
                
                // --- Draw (Rendering) ---
                Raylib.BeginDrawing();
                //Raylib.ClearBackground(Color.Black);  // moved to RenderSystem
                _stateManager.Draw(this);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        } // RUN
        
        // ---------- Factory Methods ----------- //
        public int CreatePlayer(Vector3 position, Vector2 size, Color color)
        {
            int entityId = EntityManager.CreateNewEntityId();

            // "Assemble" the player by adding components to the dictionaries
            EntityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
            EntityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Color = color, Layer = RenderLayer.Entities});
            EntityManager.Velocities.Add(entityId, new VelocityComponent { Velocity = Vector2.Zero });
            EntityManager.PlayerInputs.Add(entityId, new PlayerInputComponent { Speed = 5.0f });
            EntityManager.Healths.Add(entityId, new HealthComponent(100));
            EntityManager.Pods.Add(entityId, new PodComponent { rotationSpeed = 10f, thrustForce = 40f, thrustEnergyDrain = 5f});
            
            EntityManager.Players.Add(entityId);
            return entityId;    
        }
        
        public int CreateSpaceman(int podId, Vector3 position, Vector2 size, Color color)
        {
            // Get a new ID
            int entityId = EntityManager.CreateNewEntityId();

            // "Assemble" the player by adding components to the dictionaries
            EntityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
            EntityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Color = color, Layer = RenderLayer.Entities});
            EntityManager.Velocities.Add(entityId, new VelocityComponent { Velocity = Vector2.Zero });
            EntityManager.PlayerInputs.Add(entityId, new PlayerInputComponent { Speed = 5.0f });
            EntityManager.Healths.Add(entityId, new HealthComponent(100));
            Vector2 podPosition = new Vector2(position.X, position.Y);
            EntityManager.Spacemen.Add(entityId, new SpacemanComponent(podPosition, podId));
            
            EntityManager.Players.Add(entityId);
            return entityId;    
        }

        public int CreateMotherShip(Vector3 position, Vector2 size, Color color)
        {
            int entityId = EntityManager.CreateNewEntityId();
            
            EntityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
            EntityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Color = color, Layer = RenderLayer.Entities});
            EntityManager.MotherShips.Add(entityId, new MotherShipComponent(80, 100, 120, 80));
            return entityId;
        }
        

        public void RemoveEntity(int entityId)
        {
            // ... all your Remove() calls
        }
    }
}