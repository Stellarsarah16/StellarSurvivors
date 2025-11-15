namespace StellarSurvivors.Core;
using System.Numerics;
using StellarSurvivors.Enums;

public interface IEvent { }

// ------- EVENTS ---------//

// Event for 'E' key -  Interaction button
public struct UseButtonPressedEvent : IEvent { }

public struct ThrustInputEvent : IEvent { }
public struct JumpInputEvent : IEvent { }

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

public class LeftClickOutsideUIEvent : IEvent { }


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

public struct LeftMouseDownEvent : IEvent
{
    public Vector2 MousePosition;
    public Vector2 WorldPosition;
    public bool JustPressed;
    public LeftMouseDownEvent(Vector2 mousePosition, Vector2 worldPosition,  bool justPressed)
    {
        MousePosition = mousePosition;
        WorldPosition = worldPosition;
        JustPressed = justPressed;
    }
}

public struct RightMouseDownEvent : IEvent
{
    public Vector2 MousePosition;
    public Vector2 WorldPosition;
    public RightMouseDownEvent(Vector2 mousePosition, Vector2 worldPosition)
    {
        MousePosition = mousePosition;
        WorldPosition = worldPosition;
    }
}
public struct LeftMouseReleasedEvent : IEvent { }
public struct RightMouseReleasedEvent : IEvent { }

public struct EquipToolEvent : IEvent
{
    public int EntityId {get; set; }
    public string ToolName {get; set; }
}

public class RefineFuelEvent : IEvent{ }
public class RechargePodEvent : IEvent{ }
