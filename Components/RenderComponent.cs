using System.Numerics;
using Raylib_cs;
using StellarSurvivors.WorldGen;

namespace StellarSurvivors.Components;

public struct RenderComponent
{
    public Vector2 Size;
    public RenderLayer Layer;
    public Color Color;
    
    //public Texture2D Texture;
    //public Rectangle SourceRect;
}