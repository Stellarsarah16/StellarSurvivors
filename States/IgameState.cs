namespace StellarSurvivors.States;
using StellarSurvivors.Core;

public interface IGameState
{
    void Enter();
    
    void Exit();
    
    void Update(StateManager stateManager, float dt);
    
    void Draw();
}