using Fusion;
using UnityEngine;

public class PlayerTopEntity : Entity
{
    [Networked, OnChangedRender(nameof(OnHeldItemChanged))]
    public NetworkObject HeldItem { get; set; }

    public PlayerController playerController;

    public PlayerRunTop runTopState;
    public PlayerIdleTop idleTopState;
    public PlayerHoldTop holdTopState;

    public Vector2 inputVector;
    public PlayerInput inputActions;

    public override void Spawned()
    {
        base.Spawned();
        if (!HasStateAuthority) return;

        inputActions = new PlayerInput();
        inputActions.Enable();

        playerController = GetComponent<PlayerController>();
        fsm = new FSM();

        runTopState = new PlayerRunTop(fsm, this);
        idleTopState = new PlayerIdleTop(fsm, this);
        holdTopState = new PlayerHoldTop(fsm, this);

        fsm.Init(idleTopState);
    }

    protected override void Update()
    {
        base.Update();

        if (!HasStateAuthority) return;

        inputVector = inputActions.Player.WASD.ReadValue<Vector2>();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        inputActions.Disable();
        base.Despawned(runner, hasState);
    }

    public void PickUpItem(NetworkObject targetNetObj)
    {
        if (!targetNetObj.HasStateAuthority)
        {
            targetNetObj.RequestStateAuthority();
        }
        HeldItem = targetNetObj;
    }

    public void DropItem(NetworkObject targetNetObj)
    {
        HeldItem = null;
    }

    public void ThrowItem(NetworkObject targetNetObj)
    {
        var rb = targetNetObj.GetComponent<Rigidbody>();

        targetNetObj.transform.SetParent(null);

        var nt = targetNetObj.GetComponent<NetworkTransform>();
        nt.enabled = false;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        Vector3 throwDir = (playerController.playerRotation.transform.forward + Vector3.up).normalized;
        rb.AddForce(throwDir * 30f, ForceMode.Impulse);

        HeldItem = null;
    }

    public void OnHeldItemChanged()
    {
        if (HeldItem != null)
        {
            HeldItem.GetComponent<Rigidbody>().isKinematic = true;
            HeldItem.GetComponent<NetworkTransform>().enabled = false;
            HeldItem.transform.SetParent(playerController.playerHand);
            HeldItem.transform.localPosition = Vector3.zero;
        }
        else
        {
            if (playerController.playerHand.childCount > 0)
            {
                Transform itemTransform = playerController.playerHand.GetChild(0);
                itemTransform.SetParent(null);
                itemTransform.GetComponent<Rigidbody>().isKinematic = false;
                itemTransform.GetComponent<NetworkTransform>().enabled = true;
                itemTransform.GetComponent<NetworkTransform>().Teleport(itemTransform.position, itemTransform.rotation);
            }
        }
    }
}
