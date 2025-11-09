using System.Numerics;

namespace StellarSurvivors.Core
{

    public interface IEvent { }
    
    // ------- EVENTS ---------//

    // Event for 'E' key -  Interaction button
    public struct UseButtonPressedEvent : IEvent { }
    
    
    public struct ThrustInputEvent : IEvent
    {
        
    }

    public struct ExitPodEvent : IEvent
    {
        public int PodId;
        public Vector2 PodPosition;
    }

    // 'E' Key
    public struct ReturnToPodEvent : IEvent
    {
        public int SpacemanId;
        public int PodId;
    }
    
    public class ToggleSidebarEvent : IEvent { }

    public class ManualGoldClickEvent : IEvent { }
    
    
    // Event for 'A' and 'D' 
    public class MoveInputEvent : IEvent
    {
        public float Direction;

        public MoveInputEvent(float direction)
        {
            Direction = direction;
        }
    }
    
    public struct CollisionEvent : IEvent
    {
        public int EntityA;
        public int EntityB;
    }
    
    public struct WorldCollisionEvent : IEvent
    {
        public int EntityId;
        public int TileX;
        public int TileY;
        public TileType TileType;
    }
}