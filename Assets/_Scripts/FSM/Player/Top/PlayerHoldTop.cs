using UnityEngine;

public class PlayerHoldTop : State
{
    private Transform target;

    public PlayerHoldTop(FSM fsm, Entity entity) : base(fsm, entity)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Enter PlayerHoldTop");

        target = ((PlayerTopEntity)entity).playerController.playerFieldOfView.visibleOrderedTargets[0];
        target.SetParent(((PlayerTopEntity)entity).playerController.playerHand);
        target.localPosition = Vector3.zero;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (((PlayerTopEntity)entity).inputActions.Player.Hold.WasPressedThisFrame())
        {
            fsm.ChangeState(((PlayerTopEntity)entity).idleTopState);
            return;
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        
        target.SetParent(null);
    }
}
