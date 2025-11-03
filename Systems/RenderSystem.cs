using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.WorldGen;

namespace StellarSurvivors.Systems
{
    public class RenderSystem
    {
        public Vector2 Size;
        public Color Color;
        private RenderLayer _layerToDraw;

        public RenderSystem(RenderLayer layerToDraw)
        {
            _layerToDraw = layerToDraw;
        }
        

        public void Draw(Game world, Camera2D camera)
        {
            foreach (var entityId in world.EntityManager.Renderables.Keys)
            {
                var ren = world.EntityManager.Renderables[entityId];
                if (ren.Layer == _layerToDraw)
                {
                    if (world.EntityManager.Transforms.ContainsKey(entityId))
                    {
                        var transform = world.EntityManager.Transforms[entityId];

                        Vector2 pos2d = new Vector2(transform.Position.X, transform.Position.Y);
                        Vector2 size = ren.Size;
                        Vector2 scaledSize = new Vector2(size.X * transform.Scale.X, size.Y * transform.Scale.Y);
                        Rectangle destRect = new Rectangle(pos2d.X, pos2d.Y, scaledSize.X, scaledSize.Y);
                        Vector2 origin = new Vector2(scaledSize.X / 2.0f, scaledSize.Y / 2.0f);

                        Raylib.DrawRectanglePro(
                            destRect,
                            origin,
                            transform.Rotation,
                            ren.Color
                        );
                    }
                }
            }


        }
    }
}