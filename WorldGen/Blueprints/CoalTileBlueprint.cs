using System.Numerics;
using Raylib_cs;
using StellarSurvivors.Core;
using StellarSurvivors.Components;
using StellarSurvivors.Enums;

namespace StellarSurvivors.WorldGen.Blueprints
{
    public class CoalTileBlueprint : IEntityBlueprint
    {
        public void Apply(EntityManager entityManager, int entityId, BlueprintSettings settings)
        {

            var scale = settings.Scale;
            var size = settings.Size; // Size for RenderComponent
            
            entityManager.Transforms.Add(entityId, new TransformComponent {Position = settings.Position, Rotation = 0, Scale = scale });
            entityManager.Renderables.Add(entityId, new RenderComponent {Size = size, Color = new Color(50, 40, 60), Layer = RenderLayer.Background});

        }
    }
}


