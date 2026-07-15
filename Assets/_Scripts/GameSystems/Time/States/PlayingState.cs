using GameSystems.Time.Managers;
using GameSystems.Time.Models;
using UnityEngine;

namespace GameSystems.Time.States
{
    public class PlayingState : ILevelState
    {
        public LevelState StateEnum => LevelState.Playing;

        public void EnterState(TimeManager manager) { }

        public void UpdateState(TimeManager manager)
        {
            if (manager.RemainingTime > 0)
            {
                manager.RemainingTime -= manager.Runner.DeltaTime;
                
                // Check win condition (100% completion)
                if (GameProgressManager.Instance != null && GameProgressManager.Instance.GetTotalProgress() >= 0.999f)
                {
                    manager.CompleteLevel(1, 1);
                    return;
                }

                if (manager.RemainingTime <= 0)
                {
                    manager.RemainingTime = 0;
                    manager.FailLevel();
                }
            }
        }

        public void ExitState(TimeManager manager) { }
    }
}
