using UnityEngine;

public class State
{
    protected FSM fsm;
    protected Entity entity;

    public State(FSM fsm, Entity entity)
    {
        this.fsm = fsm;
        this.entity = entity;
    }

    public virtual void EnterState()
    {
     
    }

    public virtual void ExitState()
    {

    }

    public virtual void UpdateLogic()
    {

    }

    public virtual void UpdatePhysics()
    {

    }
}
