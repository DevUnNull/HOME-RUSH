using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CameraObstacleManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera dùng để raycast từ mắt người chơi. Nếu bỏ trống, sẽ dùng Camera.main.")]
    public Camera camera;

    [Tooltip("Transform đích cần giữ rõ, thường là thân hoặc đầu của player.")]
    public Transform player;

    [Header("Obstacle Settings")]
    [Tooltip("LayerMask chỉ bao gồm layer chứa Walls / obstacles cần làm trong suốt.")]
    public LayerMask obstacleLayerMask;

    [Tooltip("Bán kính spherecast để đảm bảo phát hiện các bức tường mỏng hoặc nhiều collider nhỏ.")]
    [Range(0f, 0.5f)]
    public float sphereCastRadius = 0.12f;

    [Tooltip("Nếu bật, dùng spherecast để tránh hiện tượng bỏ sót khi camera bị che bởi cạnh tường.")]
    public bool useSphereCast = true;

    [Tooltip("Vẽ raycast debug trong Scene view để dễ kiểm tra logic.")]
    public bool debugRay = false;

    [Tooltip("In log chi tiết mỗi lần raycast để debug.")]
    public bool debugLogs = false;

    private readonly List<HideableObject> currentlyInTheWay = new List<HideableObject>();
    private readonly List<HideableObject> alreadyTransparent = new List<HideableObject>();

    private void Awake()
    {
        if (camera == null)
            camera = Camera.main;
    }

    private void OnDisable()
    {
        RestoreAllTransparentObjects();
    }

    private void OnDestroy()
    {
        RestoreAllTransparentObjects();
    }

    private void Update()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        EnsurePlayerReference();

        if (camera == null || !IsValidPlayerTransform(player))
        {
            if (debugLogs)
            {
                Debug.LogWarning($"CameraObstacleManager missing or invalid references: camera={(camera == null ? "null" : camera.name)} player={(player == null ? "null" : player?.name ?? "invalid")}", this);
            }
            return;
        }

        if (debugLogs)
        {
            Debug.Log($"CameraObstacleManager checking obstacles: camera={camera.name}, player={player.name}, layerMask={obstacleLayerMask.value}, sphereCast={useSphereCast}, radius={sphereCastRadius}", this);
        }

        currentlyInTheWay.Clear();

        Vector3 origin = camera.transform.position;
        Vector3 targetPosition = player.position;
        Vector3 direction = targetPosition - origin;
        float distance = direction.magnitude;

        if (distance <= Mathf.Epsilon)
            return;

        direction /= distance;

        RaycastHit[] hits;
        if (useSphereCast && sphereCastRadius > 0f)
        {
            hits = Physics.SphereCastAll(origin, sphereCastRadius, direction, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            hits = Physics.RaycastAll(origin, direction, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore);
        }

        if (debugRay)
        {
            Debug.DrawLine(origin, targetPosition, Color.cyan);
            foreach (var hit in hits)
            {
                Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.25f, Color.red, 0.05f);
            }
        }

        if (debugLogs)
        {
            Debug.Log($"CameraObstacleManager raycast hit count={hits.Length}", this);
        }

        foreach (RaycastHit hit in hits)
        {
            if (debugLogs)
            {
                Debug.Log($" CameraObstacleManager hit collider={hit.collider.name}, layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}", this);
            }

            foreach (HideableObject hideable in GetHideableObjects(hit))
            {
                if (hideable != null)
                {
                    if (debugLogs)
                    {
                        Debug.Log($"  found HideableObject on {hideable.gameObject.name}", this);
                    }

                    if (!currentlyInTheWay.Contains(hideable))
                    {
                        currentlyInTheWay.Add(hideable);
                    }
                }
            }
        }

        if (debugLogs)
        {
            Debug.Log($"CameraObstacleManager currentlyInTheWay count={currentlyInTheWay.Count}", this);
        }

        foreach (HideableObject hideable in currentlyInTheWay)
        {
            if (!alreadyTransparent.Contains(hideable))
            {
                if (debugLogs)
                {
                    Debug.Log($" CameraObstacleManager MakeTransparent {hideable.gameObject.name}", this);
                }
                hideable.MakeTransparent();
                alreadyTransparent.Add(hideable);
            }
        }

        for (int i = alreadyTransparent.Count - 1; i >= 0; i--)
        {
            HideableObject hideable = alreadyTransparent[i];
            if (hideable == null || !currentlyInTheWay.Contains(hideable))
            {
                if (hideable != null)
                {
                    if (debugLogs)
                    {
                        Debug.Log($" CameraObstacleManager MakeSolid {hideable.gameObject.name}", this);
                    }
                    hideable.MakeSolid();
                }
                alreadyTransparent.RemoveAt(i);
            }
        }
    }

    private void RestoreAllTransparentObjects()
    {
        for (int i = alreadyTransparent.Count - 1; i >= 0; i--)
        {
            HideableObject hideable = alreadyTransparent[i];
            if (hideable != null)
            {
                hideable.MakeSolid();
            }
        }
        alreadyTransparent.Clear();
    }

    private void EnsurePlayerReference()
    {
        if (IsValidPlayerTransform(player))
            return;

        if (player != null && debugLogs)
        {
            Debug.Log($"CameraObstacleManager: current player reference is invalid prefab/reference -> {player.name}", this);
        }

        Debug.Log("CameraObstacleManager: Attempting to auto-assign local player...", this);

        if (TryAssignFromParentHierarchy())
        {
            Debug.Log($"CameraObstacleManager: Assigned player from parent hierarchy -> {player.name}", this);
            return;
        }

        if (TryAssignFromFusionRunner())
        {
            Debug.Log($"CameraObstacleManager: Assigned player from Fusion runner -> {player.name}", this);
            return;
        }

        if (TryAssignFromAnyNetworkObject())
        {
            Debug.Log($"CameraObstacleManager: Assigned player from NetworkObject search -> {player.name}", this);
            return;
        }

        if (TryAssignFromTaggedPlayers())
        {
            Debug.Log($"CameraObstacleManager: Assigned player from tagged players -> {player.name}", this);
            return;
        }

        // Still null or invalid - will retry next Update
    }

    private bool IsValidPlayerTransform(Transform target)
    {
        if (target == null)
            return false;

        if (target.gameObject == null)
            return false;

        if (!target.gameObject.scene.IsValid())
            return false;

        return target.gameObject.activeInHierarchy;
    }

    private bool TryAssignFromParentHierarchy()
    {
        Transform current = transform;
        while (current != null)
        {
            if (current != transform && current.GetComponent<NetworkObject>() != null)
            {
                player = current;
                return true;
            }
            if (current != transform && current.GetComponent<PlayerController>() != null)
            {
                player = current;
                return true;
            }
            current = current.parent;
        }

        return false;
    }

    private bool TryAssignFromFusionRunner()
    {
        // Try find any active NetworkRunner and get its local player object
        NetworkRunner[] runners = FindObjectsOfType<NetworkRunner>();
        if (runners == null || runners.Length == 0)
            return false;

        foreach (var runner in runners)
        {
            if (runner == null)
                continue;

            // Attempt to get player object for the runner's LocalPlayer
            try
            {
                NetworkObject playerObject = runner.GetPlayerObject(runner.LocalPlayer);
                if (playerObject != null)
                {
                    player = playerObject.transform;
                    return true;
                }
            }
            catch
            {
                // Some runner states may throw; ignore and continue
            }
        }

        return false;
    }

    private bool TryAssignFromAnyNetworkObject()
    {
        NetworkObject[] allNetObjs = FindObjectsOfType<NetworkObject>();
        foreach (var netObj in allNetObjs)
        {
            if (netObj == null)
                continue;

            // Prefer objects that have input authority (likely the local player)
            if (netObj.HasInputAuthority)
            {
                player = netObj.transform;
                return true;
            }
        }

        return false;
    }

    private bool TryAssignFromTaggedPlayers()
    {
        GameObject[] taggedPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject taggedPlayer in taggedPlayers)
        {
            if (taggedPlayer == null)
                continue;

            if (taggedPlayer.TryGetComponent<NetworkObject>(out NetworkObject netObj) && netObj.HasInputAuthority)
            {
                player = taggedPlayer.transform;
                return true;
            }
        }

        return false;
    }

    private IEnumerable<HideableObject> GetHideableObjects(RaycastHit hit)
    {
        HideableObject parentHideable = hit.collider.GetComponentInParent<HideableObject>();
        if (parentHideable != null)
            yield return parentHideable;

        foreach (HideableObject childHideable in hit.collider.GetComponentsInChildren<HideableObject>())
        {
            if (childHideable != parentHideable)
                yield return childHideable;
        }
    }
}
