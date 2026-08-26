using UnityEngine;
using Fusion;
using System.Collections.Generic;
using BoomDog.Core;
using TMPro;

namespace BoomDog.Visual
{
    public class CardGameVisualManager : MonoBehaviour
    {
        public static CardGameVisualManager Instance { get; private set; }

        [Header("Prefabs & References")]
        public GameObject originalPlayerPrefab; // The user's main Player.prefab
        public GameObject cardGamePlayerVisualPrefab; // Optional prefab with CardGamePlayerVisual script
        public Transform tableCenter;

        [Header("Layout Settings")]
        public Vector3 localPlayerPosition = new Vector3(0, 0.5f, -6f); // Bottom
        public float opponentArcRadius = 7f;
        public float opponentArcHeight = 0.5f;

        private Dictionary<PlayerRef, CardGamePlayerVisual> _playerVisuals = new Dictionary<PlayerRef, CardGamePlayerVisual>();
        private GameObject _targetingBlocker;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (CardGameManager.Instance == null || !CardGameManager.Instance.IsGameStarted) return;

            // Initialize visuals if needed
            List<PlayerRef> activePlayers = CardGameManager.Instance.GetAlivePlayerRefs();
            if (_playerVisuals.Count == 0 && activePlayers.Count > 0)
            {
                // Include local player even if dead
                List<PlayerRef> allPlayers = new List<PlayerRef>();
                foreach (var cp in CardGameManager.Instance.spawnedCardPlayers)
                {
                    if (cp != null && cp.Object != null)
                    {
                        allPlayers.Add(cp.Object.StateAuthority);
                    }
                }
                
                if (allPlayers.Count > 0)
                {
                    InitializeVisuals(allPlayers);
                }
            }

            // Sync States
            UpdateVisualStates();
        }

        private void InitializeVisuals(List<PlayerRef> allPlayers)
        {
            // Sort players deterministically by PlayerId to ensure consistent order
            allPlayers.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));

            // Find local player index
            int localIndex = -1;
            PlayerRef localRef = CardGameManager.Instance.Runner.LocalPlayer;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                if (allPlayers[i] == localRef)
                {
                    localIndex = i;
                    break;
                }
            }

            if (localIndex == -1) return; // Wait until local player joins

            // Rearrange array so local player is first (Index 0)
            List<PlayerRef> orderedPlayers = new List<PlayerRef>();
            for (int i = 0; i < allPlayers.Count; i++)
            {
                int idx = (localIndex + i) % allPlayers.Count;
                orderedPlayers.Add(allPlayers[idx]);
            }

            // Instantiate and Position
            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                PlayerRef pRef = orderedPlayers[i];
                CardPlayer logicPlayer = CardGameManager.Instance.GetCardPlayer(pRef);
                
                if (logicPlayer == null) continue;

                Vector3 targetPos = Vector3.zero;
                if (i == 0) // Local Player
                {
                    targetPos = localPlayerPosition;
                }
                else // Opponents (Arc layout)
                {
                    int totalOpponents = orderedPlayers.Count - 1;
                    float angleStep = 180f / (totalOpponents + 1);
                    float angle = angleStep * i; // Distribute evenly from 0 to 180 (excluding edges)
                    
                    // Convert angle to position (assuming table center is 0,0,0)
                    float rad = angle * Mathf.Deg2Rad;
                    float x = -Mathf.Cos(rad) * opponentArcRadius; // Start from left to right
                    float z = Mathf.Sin(rad) * opponentArcRadius;
                    
                    targetPos = new Vector3(x, opponentArcHeight, z);
                    if (tableCenter != null) targetPos += tableCenter.position;
                }

                GameObject visualGo = null;
                if (cardGamePlayerVisualPrefab != null)
                {
                    visualGo = Instantiate(cardGamePlayerVisualPrefab, targetPos, Quaternion.identity);
                }
                else
                {
                    // Create fallback visually if prefab is not assigned
                    visualGo = new GameObject($"PlayerVisual_{pRef.PlayerId}");
                    visualGo.transform.position = targetPos;
                    var cv = visualGo.AddComponent<CardGamePlayerVisual>();
                    _playerVisuals.Add(pRef, cv);
                    
                    // Look at center
                    Vector3 lookPos = tableCenter != null ? tableCenter.position : Vector3.zero;
                    lookPos.y = visualGo.transform.position.y;
                    visualGo.transform.LookAt(lookPos);
                    
                    PlayerColor pColor = PlayerSkinData.Instance != null ? PlayerSkinData.Instance.GetPlayerSkin(pRef) : PlayerColor.White;
                    cv.Initialize(pRef, logicPlayer.PlayerName.ToString(), pColor, originalPlayerPrefab);
                    
                    if (i == 0) visualGo.transform.localScale = Vector3.one * 1.2f; // Local player slightly bigger
                    else visualGo.transform.localScale = Vector3.one * 0.85f;
                    continue; // Skip the rest if using fallback
                }

                var visualComp = visualGo.GetComponent<CardGamePlayerVisual>();
                if (visualComp != null)
                {
                    _playerVisuals.Add(pRef, visualComp);
                    
                    // Look at center
                    Vector3 lookPos = tableCenter != null ? tableCenter.position : Vector3.zero;
                    lookPos.y = visualGo.transform.position.y;
                    visualGo.transform.LookAt(lookPos);

                    PlayerColor pColor = PlayerSkinData.Instance != null ? PlayerSkinData.Instance.GetPlayerSkin(pRef) : PlayerColor.White;
                    visualComp.Initialize(pRef, logicPlayer.PlayerName.ToString(), pColor, originalPlayerPrefab);

                    if (i == 0) visualGo.transform.localScale = Vector3.one * 1.2f;
                    else visualGo.transform.localScale = Vector3.one * 0.85f;
                }
            }
        }

        private void UpdateVisualStates()
        {
            if (_playerVisuals.Count == 0) return;

            foreach (var kvp in _playerVisuals)
            {
                PlayerRef pRef = kvp.Key;
                CardGamePlayerVisual visual = kvp.Value;
                CardPlayer logicPlayer = CardGameManager.Instance.GetCardPlayer(pRef);

                if (logicPlayer == null) continue;

                if (logicPlayer.IsDead && !visual.isDead)
                {
                    visual.MarkAsDead();
                }

                if (!visual.isDead)
                {
                    visual.UpdateCardCount(logicPlayer.handCardIds.Count);
                    visual.SetTurnGlow(CardGameManager.Instance.IsMyTurn(pRef));
                }
            }
        }

        public void StartTargetingMode(System.Action<PlayerRef> onTargetSelected, System.Action onCancel)
        {
            EndTargetingMode(); // Ensure clean state

            // Create invisible fullscreen button to cancel
            GameObject canvasGo = GameObject.Find("BoomDogBackgroundCanvas");
            if (canvasGo != null)
            {
                if (canvasGo.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }

                _targetingBlocker = new GameObject("TargetingBlocker");
                _targetingBlocker.transform.SetParent(canvasGo.transform, false);
                _targetingBlocker.transform.SetAsLastSibling(); // Ensure it's in front of background but behind everything else

                var img = _targetingBlocker.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(0, 0, 0, 0); // Transparent

                var rect = _targetingBlocker.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var btn = _targetingBlocker.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(() => {
                    EndTargetingMode();
                    if (onCancel != null) onCancel.Invoke();
                });
            }

            PlayerRef localRef = CardGameManager.Instance.Runner.LocalPlayer;

            foreach (var kvp in _playerVisuals)
            {
                PlayerRef pRef = kvp.Key;
                CardGamePlayerVisual visual = kvp.Value;

                if (pRef != localRef && !visual.isDead)
                {
                    visual.SetTargetingState(true, () => {
                        EndTargetingMode();
                        if (onTargetSelected != null) onTargetSelected.Invoke(pRef);
                    });
                }
            }
        }

        public void EndTargetingMode()
        {
            if (_targetingBlocker != null)
            {
                Destroy(_targetingBlocker);
            }

            foreach (var kvp in _playerVisuals)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetTargetingState(false, null);
                }
            }
        }
    }
}
