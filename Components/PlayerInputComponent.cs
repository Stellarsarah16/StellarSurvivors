namespace StellarSurvivors.Components
{
    public class PlayerInputComponent
    {
        public float Speed;
        public float FacingDirection { get; set; } = 1.0f;
        
        public PlayerInputComponent(float speed)
        {
            this.Speed = speed;
        }
    }
}