using Raylib_cs;
using System.Numerics;
using System.Linq; // We'll use this to find the player
using StellarSurvivors.Core;

namespace StellarSurvivors.Systems
{
    public class MovementSystem : IUpdateSystem
    {
        private EventManager _eventManager;
        private Vector2 _playerMoveDirection = Vector2.Zero;
        
        // You can change this to make the player faster or slower
        private const float _playerSpeed = 200.0f;
        
        public MovementSystem(EventManager eventManager)
        {
            _eventManager  = eventManager;
            _eventManager.Subscribe<PlayerMoveInputEvent>(OnPlayerMove);
        }
        
        public void Shutdown(EventManager eventManager)
        {
            eventManager.Unsubscribe<PlayerMoveInputEvent>(OnPlayerMove);
        }
        
        private void OnPlayerMove(PlayerMoveInputEvent e)
        {
            _playerMoveDirection = e.Direction;
        }

        public void Update(Game world, float  deltaTime)
        {
            if (_playerMoveDirection == Vector2.Zero)
            {
                return;
            }
            
            if (world.EntityManager.Pods.Count == 0) return;
            int playerId = world.EntityManager.Players.First();
            if (!world.EntityManager.Transforms.ContainsKey(playerId)) return;
            var transform = world.EntityManager.Transforms[playerId];
            
            Vector3 velocity = new Vector3(_playerMoveDirection.X, _playerMoveDirection.Y, 0) * _playerSpeed;
            float dt = Raylib.GetFrameTime();

            // Apply the velocity to the position
            transform.Position += velocity * dt;
            world.EntityManager.Transforms[playerId] = transform;

            // Reset State
            _playerMoveDirection = Vector2.Zero;
        }
    }
}
