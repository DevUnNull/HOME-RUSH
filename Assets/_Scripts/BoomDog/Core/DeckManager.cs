using Fusion;
using UnityEngine;
using BoomDog.Data;
using System.Collections.Generic;
using System.Linq;

namespace BoomDog.Core
{
    public class DeckManager : NetworkBehaviour
    {
        public static DeckManager Instance { get; private set; }

        public List<CardData> allAvailableCards = new List<CardData>(); 
        
        private List<string> _drawPileIds = new List<string>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InitializeDeck()
        {
            if (!HasStateAuthority) return;
            
            _drawPileIds.Clear();
            
            // TỰ ĐỘNG NHÂN BẢN Ở RUNTIME NẾU SỐ LƯỢNG QUÁ ÍT (Phòng hờ Scene chưa lưu)
            if (allAvailableCards.Count > 0 && allAvailableCards.Count < 20)
            {
                var unique = new List<CardData>(allAvailableCards);
                allAvailableCards.Clear();
                foreach(var c in unique)
                {
                    for(int i = 0; i < 6; i++) allAvailableCards.Add(c);
                }
            }
            
            // Đưa toàn bộ ID bài vào nọc
            foreach (var card in allAvailableCards)
            {
                if (card != null) _drawPileIds.Add(card.id);
            }
            
            ShuffleDrawPile();
        }

        public void ShuffleDrawPile()
        {
            System.Random rng = new System.Random();
            _drawPileIds = _drawPileIds.OrderBy(a => rng.Next()).ToList();
        }

        public void DealCardsToPlayers(List<CardPlayer> players)
        {
            if (!HasStateAuthority) return;
            
            if (_drawPileIds.Count == 0) InitializeDeck();
            
            foreach (var player in players)
            {
                DealCardsToSinglePlayer(player);
            }
        }

        public void DealCardsToSinglePlayer(CardPlayer player)
        {
            if (!HasStateAuthority) return;

            if (_drawPileIds.Count == 0) InitializeDeck();

            int startingCards = 5;
            for (int i = 0; i < startingCards; i++)
            {
                string cardId = DrawCard();
                if (!string.IsNullOrEmpty(cardId))
                {
                    player.RPC_ReceiveCard(cardId);
                }
            }
        }

        public string DrawCard()
        {
            if (_drawPileIds.Count == 0) return null;
            
            string drawnId = _drawPileIds[0];
            _drawPileIds.RemoveAt(0);
            return drawnId;
        }

        public CardData GetCardDataById(string id)
        {
            var data = allAvailableCards.FirstOrDefault(c => c != null && c.id == id);
#if UNITY_EDITOR
            // Tự động tìm lại thẻ bài trong Project nếu Client bị thiếu dữ liệu (rất hay gặp khi dùng ParrelSync)
            if (data == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardData");
                foreach (string guid in guids)
                {
                    string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var c = UnityEditor.AssetDatabase.LoadAssetAtPath<BoomDog.Data.CardData>(p);
                    if (c != null && c.id == id)
                    {
                        allAvailableCards.Add(c);
                        return c;
                    }
                }
            }
#endif
            return data;
        }
    }
}
