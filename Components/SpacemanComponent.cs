using System.Numerics;

namespace StellarSurvivors.Components;

public struct SpacemanComponent
{
    private Vector2 _pod_position;
    private int _podId;

    public SpacemanComponent(Vector2 podPosition, int podId)
    {
        _pod_position = podPosition;
        _podId = podId;
    }
}