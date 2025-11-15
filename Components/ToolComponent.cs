namespace StellarSurvivors.Components;
using Gameplay.Tools;

public class ToolComponent
{
    // The currently equipped tool
    public ITool CurrentTool { get; set; }
    public Dictionary<string, ITool> Toolset { get; private set; }
    private List<string> _toolNames;
    private int _currentToolIndex;

    public ToolComponent()
    {
        // Pre-populate the toolset
        Toolset = new Dictionary<string, ITool>
        {
            { "Drill", new DrillTool() },
            { "MoveTile", new MoveTileTool() } // You can implement this later
        };
        
        _toolNames = Toolset.Keys.ToList();
        
        _currentToolIndex = 0;
        CurrentTool = Toolset[_toolNames[_currentToolIndex]];
    }

    public void EquipTool(string toolName)
    {
        if (Toolset.TryGetValue(toolName, out ITool tool))
        {
            CurrentTool = tool;
            _currentToolIndex = _toolNames.IndexOf(toolName);
        }
    }
    
    public void CycleToolNext()
    {
        _currentToolIndex++; // Move to the next index
        
        // If we go past the end, wrap around to the start
        if (_currentToolIndex >= _toolNames.Count)
        {
            _currentToolIndex = 0;
        }

        // Equip the tool at the new index
        EquipTool(_toolNames[_currentToolIndex]);
    }

    // --- NEW METHOD 2 ---
    public void CycleToolPrevious()
    {
        _currentToolIndex--; // Move to the previous index

        // If we go before the start, wrap around to the end
        if (_currentToolIndex < 0)
        {
            _currentToolIndex = _toolNames.Count - 1;
        }

        // Equip the tool at the new index
        EquipTool(_toolNames[_currentToolIndex]);
    }
}