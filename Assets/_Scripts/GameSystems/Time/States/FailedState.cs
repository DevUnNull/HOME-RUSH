using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.States
{
    public class FailedState : ILevelState
    {
        public LevelState StateEnum => LevelState.Failed;

        public void EnterState(TimeManager manager)
        {
            // Level is failed, logic handled before transition
        }

        public void UpdateState(TimeManager manager) { }

        public void ExitState(TimeManager manager) { }
    }
}
