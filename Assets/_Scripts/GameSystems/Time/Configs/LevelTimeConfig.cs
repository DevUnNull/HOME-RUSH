using UnityEngine;

namespace GameSystems.Time.Configs
{
    /// <summary>
    /// Configuration for level timing, star thresholds, and time bonuses/penalties.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelTimeConfig", menuName = "Game Systems/Time/Level Time Config")]
    public class LevelTimeConfig : ScriptableObject
    {
        [Header("Level Info")]
        [Tooltip("The name or identifier of the level.")]
        public string LevelName;

        [Tooltip("The total time allocated for the level in seconds.")]
        public float TotalTime = 300f;

        [Header("Star Thresholds")]
        [Tooltip("Percentage of time remaining to earn 3 stars (e.g., 0.5 for 50%).")]
        [Range(0f, 1f)]
        public float Star3Threshold = 0.5f;

        [Tooltip("Percentage of time remaining to earn 2 stars (e.g., 0.25 for 25%).")]
        [Range(0f, 1f)]
        public float Star2Threshold = 0.25f;

        [Header("Time Modifiers")]
        [Tooltip("Time added when a bonus objective is completed.")]
        public float BonusTimeReward = 15f;

        [Tooltip("Time subtracted when a penalty occurs.")]
        public float PenaltyTime = 10f;
    }
}
