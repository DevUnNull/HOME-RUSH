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
