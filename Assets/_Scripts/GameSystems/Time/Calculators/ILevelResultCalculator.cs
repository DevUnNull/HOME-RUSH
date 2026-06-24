using GameSystems.Time.Configs;
using GameSystems.Time.Models;

namespace GameSystems.Time.Calculators
{
    /// <summary>
    /// Interface for calculating level results, allowing different score/star logic implementations.
    /// </summary>
    public interface ILevelResultCalculator
    {
        LevelResult CreateLevelResult(
            LevelTimeConfig config, 
            float remainingTime, 
            int completedObjectives, 
            int totalObjectives,
            bool isFailed);
    }
}
