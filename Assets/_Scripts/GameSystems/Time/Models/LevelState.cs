using System;

namespace GameSystems.Time.Models
{
    /// <summary>
    /// Represents the current state of a level's timing and progression.
    /// </summary>
    public enum LevelState
    {
        Waiting,
        Playing,
        Paused,
        Completed,
        Failed
    }
}
