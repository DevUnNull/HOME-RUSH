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

        [Networked] public int CardsInDeck { get; set; }

        public List<CardData> allAvailableCards = new List<CardData>(); 
        
        private List<string> _drawPileIds = new List<string>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private List<string> _allNormalCards = new List<string>();
        private List<string> _allDefuseCards = new List<string>();
        private List<string> _allExplodingDogs = new List<string>();

        public void InitializeDeck(int playerCount)
        {
            if (!HasStateAuthority) return;
            CreateCardInstances();
        }

        private void CreateCardInstances()
        {
            _allNormalCards.Clear();
            _allDefuseCards.Clear();
            _allExplodingDogs.Clear();
            _drawPileIds.Clear();
            
            var uniqueTemplates = allAvailableCards.Where(c => c != null).Distinct().ToList();
            
            foreach (var template in uniqueTemplates)
            {
                if (template.cardType == CardType.ExplodingDog)
                {
                    for (int i = 0; i < 4; i++) _allExplodingDogs.Add(template.id);
                }
                else if (template.cardType == CardType.Defuse)
                {
                    for (int i = 0; i < 6; i++) _allDefuseCards.Add(template.id);
                }
                else
                {
                    int count = 4;
                    if (template.cardType == CardType.Nope || template.cardType == CardType.SeeTheFuture) count = 5;
                    for (int i = 0; i < count; i++) _allNormalCards.Add(template.id);
                }
            }
            
            System.Random rng = new System.Random();
            _allNormalCards = _allNormalCards.OrderBy(a => rng.Next()).ToList();
        }

        public void DealCardsToAllPlayers()
        {
            if (!HasStateAuthority) return;

            var players = CardGameManager.Instance.spawnedCardPlayers;
            int playerCount = players.Count;
            int defuseDistributed = 0;

            foreach (var player in players)
            {
                if (_allDefuseCards.Count > 0)
                {
                    string defuseId = _allDefuseCards[0];
                    _allDefuseCards.RemoveAt(0);
                    player.RPC_ReceiveCard(defuseId);
                    defuseDistributed++;
                }

                for (int i = 0; i < 7; i++)
                {
                    if (_allNormalCards.Count > 0)
                    {
                        string normalId = _allNormalCards[0];
                        _allNormalCards.RemoveAt(0);
                        player.RPC_ReceiveCard(normalId);
                    }
                }
            }

            CreateDrawPile(playerCount);

            Debug.Log($"=== DECK INITIALIZATION ===\n" +
                      $"Players: {playerCount}\n" +
                      $"Cards per player: 8\n" +
                      $"Defuse distributed: {defuseDistributed}\n" +
                      $"Defuse in Draw Pile: {_allDefuseCards.Count}\n" +
                      $"ExplodingDog in Draw Pile: {playerCount - 1}\n" +
                      $"Draw Pile: {_drawPileIds.Count}\n" +
                      $"===========================");
        }

        private void CreateDrawPile(int playerCount)
        {
            _drawPileIds.Clear();

            _drawPileIds.AddRange(_allNormalCards);
            _allNormalCards.Clear();

            _drawPileIds.AddRange(_allDefuseCards);
            _allDefuseCards.Clear();

            int dogsToAdd = playerCount - 1;
            for (int i = 0; i < dogsToAdd; i++)
            {
                if (_allExplodingDogs.Count > 0)
                {
                    _drawPileIds.Add(_allExplodingDogs[0]);
                    _allExplodingDogs.RemoveAt(0);
                }
            }

            ShuffleDrawPile();
            CardsInDeck = _drawPileIds.Count;
        }

        public void ShuffleDrawPile()
        {
            System.Random rng = new System.Random();
            _drawPileIds = _drawPileIds.OrderBy(a => rng.Next()).ToList();
        }

        public string DrawCard()
        {
            if (_drawPileIds.Count == 0) return null;
            
            string drawnId = _drawPileIds[0];
            _drawPileIds.RemoveAt(0);
            CardsInDeck = _drawPileIds.Count;
            return drawnId;
        }

        public void ReturnCardToDrawPileRandomly(string cardId)
        {
            if (!HasStateAuthority) return;
            System.Random rng = new System.Random();
            int insertIndex = rng.Next(0, _drawPileIds.Count + 1);
            _drawPileIds.Insert(insertIndex, cardId);
            CardsInDeck = _drawPileIds.Count;
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
