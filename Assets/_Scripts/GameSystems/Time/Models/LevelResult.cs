using System;

namespace GameSystems.Time.Models
{
    /// <summary>
    /// Stores the final statistics and outcome of a level.
    /// </summary>
    [Serializable]
    public class LevelResult
    {
        public bool IsCompleted;
        public float RemainingTime;
        public float TotalTime;
        public int CompletedObjectives;
        public int TotalObjectives;
        public float CompletionPercentage;
        public int StarCount;
        public int FinalScore;
    }
}
