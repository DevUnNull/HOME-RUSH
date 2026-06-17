using Fusion;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    protected FSM fsm;

    public override void Render()
    {
        base.Render();
        if (fsm == null || fsm.currentState == null) return;

        fsm.currentState.UpdateLogic();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (fsm == null || fsm.currentState == null) return;

        fsm.currentState.UpdatePhysics();
    }
}
