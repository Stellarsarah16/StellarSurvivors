namespace StellarSurvivors.WorldGen.TileData;
using StellarSurvivors.Enums;

public class TileDefinition
{
    public string Name { get; set; } = "Unknown";
    public bool IsSolid { get; set; } = false;
    public float Friction { get; set; } = 1.0f;
    public float Hardness  { get; set; } = 0f;
    public bool IsFlammable { get; set; } = false;
        
    // This is the "composition" part of your synergy.
    // A TileDefinition is "composed" of a TileRenderDefinition.
    public TileRenderDefinition RenderInfo {  get; set; }

    public ResourceType? DropsResource { get; set; } = null;
}