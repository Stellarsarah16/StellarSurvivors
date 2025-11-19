namespace StellarSurvivors.WorldGen.TileData;
using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Components;

public class TileRenderDefinition
{
    // Tells the RenderSystem whether to draw a shape or a texture
    public RenderType Type { get; set; } = RenderType.Shape;
            
    // --- For Shape rendering ---
    public ShapeType Shape { get; set; } = ShapeType.Rectangle;
    public Color Color { get; set; } = Color.Magenta; // Default to magenta to see errors

    // --- For Texture rendering ---
    public Texture2D Texture { get; set; }
    public Rectangle SourceRect { get; set; }
    public Color Tint { get; set; } = Color.White;
    
}