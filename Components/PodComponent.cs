namespace StellarSurvivors.Components;

public struct PodComponent
{
    public float rotationSpeed;
    public float thrustForce;
    public float thrustEnergyDrain;
    public float ThrustFuelDrain;
    
    public PodComponent(float rotationSpeed, float thrustForce, float thrustEnergyDrain)
    {
        this.rotationSpeed = rotationSpeed;
        this.thrustForce = thrustForce;
        this.thrustEnergyDrain = thrustEnergyDrain;
    }
}