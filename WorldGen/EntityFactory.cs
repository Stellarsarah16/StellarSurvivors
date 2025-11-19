namespace StellarSurvivors.WorldGen;
using System.Collections.Generic;
using System.Numerics;
using StellarSurvivors.Core;
using StellarSurvivors.WorldGen.Blueprints; // For the interface
using Raylib_cs;
using StellarSurvivors.Components;
using StellarSurvivors.Enums;

public class EntityFactory
{

    private Dictionary<string, IEntityBlueprint> _blueprints;
    private EntityManager _entityManager;

    public EntityFactory(EntityManager entityManager)
    {
        _entityManager = entityManager;
        _blueprints = new Dictionary<string, IEntityBlueprint>();
    }
    
    public void Register(string blueprintId, IEntityBlueprint blueprint)
    {
        _blueprints[blueprintId] = blueprint;
        // Or: _blueprints.Add(blueprintId, blueprint);
    }
    
    public int CreateMapEntity(string blueprintId, BlueprintSettings settings)
    {
        if (!_blueprints.ContainsKey(blueprintId))
        {
            System.Console.WriteLine($"Error: No blueprint registered for ID '{blueprintId}'");
            return -1; // Return an invalid ID
        }

        IEntityBlueprint blueprint = _blueprints[blueprintId];
        int newEntityId = _entityManager.CreateNewEntityId();
        blueprint.Apply(_entityManager, newEntityId, settings);

        return newEntityId;
    }
    public int CreatePod(Vector3 position, Vector2 size, Color color)
    {
        int entityId = _entityManager.CreateNewEntityId();
        _entityManager.SetPodId(entityId);

        Texture2D podTexture = AssetManager.GetTexture("pod");
        // "Assemble" the player by adding components to the dictionaries
        _entityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
        _entityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, Type = RenderType.Texture, 
            Texture = podTexture, SourceRect = new Rectangle(0, 0, podTexture.Width, podTexture.Height), Tint = Color.White });
        _entityManager.Velocities.Add(entityId, new VelocityComponent { Velocity = Vector2.Zero , TerminalVelocity = 100f, MaxVelocity = 300f});
        _entityManager.PlayerInputs.Add(entityId, new PlayerInputComponent (5.0f ));
        _entityManager.Healths.Add(entityId, new HealthComponent(100));
        _entityManager.Colliders.Add(entityId, new ColliderComponent { Radius = 48f });
        _entityManager.Pods.Add(entityId, new PodComponent { rotationSpeed = 10f, thrustForce = 100f, thrustEnergyDrain = 0.8f});
        _entityManager.Fuels.Add(entityId, new FuelComponent (20f, 2f,50f));
        _entityManager.Inventories.Add(entityId, new InventoryComponent(16,3));
        _entityManager.CameraFocus.Add(entityId, new CameraFocusComponent {TargetZoom = 1f});
        _entityManager.AddEntityToChunk(entityId, position);
        _entityManager.SetControlledEntity(entityId);
        _entityManager.SetPodId(entityId);
        return entityId;    
    }
    
    public int CreateSpaceman(int podId,Vector3 position, Vector2 size, Color color)
    {
        // Get a new ID
        int entityId = _entityManager.CreateNewEntityId();
        _entityManager.SetSpacemanId(entityId);

        Texture2D playerSheet = AssetManager.GetTexture("spaceman_sheet");
        var defaultAnim = AnimationManager.GetDefinition("spaceman_idle");
        
        // "Assemble" the player by adding components to the dictionaries
        _entityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
        _entityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, 
            Type = RenderType.Texture, Texture = playerSheet, Tint = Color.White, 
            SourceRect = defaultAnim.Frames[0]
        });
        _entityManager.Velocities.Add(entityId, new VelocityComponent { Velocity = Vector2.Zero, TerminalVelocity = 150f, MaxVelocity = 200});
        _entityManager.PlayerInputs.Add(entityId, new PlayerInputComponent( 5.0f ));
        _entityManager.Healths.Add(entityId, new HealthComponent(100));
        _entityManager.Colliders.Add(entityId, new ColliderComponent { Radius = 26f });
        Vector2 podPosition = new Vector2(position.X, position.Y);
        _entityManager.Fuels.Add(entityId, new FuelComponent( 20f,  0f, 50f));
        _entityManager.Spacemen.Add(entityId, new SpacemanComponent(300, 120, 150, 0.5f, podPosition, podId));
        _entityManager.Tools.Add(entityId, new ToolComponent());
        _entityManager.CameraFocus.Add(entityId, new CameraFocusComponent { TargetZoom = 2.5f });
        _entityManager.Inventories.Add(entityId, new InventoryComponent(8, 2));
        var anim = new AnimationComponent();
        _entityManager.Animations.Add(entityId, anim);
        anim.Play("spaceman_idle");
        
        
        _entityManager.AddEntityToChunk(entityId, position);
        _entityManager.SetControlledEntity(entityId);
        _entityManager.SetSpacemanId(entityId);
        return entityId;    
    }

    public int CreateMotherShip(Vector3 position, Vector2 size, Color color)
    {
        int entityId = _entityManager.CreateNewEntityId();
        _entityManager.SetMothershipId(entityId);
        Texture2D mothershipTexture = AssetManager.GetTexture("mothership");
        
        _entityManager.Transforms.Add(entityId, new TransformComponent { Position = position, Rotation = 0.0f, Scale = new Vector3(1,1,1)});
        _entityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, 
            Type = RenderType.Texture, Texture = mothershipTexture, Tint = Color.White, SourceRect = new Rectangle(0,0, mothershipTexture.Width,mothershipTexture.Height)});
        _entityManager.MotherShips.Add(entityId, new MotherShipComponent(2, 80, 100, 120, 80));
        _entityManager.Inventories.Add(entityId, new InventoryComponent(200, 50));
        _entityManager.Fuels.Add(entityId, new FuelComponent(10, 10, 1000));
        _entityManager.AddEntityToChunk(entityId, position);
        return entityId;
    }
    
    public int CreateMinerEntity(Vector2 position, ResourceType type, int quantity, Color color)
    {
        int entityId = _entityManager.CreateNewEntityId();
        Vector3 pos = new Vector3(position.X, position.Y, 0);
        Texture2D minerTexture = AssetManager.GetTexture("powerup");
        Vector2 size = new Vector2(minerTexture.Width, minerTexture.Height);
        _entityManager.Transforms[entityId] = new TransformComponent { Position = pos, Scale = new Vector3(1, 1, 1) };
        _entityManager.Resources[entityId] = new ResourceComponent { Type = type, Quantity = quantity };
        _entityManager.Colliders[entityId] = new ColliderComponent { Radius = 4f };
        _entityManager.Generators[entityId] = new GeneratorComponent(0.01, type);
        _entityManager.Renderables.Add(entityId, new RenderComponent { Size = size, Layer = RenderLayer.Entities, 
            Type = RenderType.Texture, Texture = minerTexture, Tint = Color.White, SourceRect = new Rectangle(0,0, minerTexture.Width,minerTexture.Height)});
        // --- END FIX ---
        
        return entityId;
    }
    
}

