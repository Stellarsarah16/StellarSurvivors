using System.Collections.Generic;

namespace StellarSurvivors.Components
{
    public struct HealthComponent
    {
        public int CurrentHealth;
        public int MaxHealth;
        public List<int> DamageToApply = new List<int>();

        public HealthComponent(int health)
        {
            this.CurrentHealth = health;
            this.MaxHealth = health;
        }
    }
}