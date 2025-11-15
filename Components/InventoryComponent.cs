namespace StellarSurvivors.Components;
using StellarSurvivors.Enums;

public class InventoryComponent
{
    public Dictionary<ResourceType, int> Contents { get; private set; } = new Dictionary<ResourceType, int>();
    public int MaxCapacity { get; private set; }
    public int MaxSlots {get; private set;}
    
    public InventoryComponent(int maxCapacity,  int maxSlots)
    {
        MaxCapacity = maxCapacity;
        MaxSlots = maxSlots;    
    }
    
    public void ClearContents() { Contents.Clear(); }
    
    public int GetCurrentLoad()
    {
        int load = 0;
        foreach (var quantity in Contents.Values) { load += quantity; }
        return load;
    }
    
    public bool TryAddItem(ResourceType type, int quantity)
    {
        if (GetCurrentLoad() + quantity > MaxCapacity)
        {
            return false; // Not enough space
        }

        if (Contents.ContainsKey(type))
        {
            Contents[type] += quantity;
            return true;
        }
        if (Contents.Count < MaxSlots)
        {
            Contents.Add(type, quantity);
            return true;
        }
        return false;
    }
    
    public bool TryRemoveItem(ResourceType type, int quantity)
    {
        // 1. Check if we have the item
        if (!Contents.ContainsKey(type)) { return false; }

        // 2. Check if we have enough
        if (Contents[type] < quantity) { return false; }

        // 3. Subtract the quantity
        Contents[type] -= quantity;

        // 4. If the stack is empty, remove it from the dictionary
        if (Contents[type] == 0) { Contents.Remove(type); }
        
        return true;
    }
    
}