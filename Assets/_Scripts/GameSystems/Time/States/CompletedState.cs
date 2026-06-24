using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.States
{
    public class CompletedState : ILevelState
    {
        public LevelState StateEnum => LevelState.Completed;

        public void EnterState(TimeManager manager)
        {
            // Calculation is done by TimeManager right before transitioning, 
            // or could be handled here if we inject parameters.
        }

        public void UpdateState(TimeManager manager) { }

        public void ExitState(TimeManager manager) { }
    }
}
