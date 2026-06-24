using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.States
{
    public class WaitingState : ILevelState
    {
        public LevelState StateEnum => LevelState.Waiting;

        public void EnterState(TimeManager manager) { }

        public void UpdateState(TimeManager manager) { }

        public void ExitState(TimeManager manager) { }
    }
}
