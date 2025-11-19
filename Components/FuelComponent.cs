namespace StellarSurvivors.Components;

public class FuelComponent
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

    public bool ConsumeFuel(float amount)
    {
        if (amount > CurrentFuel) return false;
        
        CurrentFuel -= amount;
        return true;
    }
    
    public void Refuel(float amount)
    {
        CurrentFuel += amount;
        if (CurrentFuel > FuelCapacity)
        {
            CurrentFuel = FuelCapacity;
        }
    }
}