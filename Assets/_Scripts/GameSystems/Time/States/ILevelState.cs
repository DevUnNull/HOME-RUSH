using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.States
{
    /// <summary>
    /// Interface for Level States (State Pattern).
    /// </summary>
    public interface ILevelState
    {
        LevelState StateEnum { get; }
        
        void EnterState(TimeManager manager);
        void UpdateState(TimeManager manager);
        void ExitState(TimeManager manager);
    }
}
