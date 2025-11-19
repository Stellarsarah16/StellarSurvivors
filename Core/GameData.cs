namespace StellarSurvivors.Core;
using StellarSurvivors.Enums;

public class GameData
{
    // Using a Dictionary makes it easy to add new resources later without changing code
    public Dictionary<ResourceType, double> Resources { get; set; } = new();

    public GameData()
    {
        // Initialize resources
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            Resources[type] = 0;
        }
    }

    // Helper to safely add resources
    public void AddResource(ResourceType type, double amount)
    {
        if (Resources.ContainsKey(type))
        {
            Resources[type] += amount;
        }
    }
}