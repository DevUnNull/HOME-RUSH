using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BoomDog.Visual;

namespace BoomDog.Core
{
    public class CardGameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public static CardGameManager Instance { get; private set; }
        
        [Networked] public int CurrentPlayerTurnIndex { get; set; }
        [Networked] public int RemainingTurns { get; set; }
        [Networked] public NetworkBool HasDrawnThisTurn { get; set; }
        [Networked] public NetworkBool IsGameStarted { get; set; }

        [Header("Setup")]
        public NetworkObject playerPrefab; // Đã đổi sang NetworkObject cho dễ tự động gán
        public GameObject originalPlayerVisualPrefab; // Kéo Player.prefab của bạn vào đây
        
        // Vị trí chỗ ngồi
        private Vector3[] seatPositions = {
            new Vector3(0, 0.5f, -4), // Seat 1 (Bottom)
            new Vector3(-6, 0.5f, 5), // Seat 2 
            new Vector3(-2, 0.5f, 5), // Seat 3 
            new Vector3(2, 0.5f, 5),  // Seat 4 
            new Vector3(6, 0.5f, 5)   // Seat 5 
        };

        private List<PlayerRef> _activePlayers = new List<PlayerRef>();
        public List<CardPlayer> spawnedCardPlayers = new List<CardPlayer>();

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                // Automatically add the Visual Manager
                if (gameObject.GetComponent<BoomDog.Visual.CardGameVisualManager>() == null)
                {
                    var cvm = gameObject.AddComponent<BoomDog.Visual.CardGameVisualManager>();
                    cvm.originalPlayerPrefab = originalPlayerVisualPrefab;
                }
            }
            else Destroy(gameObject);
        }

        public override void Spawned()
        {
            // Trong Shared Mode, mỗi client tự cập nhật danh sách Player khi load scene
            foreach (var p in Runner.ActivePlayers)
            {
                PlayerJoined(p);
            }
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (!_activePlayers.Contains(player))
            {
                _activePlayers.Add(player);
                
                // ĐẢM BẢO THỨ TỰ NGƯỜI CHƠI LUÔN GIỐNG NHAU TRÊN MỌI MÁY (Bắt buộc cho Shared Mode)
                _activePlayers.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));

                // TRONG SHARED MODE: Mỗi người tự Spawn NetworkObject của chính mình
                if (player == Runner.LocalPlayer && playerPrefab != null)
                {
                    int seatIndex = _activePlayers.Count - 1;
                    if (seatIndex >= seatPositions.Length) seatIndex = 0;
                    
                    Runner.Spawn(playerPrefab, seatPositions[seatIndex], Quaternion.identity, player);
                }

                // Tự động bắt đầu game khi tất cả mọi người đã vào phòng (Chỉ Master Client xử lý lệnh này)
                if (HasStateAuthority && _activePlayers.Count >= Runner.ActivePlayers.Count() && !IsGameStarted)
                {
                    Invoke("StartGame", 2f);
                }
            }
        }

        public bool IsMyTurn(PlayerRef player)
        {
            if (!IsGameStarted || _activePlayers.Count == 0) return false;
            return _activePlayers.IndexOf(player) == CurrentPlayerTurnIndex;
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (HasStateAuthority)
            {
                _activePlayers.Remove(player);
            }
        }

        public void StartGame()
        {
            if (!HasStateAuthority || IsGameStarted) return;
            
            IsGameStarted = true;
            CurrentPlayerTurnIndex = 0;
            RemainingTurns = 1;
            HasDrawnThisTurn = false;
            
            Debug.Log("Game Started!");
            
            int playerCount = _activePlayers.Count;
            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.InitializeDeck(playerCount);
                DeckManager.Instance.DealCardsToAllPlayers();
            }
        }

        public CardPlayer GetCardPlayer(PlayerRef pRef)
        {
            foreach (var p in spawnedCardPlayers)
            {
                if (p != null && p.Object != null && p.Object.StateAuthority == pRef) return p;
            }
            return null;
        }

        public void FinishCurrentTurn()
        {
            if (!HasStateAuthority) return;

            RemainingTurns--;

            if (GetAlivePlayerCount() <= 1)
            {
                GameOver();
                return;
            }

            if (RemainingTurns > 0)
            {
                HasDrawnThisTurn = false;
                return;
            }

            FindNextAlivePlayer();
            RemainingTurns = 1;
            HasDrawnThisTurn = false;
        }

        public void HandleAttack()
        {
            if (!HasStateAuthority) return;

            int transferTurns = (RemainingTurns - 1) + 2;
            RemainingTurns = 0;
            FindNextAlivePlayer();
            RemainingTurns = transferTurns;
            HasDrawnThisTurn = false;
        }

        public void HandlePlayerDeath(PlayerRef deadPlayerRef)
        {
            if (!HasStateAuthority) return;

            var p = GetCardPlayer(deadPlayerRef);
            if (p != null) p.IsDead = true;

            if (IsMyTurn(deadPlayerRef))
            {
                RemainingTurns = 0;

                if (GetAlivePlayerCount() <= 1)
                {
                    GameOver();
                    return;
                }

                FindNextAlivePlayer();
                RemainingTurns = 1;
                HasDrawnThisTurn = false;
            }
        }

        private int GetAlivePlayerCount()
        {
            int count = 0;
            foreach (var p in _activePlayers)
            {
                var cp = GetCardPlayer(p);
                if (cp != null && !cp.IsDead) count++;
            }
            return count;
        }

        public List<PlayerRef> GetAlivePlayerRefs()
        {
            List<PlayerRef> alive = new List<PlayerRef>();
            foreach (var p in _activePlayers)
            {
                var cp = GetCardPlayer(p);
                if (cp != null && !cp.IsDead) alive.Add(p);
            }
            return alive;
        }

        private void FindNextAlivePlayer()
        {
            if (_activePlayers.Count == 0) return;

            int safety = 0;
            while (safety < 10)
            {
                CurrentPlayerTurnIndex = (CurrentPlayerTurnIndex + 1) % _activePlayers.Count;
                var cp = GetCardPlayer(_activePlayers[CurrentPlayerTurnIndex]);
                if (cp != null && !cp.IsDead)
                {
                    break;
                }
                safety++;
            }
        }

        private void GameOver()
        {
            Debug.Log("Game Over!");
            if (GameUIManager.Instance != null) GameUIManager.Instance.AddLog("GAME OVER! Đã tìm ra người thắng!");
        }
    }
}
