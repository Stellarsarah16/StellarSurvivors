using System.Numerics;

namespace StellarSurvivors.Core;
using StellarSurvivors.Components;

public class EntityManager
{
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
    public HashSet<int> Players {get; private set;}
    
    //Reference to singleton entityIDs
    private int _podId;
    private Vector2 _podPosition;
    private int _mothershipId;
    private Vector2 _mothershipPosition;
    private int SpacemanId;
    
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
        Players = new HashSet<int>();
    }
    
    public int CreateNewEntityId() {
        // Get the current ID and Increment
        int newId = _nextEntityId++;
        return newId;
    }

    public void SetPodId(int podId)
    {
        _podId = podId;
    }

    public int GetPodId()
    {
        return _podId;
    }
    
    public void SetMothershipId(int mothershipId)
    {
        _mothershipId = mothershipId;
    }

    public void DestroyEntity(int entityId)
    {
        Transforms.Remove(entityId);
        Renderables.Remove(entityId);
        Spacemen.Remove(entityId);
        Colliders.Remove(entityId);
        PlayerInputs.Remove(entityId);
        Velocities.Remove(entityId);
        DestroyTags.Remove(entityId);
        Pods.Remove(entityId);
        Players.Remove(entityId);
    
        System.Console.WriteLine($"Destroyed entity: {entityId}");
    }
}