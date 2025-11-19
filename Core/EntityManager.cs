namespace StellarSurvivors.Core;
using StellarSurvivors.Components;
using System.Numerics;
using StellarSurvivors.Enums;

public class EntityManager
{
    
    public struct ChunkCoord
    {
        public int X;
        public int Y;
        public ChunkCoord(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    
    private int _nextEntityId = 0;
    
    public Dictionary<int, TransformComponent> Transforms;
    public Dictionary<int, VelocityComponent> Velocities;
    public Dictionary<int, PlayerInputComponent> PlayerInputs;
    public Dictionary<int, RenderComponent> Renderables;
    public Dictionary<int, HealthComponent> Healths;
    public Dictionary<int, DestroyComponent> DestroyTags;
    public Dictionary<int, EnergyComponent> Energies;
    public Dictionary<int, MotherShipComponent> MotherShips;
    public Dictionary<int, PodComponent> Pods;
    public Dictionary<int, SpacemanComponent> Spacemen;
    public Dictionary<int, ColliderComponent> Colliders;
    public Dictionary<int, FuelComponent>  Fuels;
    public Dictionary<int, ResourceComponent> Resources;
    public Dictionary<int, MiningComponent>  MiningComponents;
    public Dictionary<int, ToolComponent> Tools;
    public Dictionary<int, InventoryComponent> Inventories;
    public Dictionary<int, AnimationComponent> Animations;
    public Dictionary<int, CameraFocusComponent> CameraFocus;
    public Dictionary<int, GeneratorComponent> Generators;

    // Spacial Hashmap and Reverse lookup map for supreme performance.
    public const int CHUNK_SIZE = 64;
    public Dictionary<ChunkCoord, List<int>> SpacialMap =  new Dictionary<ChunkCoord, List<int>>();
    private Dictionary<int, ChunkCoord> _entityChunkCache = new Dictionary<int, ChunkCoord>();
    
    //Reference to singleton entityIDs
    public int ControlledEntityId { get; private set; } = -1;
    public int PodId {get; private set; } = -1;
    public int MothershipId {get; private set; } = -1;
    public int SpacemanId {get; private set; } = -1;
    
    public  EntityManager()
    {
        Transforms = new Dictionary<int, TransformComponent>();
        Velocities = new Dictionary<int, VelocityComponent>();
        Renderables = new Dictionary<int, RenderComponent>();
        PlayerInputs = new Dictionary<int, PlayerInputComponent>();
        Energies = new Dictionary<int, EnergyComponent>();
        MotherShips = new Dictionary<int, MotherShipComponent>();
        Healths  = new Dictionary<int, HealthComponent>();
        DestroyTags = new Dictionary<int, DestroyComponent>();
        Pods = new Dictionary<int, PodComponent>();
        Spacemen = new Dictionary<int, SpacemanComponent>();
        Colliders  = new Dictionary<int, ColliderComponent>();
        Fuels = new Dictionary<int, FuelComponent>();
        Resources  = new Dictionary<int, ResourceComponent>();
        MiningComponents =  new Dictionary<int, MiningComponent>();
        Tools = new Dictionary<int, ToolComponent>();
        Inventories = new Dictionary<int, InventoryComponent>();
        CameraFocus = new Dictionary<int, CameraFocusComponent>();
        Animations = new Dictionary<int, AnimationComponent>();
        Generators = new Dictionary<int, GeneratorComponent>();
    }
    
    public int CreateNewEntityId() {
        int newId = _nextEntityId++;
        return newId;
    }

    public void SetPodId(int podId)
    {
        PodId = podId;
    }
    public void SetMothershipId(int mothershipId)
    {
        MothershipId = mothershipId;
    }
    public void SetSpacemanId(int spacemanId)
    {
        SpacemanId = spacemanId;
    }
    public void SetControlledEntity(int entityId)
    {
        ControlledEntityId = entityId;
    }

    public void DestroyEntity(int entityId)
    {
        if (entityId == ControlledEntityId) { ControlledEntityId = -1; }
        if (entityId == PodId) { PodId = -1; }
        if (entityId == MothershipId) { MothershipId = -1; }
        if (entityId == SpacemanId) { SpacemanId = -1; }
        
        Transforms.Remove(entityId);
        Renderables.Remove(entityId);
        Spacemen.Remove(entityId);
        Colliders.Remove(entityId);
        PlayerInputs.Remove(entityId);
        Velocities.Remove(entityId);
        DestroyTags.Remove(entityId);
        Pods.Remove(entityId);
        Inventories.Remove(entityId);
        Tools.Remove(entityId);
        MiningComponents.Remove(entityId);
        Resources.Remove(entityId);
        Fuels.Remove(entityId);
        
        RemoveEntityFromChunk(entityId);
    
        System.Console.WriteLine($"Destroyed entity: {entityId}");
    }
    
    // This is the most important function
    public ChunkCoord GetChunkCoordFromWorldPos(Vector2 worldPos)
    {
        // Note: Use Math.Floor to handle negative coordinates correctly
        int chunkX = (int)Math.Floor(worldPos.X / CHUNK_SIZE);
        int chunkY = (int)Math.Floor(worldPos.Y / CHUNK_SIZE);
        return new ChunkCoord { X = chunkX, Y = chunkY };
    }

    public void AddEntityToChunk(int entityId, Vector3 position)
    {
        ChunkCoord chunk = GetChunkCoordFromWorldPos(new Vector2(position.X, position.Y));

        if (!SpacialMap.ContainsKey(chunk))
        {
            SpacialMap[chunk] = new List<int>();
        }
        if (!SpacialMap[chunk].Contains(entityId))
        {
            SpacialMap[chunk].Add(entityId);
        }
        _entityChunkCache[entityId] = chunk;
    }

    public void RemoveEntityFromChunk(int entityId)
    {
        // 1. Check if we are tracking this entity
        if (_entityChunkCache.TryGetValue(entityId, out ChunkCoord chunk))
        {
            // 2. Get the list of entities for that chunk
            if (SpacialMap.ContainsKey(chunk))
            {
                // 3. Remove the entity from the list
                SpacialMap[chunk].Remove(entityId);
            }

            // 4. Remove the entity from our tracking cache
            _entityChunkCache.Remove(entityId);
        }
    }
    
    public void UpdateEntityChunk(int entityId, Vector3 newPosition)
    {
        // Get the entity's new chunk
        ChunkCoord newChunk = GetChunkCoordFromWorldPos(new Vector2(newPosition.X, newPosition.Y));

        // Check if we are tracking this entity
        if (_entityChunkCache.TryGetValue(entityId, out ChunkCoord oldChunk))
        {
            // If the chunk hasn't changed, do nothing
            if (oldChunk.Equals(newChunk))
            {
                return;
            }

            // Remove from old chunk's list
            if (SpacialMap.ContainsKey(oldChunk))
            {
                SpacialMap[oldChunk].Remove(entityId);
            }
        }

        // Add to the new chunk's list
        if (!SpacialMap.ContainsKey(newChunk))
        {
            SpacialMap[newChunk] = new List<int>();
        }
        SpacialMap[newChunk].Add(entityId);

        // Update the cache to the new location
        _entityChunkCache[entityId] = newChunk;
    }
    
    public void InitializeGameAssets()
    {
        // Load all textures from the "Assets/Textures" folder
        // (Make sure that folder exists and has your PNGs)
        AssetManager.LoadTextures("Assets/Textures");
        AssetManager.LoadAudio("Assets/sfx");
    }

}

