using Fusion;
using System;
using UnityEngine;

public class PlayerTopEntity : Entity
{
    [Networked, OnChangedRender(nameof(OnHeldItemChanged))]
    public NetworkObject HeldItem { get; set; }

    [Networked]
    public NetworkBool ReleaseWithThrow { get; set; }

    public event Action OnCollidedWithPlayer;

    public PlayerController playerController;

    public PlayerRunTop runTopState;
    public PlayerIdleTop idleTopState;
    public PlayerHoldTop holdTopState;

    public Vector2 inputVector;
    public PlayerInput inputActions;

    private NetworkObject _lastAttachedItem;
    private NetworkObject _pendingPickup;
    private bool _pendingRelease;
    private bool _pendingReleaseThrow;

    private float throwingForce = 100f;

    public override void Spawned()
    {
        base.Spawned();

        playerController = GetComponent<PlayerController>();

        if (HasStateAuthority)
        {
            inputActions = new PlayerInput();
            inputActions.Enable();

            fsm = new FSM();

            runTopState = new PlayerRunTop(fsm, this);
            idleTopState = new PlayerIdleTop(fsm, this);
            holdTopState = new PlayerHoldTop(fsm, this);

            fsm.Init(idleTopState);
        }

        OnHeldItemChanged();
    }

    public override void Render()
    {
        base.Render();

        if (!HasStateAuthority) return;

        inputVector = inputActions.Player.WASD.ReadValue<Vector2>();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!HasStateAuthority) return;

        if (_pendingPickup != null)
        {
            if (!_pendingPickup.HasStateAuthority)
            {
                _pendingPickup.RequestStateAuthority();
                return;
            }

            ReleaseWithThrow = false;
            HeldItem = _pendingPickup;
            _pendingPickup = null;
        }

        if (_pendingRelease && HeldItem != null)
        {
            ReleaseWithThrow = _pendingReleaseThrow;
            HeldItem = null;
            _pendingRelease = false;
            _pendingReleaseThrow = false;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }

        base.Despawned(runner, hasState);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collided with another player. Invoking OnCollidedWithPlayer event.");
            OnCollidedWithPlayer?.Invoke();
        }
    }

    public void QueuePickup(NetworkObject targetNetObj)
    {
        if (!HasStateAuthority) return;

        _pendingPickup = targetNetObj;
    }

    public void QueueRelease(bool throwItem)
    {
        if (!HasStateAuthority) return;

        _pendingRelease = true;
        _pendingReleaseThrow = throwItem;
    }

    public void OnHeldItemChanged()
    {
        if (HeldItem != null)
        {
            AttachItemToHand(HeldItem);
            return;
        }

        if (_lastAttachedItem != null)
        {
            ReleaseItem(_lastAttachedItem, ReleaseWithThrow);
            _lastAttachedItem = null;
            
            if (TaskBoardManager.Instance != null)
            {
                TaskBoardManager.Instance.CloseTaskBoard();
            }
        }
    }

    private void AttachItemToHand(NetworkObject item)
    {
        _lastAttachedItem = item;

        var rb = item.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        var nt = item.GetComponent<NetworkTransform>();
        if (nt != null)
        {
            nt.enabled = false;
        }

        item.transform.SetParent(playerController.playerHand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    private void ReleaseItem(NetworkObject item, bool throwItem)
    {
        item.transform.SetParent(null);

        var rb = item.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var nt = item.GetComponent<NetworkTransform>();
        if (nt != null)
        {
            nt.enabled = true;
            nt.Teleport(item.transform.position, item.transform.rotation);
        }

        if (throwItem && item.HasStateAuthority)
        {
            Vector3 throwDir = (playerController.playerRotation.transform.forward + Vector3.up).normalized;
            rb.AddForce(throwDir * throwingForce, ForceMode.Impulse);
        }
    }
}
