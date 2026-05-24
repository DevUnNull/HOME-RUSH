using Fusion;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    protected FSM fsm;

    protected virtual void Update()
    {
        fsm.currentState.UpdateLogic();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        fsm.currentState.UpdatePhysics();
    }
}
