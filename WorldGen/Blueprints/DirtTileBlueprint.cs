using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.Components;

namespace StellarSurvivors.WorldGen.Blueprints
{
    public class DirtTileBlueprint : IEntityBlueprint
    {
        public void Apply(Game world, int entityId, BlueprintSettings settings)
        {
            // This is the "recipe" for a grass tile.
            // It just needs a Transform and a Renderable.
            
            // Note: We're making a 1x1 tile. We'd get this from settings later.
            var scale = settings.Scale;
            var size = settings.Size; // Size for RenderComponent
            
            world.EntityManager.Transforms.Add(entityId, new TransformComponent{Position = settings.Position, Rotation = 0, Scale = scale });
            world.EntityManager.Renderables.Add(entityId, new RenderComponent {Size = size, Color = Color.Brown, Layer = RenderLayer.Background});
            
            // That's it! It has no Health, no Input, no Velocity.
        }
    }
}


