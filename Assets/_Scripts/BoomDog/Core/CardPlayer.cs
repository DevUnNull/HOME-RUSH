using Fusion;
using UnityEngine;
using BoomDog.Data;
using BoomDog.Visual;
using System.Collections.Generic;
using DG.Tweening;

namespace BoomDog.Core
{
    public class CardPlayer : NetworkBehaviour
    {
        public static CardPlayer Local { get; private set; }

        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public NetworkBool IsDead { get; set; }

        public List<string> handCardIds = new List<string>();

        private RectTransform _mockHandRect;
        private float _originalMockHandY;
        private bool _isMyTurnRaised = true; // Mặc định là đang giơ lên để tý hạ xuống
        private int _lastChildCount = -1;

        [Header("UI Prefab (Non-Networked)")]
        public GameObject cardUIPrefab; // Kéo MockCardPrefab vào đây

        public override void Spawned()
        {
            // Khi ai đó spawn thành công, tự add vào danh sách của GameManager
            if (!CardGameManager.Instance.spawnedCardPlayers.Contains(this))
                CardGameManager.Instance.spawnedCardPlayers.Add(this);

            if (HasStateAuthority) // Trong Shared Mode, người spawn ra object sẽ là StateAuthority
            {
                Local = this;
                Debug.Log("Local Player đã Spawn thành công trên mạng!");
                CreateDrawPileUI(); // Tự tạo UI Nọc Bài
                
                GameObject mockHandGO = GameObject.Find("MockHand_TestOnly");
                if (mockHandGO != null)
                {
                    _mockHandRect = mockHandGO.GetComponent<RectTransform>();
                    if (_mockHandRect != null) _originalMockHandY = _mockHandRect.anchoredPosition.y;
                }
            }
        }

        private TMPro.TextMeshProUGUI _deckCountText;

        private void CreateDrawPileUI()
        {
            if (cardUIPrefab == null) return;
            GameObject canvas = GameObject.Find("MainCanvas");
            if (canvas != null)
            {
                Transform drawPile = canvas.transform.Find("DrawPile");
                if (drawPile == null)
                {
                    GameObject dp = new GameObject("DrawPile");
                    dp.transform.SetParent(canvas.transform, false);
                    dp.transform.localPosition = new Vector3(-250f, 0, 0); // Nằm lệch sang trái
                    
                    GameObject deckVisual = Instantiate(cardUIPrefab, dp.transform);
                    deckVisual.transform.localPosition = Vector3.zero;
                    deckVisual.transform.localScale = Vector3.one * 0.8f; // Thu nhỏ lại một chút
                    
                    var visual = deckVisual.GetComponent<CardVisual>();
                    visual.SetFaceUp(false); // Úp bài
                    visual.MarkAsPlayed(); // KHÓA HOVER
                    
                    // Thêm Text đếm số bài
                    GameObject countTextGo = new GameObject("DeckCountText");
                    countTextGo.transform.SetParent(dp.transform, false);
                    countTextGo.transform.localPosition = new Vector3(0, -110f, 0); // Nằm dưới xấp bài một chút
                    _deckCountText = countTextGo.AddComponent<TMPro.TextMeshProUGUI>();
                    _deckCountText.text = "0";
                    _deckCountText.fontSize = 40;
                    _deckCountText.alignment = TMPro.TextAlignmentOptions.Center;
                    _deckCountText.color = Color.white;
                    _deckCountText.fontStyle = TMPro.FontStyles.Bold;
                    
                    // Thêm nút bấm để bốc bài
                    var btn = deckVisual.AddComponent<UnityEngine.UI.Button>();
                    btn.onClick.AddListener(() => {
                        if (CardGameManager.Instance.IsMyTurn(Object.StateAuthority))
                        {
                            RPC_RequestDrawCard();
                        }
                        else
                        {
                            Debug.Log("Chưa tới lượt của bạn!");
                        }
                    });
                }
            }
        }



        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RequestDrawCard()
        {
            // Chủ phòng sẽ bốc 1 lá ngẫu nhiên từ DeckManager và gửi cho người xin bốc
            if (CardGameManager.Instance.HasStateAuthority)
            {
                if (DeckManager.Instance != null)
                {
                    string cardId = DeckManager.Instance.DrawCard();
                    if (!string.IsNullOrEmpty(cardId))
                    {
                        RPC_ReceiveCard(cardId);
                        
                        // Bốc bài xong là kết thúc lượt
                        CardGameManager.Instance.NextTurn(); 
                    }
                }
            }
        }

        public override void Render()
        {
            // Cập nhật liên tục số bài còn lại trên mọi máy (Render chạy mỗi frame trên Client)
            if (_deckCountText != null && DeckManager.Instance != null && CardGameManager.Instance != null && CardGameManager.Instance.IsGameStarted)
            {
                _deckCountText.text = DeckManager.Instance.CardsInDeck.ToString();
                
                // Cập nhật quạt bài
                if (_mockHandRect != null && _mockHandRect.childCount != _lastChildCount)
                {
                    _lastChildCount = _mockHandRect.childCount;
                    UpdateHandFanLayout();
                }

                // Hiệu ứng nâng bài lên khi tới lượt (chỉ áp dụng cho máy Local)
                if (_mockHandRect != null && HasStateAuthority)
                {
                    bool isMyTurn = CardGameManager.Instance.IsMyTurn(Object.StateAuthority);
                    if (isMyTurn && !_isMyTurnRaised)
                    {
                        _isMyTurnRaised = true;
                        _mockHandRect.DOAnchorPosY(_originalMockHandY, 0.3f).SetEase(DG.Tweening.Ease.OutBack);
                    }
                    else if (!isMyTurn && _isMyTurnRaised)
                    {
                        _isMyTurnRaised = false;
                        _mockHandRect.DOAnchorPosY(_originalMockHandY - 60f, 0.3f).SetEase(DG.Tweening.Ease.InOutQuad);
                    }
                }
            }
        }

        private void UpdateHandFanLayout()
        {
            if (_mockHandRect == null) return;
            int childCount = _mockHandRect.childCount;
            if (childCount == 0) return;

            float maxAngle = 10f; // Hai bên nghiêng tối đa 10 độ
            float maxYDrop = -15f; // Hai bên thấp xuống 15 pixel

            for (int i = 0; i < childCount; i++)
            {
                Transform child = _mockHandRect.GetChild(i);
                var cardVisual = child.GetComponent<BoomDog.Visual.CardVisual>();
                
                if (cardVisual != null)
                {
                    float normalizedPos = (childCount <= 1) ? 0 : ((float)i / (childCount - 1)) * 2f - 1f; 
                    float angle = -normalizedPos * maxAngle;
                    float yDrop = (normalizedPos * normalizedPos) * maxYDrop;
                    
                    cardVisual.SetFanOffset(yDrop, angle);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_PlayCard(string cardId)
        {
            if (IsDead) return;
            
            // Xóa bài trên tay của mình
            if (HasStateAuthority && handCardIds.Contains(cardId)) handCardIds.Remove(cardId);
            
            // Thông báo hiển thị log
            CardData data = DeckManager.Instance.GetCardDataById(cardId);
            string cName = data != null ? data.cardName : cardId;
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] đã đánh: {cName}");
            }

            // Render hiệu ứng lá bài bay vào Mộ Bài (Discard Pile) cho TẤT CẢ mọi người cùng xem
            if (cardUIPrefab != null)
            {
                GameObject canvas = GameObject.Find("MainCanvas");
                if (canvas != null)
                {
                    Transform discardPile = canvas.transform.Find("DiscardPile");
                    if (discardPile == null)
                    {
                        GameObject dp = new GameObject("DiscardPile");
                        dp.transform.SetParent(canvas.transform, false);
                        dp.transform.localPosition = new Vector3(250f, 0f, 0f); // Lệch sang phải 1 chút để chừa chỗ cho bộ bài nọc
                        discardPile = dp.transform;
                    }

                    GameObject fakeCard = Instantiate(cardUIPrefab, canvas.transform);
                    fakeCard.transform.position = transform.position; // Vị trí giả từ Avatar
                    
                    if (HasStateAuthority) 
                    {
                        // Nếu là người đánh, thì vị trí bắt đầu nên là giữa màn hình phía dưới (giả lập vứt từ tay)
                        fakeCard.transform.position = new Vector3(Screen.width / 2f, Screen.height * 0.2f, 0);
                    }

                    var visual = fakeCard.GetComponent<CardVisual>();
                    if (data != null) visual.Initialize(data, false);
                    visual.MarkAsPlayed(); // KHÓA HOVER! Không cho nảy lên nữa
                    
                    visual.AnimatePlayCard(discardPile.position, () => {
                        // Xóa các lá bài cũ trong Mộ
                        foreach (Transform child in discardPile) Destroy(child.gameObject);
                        
                        // Đặt lá bài mới vào Mộ, thu nhỏ lại
                        fakeCard.transform.SetParent(discardPile, true);
                        fakeCard.transform.localPosition = Vector3.zero;
                        fakeCard.transform.localScale = Vector3.one * 0.8f; // Thu nhỏ bằng 1/2 so với trước
                    });
                }
            }
            
            // Đánh dấu đã đánh 1 lá trong lượt này
            if (CardGameManager.Instance.HasStateAuthority)
            {
                CardGameManager.Instance.HasPlayedCardThisTurn = true;
            }
        }

        // Người chia bài (All) sẽ gửi bài thẳng xuống cho người sở hữu lá bài (StateAuthority)
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ReceiveCard(string cardId)
        {
            handCardIds.Add(cardId);
            CardData data = DeckManager.Instance.GetCardDataById(cardId);
            
            // QUAY LẠI TÌM TRỰC TIẾP TRONG SCENE BẰNG TÊN (Vì UI nằm ngoài Scene chứ không nằm trong Prefab)
            GameObject mockHandGO = GameObject.Find("MockHand_TestOnly");
            Transform mockHand = mockHandGO != null ? mockHandGO.transform : null;

            Debug.Log($"[Client ReceiveCard] cardId={cardId}, timThayData={(data != null)}, timThayHand={(mockHand != null)}");

            if (mockHand != null && cardUIPrefab != null && data != null)
            {
                // ÉP BUỘC TẮT TÍNH NĂNG PHÓNG TO BÀI CỦA LAYOUT GROUP Ở RUNTIME
                var hlg = mockHand.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = false;
                    hlg.spacing = -55f; // Xếp đè bài lên nhau y như ảnh
                }
                // Ép buộc thu nhỏ cả khung chứa bài
                mockHand.localScale = new Vector3(0.65f, 0.65f, 1f);

                GameObject newCard = Instantiate(cardUIPrefab, mockHand);
                newCard.name = "Card_" + data.cardName;
                newCard.GetComponent<CardVisual>().Initialize(data, true);
                
                // Hiệu ứng nhẹ khi bốc bài
                newCard.transform.localScale = Vector3.zero;
                newCard.GetComponent<CardVisual>().AnimateDrawCard();
            }
        }
    }
}
