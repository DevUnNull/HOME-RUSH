using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using GameSystems.Time.Configs;
using GameSystems.Time.Models;
using GameSystems.Time.Calculators;
using GameSystems.Time.States;

namespace GameSystems.Time.Managers
{
    /// <summary>
    /// Networked Singleton responsible for level timing and state management using Photon Fusion.
    /// Uses State Pattern for behavior and delegates calculation to ILevelResultCalculator.
    /// </summary>
    public class TimeManager : Singleton<TimeManager>
    {
        [Header("Configuration")]
        [SerializeField] private LevelTimeConfig fallbackConfig; // Used if none provided

        // --- Networked State ---
        [Networked] public float RemainingTime { get; set; }
        [Networked] public float TotalLevelTime { get; set; }
        
        [Networked, OnChangedRender(nameof(OnStateChangedCallback))]
        public LevelState CurrentStateEnum { get; set; }

        // --- Dependencies ---
        private ILevelResultCalculator resultCalculator;
        private LevelTimeConfig currentConfig;

        // --- State Pattern ---
        private Dictionary<LevelState, ILevelState> states;
        private ILevelState currentState;

        // --- Local Events for UI ---
        public event Action<LevelState> OnStateChangedLocal;
        public LevelResult FinalResult { get; private set; } // Stored locally on end

        public bool IsRunning => CurrentStateEnum == LevelState.Playing;

        public override void Spawned()
        {
            base.Spawned();
            
            // Dependency Injection (Manual for now, could use Zenject/VContainer)
            resultCalculator = new LevelResultCalculator();

            InitializeStates();
            
            if (HasStateAuthority)
            {
                TransitionToState(LevelState.Waiting);
            }
        }

        private void InitializeStates()
        {
            states = new Dictionary<LevelState, ILevelState>
            {
                { LevelState.Waiting, new WaitingState() },
                { LevelState.Playing, new PlayingState() },
                { LevelState.Paused, new PausedState() },
                { LevelState.Completed, new CompletedState() },
                { LevelState.Failed, new FailedState() }
            };
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            if (currentState != null)
            {
                currentState.UpdateState(this);
            }
        }

        /// <summary>
        /// Transitions the State Machine. Only called on State Authority.
        /// </summary>
        public void TransitionToState(LevelState newStateEnum)
        {
            if (!HasStateAuthority) return;

            if (currentState != null)
            {
                currentState.ExitState(this);
            }

            CurrentStateEnum = newStateEnum;
            currentState = states[newStateEnum];
            currentState.EnterState(this);
        }

        /// <summary>
        /// Callback triggered on all clients when CurrentStateEnum changes.
        /// </summary>
        private void OnStateChangedCallback()
        {
            OnStateChangedLocal?.Invoke(CurrentStateEnum);
        }

        // --- Public Methods (State Authority Only) ---

        public void StartLevel(LevelTimeConfig config = null)
        {
            if (!HasStateAuthority) return;

            currentConfig = config != null ? config : fallbackConfig;
            
            if (currentConfig != null)
            {
                TotalLevelTime = currentConfig.TotalTime;
                RemainingTime = TotalLevelTime;
                TransitionToState(LevelState.Playing);
            }
            else
            {
                Debug.LogError("TimeManager: Cannot start level, no config provided!");
            }
        }

        public void PauseTimer()
        {
            if (HasStateAuthority && CurrentStateEnum == LevelState.Playing)
            {
                TransitionToState(LevelState.Paused);
            }
        }

        public void ResumeTimer()
        {
            if (HasStateAuthority && CurrentStateEnum == LevelState.Paused)
            {
                TransitionToState(LevelState.Playing);
            }
        }

        public void AddTime(float seconds)
        {
            if (HasStateAuthority && (CurrentStateEnum == LevelState.Playing || CurrentStateEnum == LevelState.Paused))
            {
                RemainingTime += seconds;
            }
        }

        public void RemoveTime(float seconds)
        {
            if (HasStateAuthority && (CurrentStateEnum == LevelState.Playing || CurrentStateEnum == LevelState.Paused))
            {
                RemainingTime -= seconds;
                if (RemainingTime <= 0)
                {
                    RemainingTime = 0;
                    FailLevel();
                }
            }
        }

        public void CompleteLevel(int completedObjectives = 1, int totalObjectives = 1)
        {
            if (!HasStateAuthority) return;
            if (CurrentStateEnum == LevelState.Completed || CurrentStateEnum == LevelState.Failed) return;

            GenerateResultLocal(completedObjectives, totalObjectives, false);

            // Using RPC to broadcast the result to all clients since LevelResult isn't easily networked directly
            RPC_BroadcastResult(FinalResult.RemainingTime, FinalResult.CompletedObjectives, FinalResult.TotalObjectives, false);

            TransitionToState(LevelState.Completed);
        }

        public void FailLevel()
        {
            if (!HasStateAuthority) return;
            if (CurrentStateEnum == LevelState.Completed || CurrentStateEnum == LevelState.Failed) return;

            GenerateResultLocal(0, 1, true);

            RPC_BroadcastResult(RemainingTime, 0, 1, true);

            TransitionToState(LevelState.Failed);
        }

        private void GenerateResultLocal(int completed, int total, bool failed)
        {
            var configToUse = currentConfig != null ? currentConfig : fallbackConfig;
            if (configToUse == null) configToUse = ScriptableObject.CreateInstance<LevelTimeConfig>(); // Fallback

            FinalResult = resultCalculator.CreateLevelResult(
                configToUse,
                RemainingTime,
                completed,
                total,
                failed);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastResult(float remTime, int compObj, int totObj, bool isFailed)
        {
            // Clients generate their own local copy of the result using the synced parameters
            if (!HasStateAuthority) 
            {
                GenerateResultLocal(compObj, totObj, isFailed);
            }
        }
    }
}
