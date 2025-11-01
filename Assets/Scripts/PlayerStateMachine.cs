using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine 
{
    public PlayerState _CurrentState;

  public PlayerState GetCurrentState()
    {
        return _CurrentState; 
    }

    private PlayerState SetCurrentState(PlayerState state)
    {
        return _CurrentState = state;
    }

    public void ChangeState(PlayerState newState)
    {
        _CurrentState.Exit();
        _CurrentState = newState;
        _CurrentState.Enter();
    }   

    public void InitializeStateMachine(PlayerState startingState)
    {
        _CurrentState = startingState;
        _CurrentState.Enter();
    }

}
