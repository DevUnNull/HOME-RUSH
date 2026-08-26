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

        public List<CardVisual> selectedCards = new List<CardVisual>();
        private List<CardVisual> _pendingPlayedCards = new List<CardVisual>();
        private bool _isWaitingForServer = false;

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
            if (CardGameManager.Instance.HasStateAuthority)
            {
                // Validate lượt trên Host để tránh hack hoặc lỗi đồng bộ
                if (!CardGameManager.Instance.IsMyTurn(Object.StateAuthority)) return;
                
                if (CardGameManager.Instance.HasDrawnThisTurn) return; // Chống spam Draw
                
                if (DeckManager.Instance != null)
                {
                    string cardId = DeckManager.Instance.DrawCard();
                    if (!string.IsNullOrEmpty(cardId))
                    {
                        CardGameManager.Instance.HasDrawnThisTurn = true;
                        CardData drawnData = DeckManager.Instance.GetCardDataById(cardId);
                        
                        if (drawnData != null && drawnData.cardType == CardType.ExplodingDog)
                        {
                            string defuseId = "";
                            foreach(string hId in handCardIds)
                            {
                                var d = DeckManager.Instance.GetCardDataById(hId);
                                if (d != null && d.cardType == CardType.Defuse) { defuseId = hId; break; }
                            }
                            
                            if (!string.IsNullOrEmpty(defuseId))
                            {
                                RPC_ConfirmPlayCards(defuseId); // Vứt lá Defuse và báo cho mọi người
                                DeckManager.Instance.ReturnCardToDrawPileRandomly(cardId);
                                
                                if (GameUIManager.Instance != null)
                                    GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] đã dùng Defuse!");
                                    
                                CardGameManager.Instance.FinishCurrentTurn();
                            }
                            else
                            {
                                if (GameUIManager.Instance != null)
                                    GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] đã NỔ TUNG!");
                                    
                                CardGameManager.Instance.HandlePlayerDeath(Object.StateAuthority);
                            }
                        }
                        else
                        {
                            RPC_ReceiveCard(cardId);
                            CardGameManager.Instance.FinishCurrentTurn();
                        }
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

        public void HandleCardClick(CardVisual clickedCard)
        {
            if (_isWaitingForServer) return;

            if (selectedCards.Contains(clickedCard))
            {
                PlaySelectedCards();
                return;
            }

            if (clickedCard.CurrentData.cardType != CardType.CatCard)
            {
                ClearSelection();
                selectedCards.Add(clickedCard);
                clickedCard.SelectCard();
            }
            else // CatCard
            {
                if (selectedCards.Count > 0)
                {
                    if (selectedCards[0].CurrentData.cardType == CardType.CatCard &&
                        selectedCards[0].CurrentData.id == clickedCard.CurrentData.id)
                    {
                        if (selectedCards.Count >= 3)
                        {
                            Debug.Log("Không thể chọn quá 3 lá Cat Card cùng lúc!");
                            return; // Block 4th card
                        }
                        selectedCards.Add(clickedCard);
                        clickedCard.SelectCard();
                    }
                    else
                    {
                        ClearSelection();
                        selectedCards.Add(clickedCard);
                        clickedCard.SelectCard();
                    }
                }
                else
                {
                    selectedCards.Add(clickedCard);
                    clickedCard.SelectCard();
                }
            }
        }

        public void ClearSelection()
        {
            foreach (var c in selectedCards) c.DeselectCard();
            selectedCards.Clear();
        }

        public void PlaySelectedCards()
        {
            if (selectedCards.Count == 0) return;

            string[] ids = new string[selectedCards.Count];
            for(int i = 0; i < selectedCards.Count; i++) ids[i] = selectedCards[i].CurrentData.id;
            string joinedIds = string.Join(",", ids);

            if (selectedCards[0].CurrentData.cardType == CardType.CatCard)
            {
                if (selectedCards.Count == 2)
                {
                    CardGameVisualManager.Instance.StartTargetingMode(
                        (targetRef) => { ExecutePairCombo(joinedIds, targetRef); },
                        () => { ClearSelection(); }
                    );
                    return;
                }
                else if (selectedCards.Count == 3)
                {
                    CardGameVisualManager.Instance.StartTargetingMode(
                        (targetRef) => { OpenTargetCardUI(joinedIds, targetRef); },
                        () => { ClearSelection(); }
                    );
                    return;
                }
                else // count == 1
                {
                    Debug.Log("Cần ít nhất 2 lá Cat Card giống nhau để đánh!");
                    ClearSelection();
                    return;
                }
            }

            // Normal play (1 Cat Card or functional card)
            foreach (var c in selectedCards) 
            {
                c.MarkAsPlayed();
                _pendingPlayedCards.Add(c);
            }
            selectedCards.Clear();
            
            _isWaitingForServer = true; // Khóa UI trong lúc chờ Server

            // Xin phép Host đánh bài
            RPC_RequestPlayCards(joinedIds);
        }

        private GameObject _activeTargetUI;

        private void OpenTargetCardUI(string comboIds, PlayerRef targetRef)
        {
            if (_activeTargetUI != null) Destroy(_activeTargetUI);
            
            GameObject canvas = GameObject.Find("MainCanvas");
            if (canvas == null) return;
            
            _activeTargetUI = new GameObject("TargetCardUI");
            _activeTargetUI.transform.SetParent(canvas.transform, false);
            
            var bg = _activeTargetUI.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0, 0, 0, 0.9f);
            var bgRect = _activeTargetUI.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Bấm ra ngoài (nền đen) để hủy
            var bgBtn = _activeTargetUI.AddComponent<UnityEngine.UI.Button>();
            bgBtn.onClick.AddListener(() => {
                Destroy(_activeTargetUI);
                ClearSelection();
            });
            
            var title = new GameObject("Title");
            title.transform.SetParent(_activeTargetUI.transform, false);
            var titleText = title.AddComponent<TMPro.TextMeshProUGUI>();
            titleText.text = "Chọn lá bài muốn cướp:";
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 20;
            titleText.fontSizeMax = 50;
            titleText.alignment = TMPro.TextAlignmentOptions.Center;
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 400);

            // Tạo Scroll View đơn giản hoặc Grid Layout cho danh sách bài
            GameObject grid = new GameObject("Grid");
            grid.transform.SetParent(_activeTargetUI.transform, false);
            var gridRect = grid.AddComponent<RectTransform>();
            gridRect.sizeDelta = new Vector2(1000, 800);
            gridRect.anchoredPosition = new Vector2(0, -50);
            
            var layout = grid.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            layout.cellSize = new Vector2(300, 100);
            layout.spacing = new Vector2(20, 20);
            layout.childAlignment = TextAnchor.UpperCenter;

            foreach (var cardTemplate in DeckManager.Instance.allAvailableCards)
            {
                if (cardTemplate == null) continue;
                if (cardTemplate.cardType == CardType.ExplodingDog) continue; // Không ai muốn xin con chó nổ
                
                GameObject btn = new GameObject("Btn_Card_" + cardTemplate.id);
                btn.transform.SetParent(grid.transform, false);
                var btnImg = btn.AddComponent<UnityEngine.UI.Image>();
                btnImg.color = Color.white;
                
                var txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btn.transform, false);
                var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
                txt.text = cardTemplate.cardName;
                txt.color = Color.black;
                txt.enableAutoSizing = true;
                txt.fontSizeMin = 10;
                txt.fontSizeMax = 30;
                txt.alignment = TMPro.TextAlignmentOptions.Center;
                txt.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                txt.GetComponent<RectTransform>().anchorMax = Vector2.one;
                txt.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                txt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                
                var buttonComp = btn.AddComponent<UnityEngine.UI.Button>();
                string reqCardId = cardTemplate.id;
                buttonComp.onClick.AddListener(() => {
                    Destroy(_activeTargetUI);
                    ExecuteThreeOfAKindCombo(comboIds, targetRef, reqCardId);
                });
            }
        }

        private void ExecutePairCombo(string comboIds, PlayerRef targetRef)
        {
            foreach (var c in selectedCards) 
            {
                c.MarkAsPlayed();
                _pendingPlayedCards.Add(c);
            }
            selectedCards.Clear();
            _isWaitingForServer = true;
            RPC_RequestPlayPair(comboIds, targetRef);
        }

        private void ExecuteThreeOfAKindCombo(string comboIds, PlayerRef targetRef, string requestedCardId)
        {
            foreach (var c in selectedCards) 
            {
                c.MarkAsPlayed();
                _pendingPlayedCards.Add(c);
            }
            selectedCards.Clear();
            _isWaitingForServer = true;
            RPC_RequestPlayThreeOfAKind(comboIds, targetRef, requestedCardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RejectPlayCards()
        {
            _isWaitingForServer = false;
            foreach (var c in _pendingPlayedCards)
            {
                if (c != null) 
                {
                    c.UnmarkAsPlayed();
                    c.DeselectCard();
                }
            }
            _pendingPlayedCards.Clear();
            ClearSelection();
            Debug.Log("Server từ chối lệnh đánh bài (Có thể do hết lượt hoặc lỗi mạng).");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RequestPlayCards(string commaSeparatedIds)
        {
            if (CardGameManager.Instance.HasStateAuthority) // Host/Master Client xử lý
            {
                string[] ids = commaSeparatedIds.Split(',');
                
                // Validate lượt
                if (!CardGameManager.Instance.IsMyTurn(Object.StateAuthority)) 
                {
                    RPC_RejectPlayCards();
                    return;
                }
                
                // Validate bài có trong tay không
                bool hasAllCards = true;
                List<string> tempHand = new List<string>(handCardIds);
                foreach(var id in ids)
                {
                    if (tempHand.Contains(id)) tempHand.Remove(id);
                    else { hasAllCards = false; break; }
                }

                if (!hasAllCards) 
                {
                    RPC_RejectPlayCards();
                    return;
                }

                // Hợp lệ -> Báo toàn mạng chạy hiệu ứng và xóa data
                RPC_ConfirmPlayCards(commaSeparatedIds);
                
                if (ids.Length > 0)
                {
                    CardData firstCard = DeckManager.Instance.GetCardDataById(ids[0]);
                    if (firstCard != null)
                    {
                        if (firstCard.cardType == CardType.Skip)
                        {
                            CardGameManager.Instance.FinishCurrentTurn();
                        }
                        else if (firstCard.cardType == CardType.Attack)
                        {
                            CardGameManager.Instance.HandleAttack();
                        }
                    }
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_ConfirmPlayCards(string commaSeparatedIds)
        {
            if (IsDead) return;
            string[] ids = commaSeparatedIds.Split(',');

            // Trừ data trên tất cả các máy để Host có thể validate chính xác
            foreach(var id in ids) 
            {
                if (handCardIds.Contains(id)) handCardIds.Remove(id);
            }
            
            // Thông báo hiển thị log
            if (GameUIManager.Instance != null && ids.Length > 0)
            {
                CardData data = DeckManager.Instance.GetCardDataById(ids[0]);
                string cName = data != null ? data.cardName : ids[0];
                string qty = ids.Length > 1 ? $" x{ids.Length}" : "";
                GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] đã đánh: {cName}{qty}");
            }

            // Animation
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
                        dp.transform.localPosition = new Vector3(250f, 0f, 0f); 
                        discardPile = dp.transform;
                    }

                    // Xóa CHÍNH XÁC các UI Card đã click trên tay người đánh
                    if (HasStateAuthority)
                    {
                        _isWaitingForServer = false;
                        
                        List<string> idsList = new List<string>(ids);
                        List<CardVisual> toRemove = new List<CardVisual>();
                        
                        foreach (var pending in _pendingPlayedCards)
                        {
                            if (pending != null && idsList.Contains(pending.CurrentData.id))
                            {
                                idsList.Remove(pending.CurrentData.id);
                                toRemove.Add(pending);
                                Destroy(pending.gameObject);
                            }
                        }
                        
                        foreach (var r in toRemove) _pendingPlayedCards.Remove(r);
                    }

                    // Quăng Fake Cards vào Mộ
                    for (int i = 0; i < ids.Length; i++)
                    {
                        string id = ids[i];
                        CardData data = DeckManager.Instance.GetCardDataById(id);
                        
                        GameObject fakeCard = Instantiate(cardUIPrefab, canvas.transform);
                        fakeCard.transform.position = transform.position; 
                        
                        if (HasStateAuthority) 
                        {
                            fakeCard.transform.position = new Vector3(Screen.width / 2f + (i * 30f), Screen.height * 0.2f, 0);
                        }

                        var visual = fakeCard.GetComponent<CardVisual>();
                        if (data != null) visual.Initialize(data, false);
                        visual.MarkAsPlayed();
                        
                        visual.AnimatePlayCard(discardPile.position, () => {
                            foreach (Transform child in discardPile) Destroy(child.gameObject);
                            
                            fakeCard.transform.SetParent(discardPile, true);
                            fakeCard.transform.localPosition = Vector3.zero;
                            fakeCard.transform.localScale = Vector3.one * 0.8f; 
                        });
                    }
                }
            }
        }

        // Gửi bài cho tất cả mọi người để Host đồng bộ được handCardIds
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_ReceiveCard(string cardId)
        {
            handCardIds.Add(cardId);
            
            // Chỉ người sở hữu mới tạo UI
            if (HasStateAuthority)
            {
                CardData data = DeckManager.Instance.GetCardDataById(cardId);
                
                // QUAY LẠI TÌM TRỰC TIẾP TRONG SCENE BẰNG TÊN
                GameObject mockHandGO = GameObject.Find("MockHand_TestOnly");
                Transform mockHand = mockHandGO != null ? mockHandGO.transform : null;

                if (mockHand != null && cardUIPrefab != null && data != null)
                {
                    // ÉP BUỘC TẮT TÍNH NĂNG PHÓNG TO BÀI CỦA LAYOUT GROUP Ở RUNTIME
                    var hlg = mockHand.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.childControlWidth = false;
                        hlg.childControlHeight = false;
                        hlg.childForceExpandWidth = false; // CRITICAL: Stop Unity from spreading them out
                        hlg.childForceExpandHeight = false;
                        hlg.spacing = -80f; // Half overlap
                    }
                    // Scale down to a reasonable size
                    mockHand.localScale = new Vector3(1.3f, 1.3f, 1f);

                    GameObject newCard = Instantiate(cardUIPrefab, mockHand);
                    newCard.name = "Card_" + data.cardName;
                    newCard.GetComponent<CardVisual>().Initialize(data, true);
                    
                    // Hiệu ứng nhẹ khi bốc bài
                    newCard.transform.localScale = Vector3.zero;
                    newCard.GetComponent<CardVisual>().AnimateDrawCard();
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_LoseCard(string cardId)
        {
            if (handCardIds.Contains(cardId)) handCardIds.Remove(cardId);
            
            if (HasStateAuthority)
            {
                GameObject mockHandGO = GameObject.Find("MockHand_TestOnly");
                if (mockHandGO != null)
                {
                    foreach (Transform child in mockHandGO.transform)
                    {
                        var cv = child.GetComponent<CardVisual>();
                        if (cv != null && cv.CurrentData != null && cv.CurrentData.id == cardId)
                        {
                            Destroy(child.gameObject);
                            break;
                        }
                    }
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_LogStolen(PlayerRef victim, NetworkBool success, string requestedCardName = "")
        {
            if (GameUIManager.Instance != null)
            {
                if (success)
                {
                    if (string.IsNullOrEmpty(requestedCardName))
                        GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] cướp ngẫu nhiên 1 lá của [P{victim.PlayerId}]!");
                    else
                        GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] cướp thành công {requestedCardName} của [P{victim.PlayerId}]!");
                }
                else
                {
                    if (string.IsNullOrEmpty(requestedCardName))
                        GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] cố cướp bài nhưng [P{victim.PlayerId}] đã hết bài!");
                    else
                        GameUIManager.Instance.AddLog($"[P{Object.StateAuthority.PlayerId}] đòi {requestedCardName} nhưng [P{victim.PlayerId}] không có!");
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RequestPlayPair(string comboIds, PlayerRef targetRef)
        {
            if (CardGameManager.Instance.HasStateAuthority)
            {
                if (!CardGameManager.Instance.IsMyTurn(Object.StateAuthority)) { RPC_RejectPlayCards(); return; }
                
                string[] ids = comboIds.Split(',');
                if (ids.Length != 2) { RPC_RejectPlayCards(); return; }

                bool hasAllCards = true;
                List<string> tempHand = new List<string>(handCardIds);
                foreach(var id in ids)
                {
                    if (tempHand.Contains(id)) tempHand.Remove(id);
                    else { hasAllCards = false; break; }
                }
                if (!hasAllCards) { RPC_RejectPlayCards(); return; }

                CardPlayer targetPlayer = CardGameManager.Instance.GetCardPlayer(targetRef);
                if (targetPlayer == null || targetPlayer.IsDead || targetRef == Object.StateAuthority)
                {
                    RPC_RejectPlayCards(); return;
                }

                // Discard combo
                RPC_ConfirmPlayCards(comboIds);

                if (targetPlayer.handCardIds.Count > 0)
                {
                    int randIndex = Random.Range(0, targetPlayer.handCardIds.Count);
                    string stolenCardId = targetPlayer.handCardIds[randIndex];
                    
                    targetPlayer.RPC_LoseCard(stolenCardId);
                    this.RPC_ReceiveCard(stolenCardId);
                    
                    RPC_LogStolen(targetRef, true, "");
                }
                else
                {
                    RPC_LogStolen(targetRef, false, "");
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RequestPlayThreeOfAKind(string comboIds, PlayerRef targetRef, string requestedCardId)
        {
            if (CardGameManager.Instance.HasStateAuthority)
            {
                if (!CardGameManager.Instance.IsMyTurn(Object.StateAuthority)) { RPC_RejectPlayCards(); return; }
                
                string[] ids = comboIds.Split(',');
                if (ids.Length != 3) { RPC_RejectPlayCards(); return; }

                bool hasAllCards = true;
                List<string> tempHand = new List<string>(handCardIds);
                foreach(var id in ids)
                {
                    if (tempHand.Contains(id)) tempHand.Remove(id);
                    else { hasAllCards = false; break; }
                }
                if (!hasAllCards) { RPC_RejectPlayCards(); return; }

                CardPlayer targetPlayer = CardGameManager.Instance.GetCardPlayer(targetRef);
                if (targetPlayer == null || targetPlayer.IsDead || targetRef == Object.StateAuthority)
                {
                    RPC_RejectPlayCards(); return;
                }

                // Discard combo
                RPC_ConfirmPlayCards(comboIds);

                CardData reqData = DeckManager.Instance.GetCardDataById(requestedCardId);
                string reqName = reqData != null ? reqData.cardName : requestedCardId;

                string stolenCardId = "";
                foreach (string hId in targetPlayer.handCardIds)
                {
                    CardData d = DeckManager.Instance.GetCardDataById(hId);
                    if (d != null && d.id == requestedCardId)
                    {
                        stolenCardId = hId;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(stolenCardId))
                {
                    targetPlayer.RPC_LoseCard(stolenCardId);
                    this.RPC_ReceiveCard(stolenCardId);
                    RPC_LogStolen(targetRef, true, reqName);
                }
                else
                {
                    RPC_LogStolen(targetRef, false, reqName);
                }
            }
        }
    }
}
