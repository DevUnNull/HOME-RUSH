using UnityEngine;
using GameSystems.Time.Configs;
using GameSystems.Time.Models;

namespace GameSystems.Time.Calculators
{
    /// <summary>
    /// Standard implementation for calculating final level outcome, including stars and score.
    /// </summary>
    public class LevelResultCalculator : ILevelResultCalculator
    {
        private const int ScorePerSecondRemaining = 10;
        private const int BaseCompletionScore = 1000;

        public LevelResult CreateLevelResult(
            LevelTimeConfig config, 
            float remainingTime, 
            int completedObjectives, 
            int totalObjectives,
            bool isFailed)
        {
            bool isCompleted = !isFailed && completedObjectives >= totalObjectives;
            float completionPercentage = totalObjectives > 0 ? (float)completedObjectives / totalObjectives : 1f;

            int starCount = CalculateStars(config, remainingTime, isCompleted);
            int finalScore = CalculateScore(remainingTime, isCompleted, completionPercentage);

            return new LevelResult
            {
                IsCompleted = isCompleted,
                RemainingTime = remainingTime,
                TotalTime = config.TotalTime,
                CompletedObjectives = completedObjectives,
                TotalObjectives = totalObjectives,
                CompletionPercentage = completionPercentage,
                StarCount = starCount,
                FinalScore = finalScore
            };
        }

        private int CalculateStars(LevelTimeConfig config, float remainingTime, bool isCompleted)
        {
            if (!isCompleted) return 0;

            float remainingPercentage = remainingTime / config.TotalTime;

            if (remainingPercentage >= config.Star3Threshold) return 3;
            if (remainingPercentage >= config.Star2Threshold) return 2;
            if (remainingTime > 0) return 1;

            return 0;
        }

        private int CalculateScore(float remainingTime, bool isCompleted, float completionPercentage)
        {
            if (!isCompleted) return 0;

            int score = BaseCompletionScore;
            score += Mathf.CeilToInt(remainingTime) * ScorePerSecondRemaining;
            
            return score;
        }
    }
}
