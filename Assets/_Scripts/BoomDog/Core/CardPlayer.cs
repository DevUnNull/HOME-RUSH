using Fusion;
using UnityEngine;
using BoomDog.Data;
using BoomDog.Visual;
using System.Collections.Generic;

namespace BoomDog.Core
{
    public class CardPlayer : NetworkBehaviour
    {
        public static CardPlayer Local { get; private set; }

        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public NetworkBool IsDead { get; set; }

        public List<string> handCardIds = new List<string>();

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
                Debug.Log("Local Player đã Spawn thành công trên mạng! Đang xin Host chia bài...");
                RPC_RequestCards();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RequestCards()
        {
            // Chỉ người chủ phòng (nắm StateAuthority của GameManager) mới được quyền chia bài
            if (CardGameManager.Instance.HasStateAuthority)
            {
                if (handCardIds.Count == 0 && DeckManager.Instance != null)
                {
                    DeckManager.Instance.DealCardsToSinglePlayer(this);
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
            
            // Chủ phòng nhận lệnh để chuyển lượt cho người tiếp theo
            if (CardGameManager.Instance.HasStateAuthority)
            {
                CardGameManager.Instance.NextTurn();
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
                    hlg.spacing = -30f;
                }
                // Ép buộc thu nhỏ cả khung chứa bài
                mockHand.localScale = new Vector3(0.55f, 0.55f, 1f);

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
