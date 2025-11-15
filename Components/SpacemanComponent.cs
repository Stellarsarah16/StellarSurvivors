using System.Numerics;

namespace StellarSurvivors.Components;

public struct SpacemanComponent
{
    private Vector2 _pod_position;
    public int PodId;
    public float Thrust;
    public float HorizontalSpeed;
    public float ThrustEnergyDrain;
    public float JumpForce;
    public bool IsOnGround;

    public SpacemanComponent(float thrust, float jumpForce, float speed, float energyDrain , Vector2 podPosition, int podId)
    {
        Thrust = thrust;
        HorizontalSpeed = speed;
        _pod_position = podPosition;
        PodId = podId;
        ThrustEnergyDrain = energyDrain;
        JumpForce = jumpForce;
        IsOnGround = false;
    }
}