using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Enums;

namespace StellarSurvivors.Components;

public class RenderComponent
{
    public Vector2 Size;
    public RenderLayer Layer;
    public RenderType Type; 
    
    public ShapeType Shape;
    public Color Color;
    
    public Texture2D Texture;
    public Rectangle SourceRect;
    public Color Tint;
}

public enum RenderType
{
    Shape,
    Texture
}

public enum ShapeType
{
    Rectangle,
    PodTriangle
}