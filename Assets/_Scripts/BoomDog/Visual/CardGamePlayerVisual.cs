using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;
using BoomDog.Core;

namespace BoomDog.Visual
{
    public class CardGamePlayerVisual : MonoBehaviour
    {
        public PlayerRef OwnerRef { get; private set; }
        
        [Header("UI Elements")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI cardCountText;
        public GameObject turnGlow;
        public Transform avatarRoot;
        
        [Header("Targeting Mode")]
        public GameObject targetingArrow;
        public UnityEngine.UI.Button targetButton;

        [Header("Status")]
        public bool isDead = false;

        private GameObject _avatarInstance;

        public void Initialize(PlayerRef pRef, string playerName, PlayerColor skinColor, GameObject originalPlayerPrefab)
        {
            OwnerRef = pRef;
            
            // Auto-create UI components if not assigned
            if (nameText == null || cardCountText == null)
            {
                // Create a World Space Canvas to hold the texts
                GameObject canvasGo = new GameObject("PlayerUICanvas");
                canvasGo.transform.SetParent(transform, false);
                canvasGo.transform.localPosition = Vector3.zero;
                
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>(); // QUAN TRỌNG: Để click được button!
                
                // Scale down the canvas so fonts don't need to be tiny
                RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(200, 200);
                canvasGo.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                
                // Make the canvas face the camera
                if (Camera.main != null)
                {
                    canvasGo.transform.rotation = Camera.main.transform.rotation;
                }

                if (nameText == null)
                {
                    GameObject nameGo = new GameObject("NameText");
                    nameGo.transform.SetParent(canvasGo.transform, false);
                    nameGo.transform.localPosition = new Vector3(0, 150f, 0); // Above head (scaled coordinates)
                    
                    nameText = nameGo.AddComponent<TextMeshProUGUI>();
                    nameText.fontSize = 24;
                    nameText.alignment = TextAlignmentOptions.Center;
                    nameText.color = Color.white;
                    nameText.outlineColor = Color.black;
                    nameText.outlineWidth = 0.2f;
                    nameText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
                }
                
                if (cardCountText == null)
                {
                    GameObject countGo = new GameObject("CardCountText");
                    countGo.transform.SetParent(canvasGo.transform, false);
                    countGo.transform.localPosition = new Vector3(0, -50f, 0); // Below avatar
                    
                    cardCountText = countGo.AddComponent<TextMeshProUGUI>();
                    cardCountText.fontSize = 20;
                    cardCountText.alignment = TextAlignmentOptions.Center;
                    cardCountText.color = Color.yellow;
                    cardCountText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
                }
            }
            nameText.text = playerName;
            
            // Generate Targeting Components
            if (targetingArrow == null)
            {
                // Mũi tên (tam giác ngược)
                targetingArrow = new GameObject("TargetArrow");
                targetingArrow.transform.SetParent(nameText.transform.parent, false);
                targetingArrow.transform.localPosition = new Vector3(0, 300f, 0); // Above Name
                
                var arrowImg = targetingArrow.AddComponent<UnityEngine.UI.Image>();
                arrowImg.color = Color.red;
                
                var arrowRect = targetingArrow.GetComponent<RectTransform>();
                arrowRect.sizeDelta = new Vector2(50, 80);
                // Rotate 180 to make it point down
                arrowRect.localRotation = Quaternion.Euler(0, 0, 180);
                
                targetingArrow.SetActive(false);
            }

            if (targetButton == null)
            {
                // Nút bấm tàng hình bọc toàn bộ nhân vật
                GameObject btnGo = new GameObject("ClickTargetButton");
                btnGo.transform.SetParent(nameText.transform.parent, false);
                btnGo.transform.localPosition = new Vector3(0, 50f, 0);
                
                var btnImg = btnGo.AddComponent<UnityEngine.UI.Image>();
                btnImg.color = new Color(0, 0, 0, 0); // Transparent
                
                var btnRect = btnGo.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(300, 400); // Cover avatar
                
                targetButton = btnGo.AddComponent<UnityEngine.UI.Button>();
                targetButton.gameObject.SetActive(false);
            }

            if (turnGlow == null)
            {
                // Create a simple glowing yellow cylinder behind the player
                turnGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                turnGlow.transform.SetParent(transform, false);
                turnGlow.transform.localPosition = new Vector3(0, 0, 0);
                turnGlow.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);
                Destroy(turnGlow.GetComponent<Collider>());
                
                Renderer r = turnGlow.GetComponent<Renderer>();
                if (r != null)
                {
                    Material glowMat = new Material(Shader.Find("Unlit/Color"));
                    glowMat.color = new Color(1f, 1f, 0f, 0.5f); // Semi-transparent yellow
                    r.material = glowMat;
                }
                turnGlow.SetActive(false);
            }

            if (avatarRoot == null)
            {
                GameObject rootGo = new GameObject("AvatarRoot");
                rootGo.transform.SetParent(transform, false);
                rootGo.transform.localPosition = Vector3.zero;
                avatarRoot = rootGo.transform;
            }

            // Instantiate visual dummy
            if (originalPlayerPrefab != null && avatarRoot != null)
            {
                _avatarInstance = Instantiate(originalPlayerPrefab, avatarRoot);
                _avatarInstance.transform.localPosition = Vector3.zero;
                _avatarInstance.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face the camera
                
                // Strip network components safely
                var no = _avatarInstance.GetComponent<NetworkObject>();
                if (no != null) Destroy(no);
                
                var nt = _avatarInstance.GetComponent<NetworkTransform>();
                if (nt != null) Destroy(nt);

                // Disable all MonoBehaviours to prevent game logic from running (like PlayerMovement, etc.)
                foreach (var comp in _avatarInstance.GetComponentsInChildren<MonoBehaviour>())
                {
                    comp.enabled = false;
                }
                
                // Apply Skin manually because LoadPlayerSkin is disabled
                if (PlayerSkinData.Instance != null)
                {
                    Material targetMat = PlayerSkinData.Instance.GetMaterial(skinColor);
                    if (targetMat != null)
                    {
                        var meshRenderers = _avatarInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        foreach(var mr in meshRenderers)
                        {
                            mr.material = targetMat;
                        }
                    }
                }
            }
        }

        public void UpdateCardCount(int count)
        {
            if (cardCountText != null)
                cardCountText.text = "Bài: " + count.ToString();
        }

        private void Update()
        {
            if (CardGameManager.Instance != null && !isDead)
            {
                CardPlayer cp = CardGameManager.Instance.GetCardPlayer(OwnerRef);
                if (cp != null)
                {
                    UpdateCardCount(cp.handCardIds.Count);
                }
            }

            if (targetingArrow != null && targetingArrow.activeSelf)
            {
                float yPos = 300f + Mathf.Sin(Time.time * 5f) * 20f;
                targetingArrow.transform.localPosition = new Vector3(0, yPos, 0);
            }
        }

        public void SetTargetingState(bool active, System.Action onClick)
        {
            if (isDead) return;

            if (targetingArrow != null) targetingArrow.SetActive(active);
            
            if (targetButton != null)
            {
                targetButton.gameObject.SetActive(active);
                targetButton.onClick.RemoveAllListeners();
                if (active && onClick != null)
                {
                    targetButton.onClick.AddListener(() => onClick.Invoke());
                }
            }
        }

        public void SetTurnGlow(bool isTurn)
        {
            if (isDead) isTurn = false;
            if (turnGlow != null) turnGlow.SetActive(isTurn);
        }

        public void MarkAsDead()
        {
            isDead = true;
            SetTurnGlow(false);
            if (_avatarInstance != null)
            {
                // Simple death visual: rotate 90 degrees
                _avatarInstance.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
            if (cardCountText != null) cardCountText.text = "DEAD";
        }
    }
}
