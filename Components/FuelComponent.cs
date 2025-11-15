namespace StellarSurvivors.Components;

public struct FuelComponent
{
    public float CurrentFuel;
    public float RechargeRate;
    public float FuelCapacity;
    
    public FuelComponent(float currentFuel, float fuelRate, float fuelCapacity)
    {
        CurrentFuel = currentFuel;
        RechargeRate = fuelRate;
        FuelCapacity = fuelCapacity;

    }
}