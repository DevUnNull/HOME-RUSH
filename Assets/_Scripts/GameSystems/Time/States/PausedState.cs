using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.States
{
    public class PausedState : ILevelState
    {
        public LevelState StateEnum => LevelState.Paused;

        public void EnterState(TimeManager manager) { }

        public void UpdateState(TimeManager manager) { }

        public void ExitState(TimeManager manager) { }
    }
}
