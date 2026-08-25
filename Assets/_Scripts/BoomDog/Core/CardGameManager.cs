using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BoomDog.Core
{
    public class CardGameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public static CardGameManager Instance { get; private set; }
        
        [Networked] public int CurrentPlayerTurnIndex { get; set; }
        [Networked] public NetworkBool IsGameStarted { get; set; }
        [Networked] public NetworkBool HasDrawnCardThisTurn { get; set; }

        [Header("Setup")]
        public NetworkObject playerPrefab; // Đã đổi sang NetworkObject cho dễ tự động gán
        
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
            if (Instance == null) Instance = this;
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
            
            Debug.Log("Game Started!");
        }

        public void NextTurn()
        {
            if (!HasStateAuthority) return;
            
            CurrentPlayerTurnIndex = (CurrentPlayerTurnIndex + 1) % _activePlayers.Count;
            HasDrawnCardThisTurn = false; // Bắt đầu lượt mới, chưa bốc bài
        }
    }
}
