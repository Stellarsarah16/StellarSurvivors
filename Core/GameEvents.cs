using System.Numerics;

namespace StellarSurvivors.Core
{
    /// <summary>
    /// A marker interface for all game events.
    /// </summary>
    public interface IEvent { }

    // generic event for the 'E' key
    public struct UseButtonPressedEvent : IEvent { }

    public struct RotationInputEvent : IEvent
    {
        public float Direction; // -1 for left, 1 for right
    }

    public struct ThrustInputEvent : IEvent
    {
        
    }

    public struct ExitPodEvent : IEvent
    {
        public int PodId;
        public Vector2 PodPosition;
    }

    public struct ReturnToPodEvent : IEvent
    {
        public int SpacemanId;
        public int PodId;
    }
    
    public class ToggleSidebarEvent : IEvent { }

    public class ManualGoldClickEvent : IEvent { }


    // --- Game Logic Events ---

    /// <summary>
    /// Published by InputSystem when movement keys are pressed.
    /// Listened for by MovementSystem to update player velocity.
    /// </summary>
    public class PlayerMoveInputEvent : IEvent
    {
        /// <summary>
        /// The direction of input (e.g., (0, -1) for Up).
        /// </summary>
        public Vector2 Direction;

        public PlayerMoveInputEvent(Vector2 direction)
        {
            Direction = direction;
        }
    }
    
    // In your EventManager.cs
    public struct CollisionEvent : IEvent
    {
        public int EntityA;
        public int EntityB;
    }

// In your EventManager.cs
    public struct WorldCollisionEvent : IEvent
    {
        public int EntityId;
        public int TileX;
        public int TileY;
        public TileType TileType;
    }
}