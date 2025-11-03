using StellarSurvivors.Components;
using StellarSurvivors.Core;

namespace StellarSurvivors.States;

public class StateManager
{
    // is supposed to be a stack
    private Stack<IGameState> _states = new Stack<IGameState>();

    public StateManager()
    {
    }

    public void PushState(IGameState state, Game world)
    {
        // Deactivate the old "top" state if there is one
        if (_states.Count > 0)
        {
            var oldState = GetCurrentState();
            // We'll add an OnPause() method later if we need it
        }

        _states.Push(state);
        state.Enter();
    }

    public void PopState(Game world)
    {
        if (_states.Count > 0)
        {
            var oldState = _states.Pop();
            oldState.Exit();
        }
            
        // "Resume" the new "top" state if there is one
        if (_states.Count > 0)
        {
            var newState = GetCurrentState();
            // We'll add an OnResume() method later
        }
    }

    // Swaps the top state (e.g., MainMenu -> Gameplay)
    public void ChangeState(IGameState state, Game world)
    {
        // Pop the old state
        if (_states.Count > 0)
        {
            var oldState = _states.Pop();
            oldState.Exit();
        }
        // Push the new one
        _states.Push(state);
        state.Enter();
    }
    
    // Safely gets the state on top of the stack
    public IGameState GetCurrentState()
    {
        if (_states.Count == 0)
        {
            throw new System.InvalidOperationException(
                "State stack is empty. Cannot GetCurrentState(). " +
                "Did you forget to push an initial state, or did you pop the last state?"
            );
        }
        return _states.Peek();
    }

    // --- These are the most important methods! ---
    // The Game.Run() loop will call these.
        
    public void Update(Game world, float dt)
    {
        var currentState = GetCurrentState();
        currentState?.Update(this, dt);
    }
        
    public void Draw(Game world)
    {
        var currentState = GetCurrentState();
        currentState?.Draw();
    }
}
    
    