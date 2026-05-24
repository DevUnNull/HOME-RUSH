using UnityEngine;

public class FSM
{
    public State currentState;

    public FSM()
    {

    }

    public void Init(State newState)
    {
        currentState = newState;
        currentState.EnterState();
    }

    public void ChangeState(State newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}
