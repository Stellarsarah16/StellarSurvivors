using StellarSurvivors.Core;

namespace StellarSurvivors.Systems
{
    public interface IUpdateSystem
    {
        public void Update(Game world, float deltaTime);
    }
    
}