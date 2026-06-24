using UnityEngine;
using GameSystems.Time.Managers;
using Fusion;
using System.Threading.Tasks;

namespace GameSystems.Time.Managers
{
    public class TimeStarter : MonoBehaviour
    {
        public bool autoStart = true;
        public float delay = 1f;

        private async void Start()
        {
            if (autoStart)
            {
                // Ensure a NetworkRunner exists and is started so NetworkObjects are valid
                NetworkRunner runner = FindObjectOfType<NetworkRunner>();
                if (runner == null)
                {
                    GameObject runnerObj = new GameObject("NetworkRunner");
                    runner = runnerObj.AddComponent<NetworkRunner>();
                    runner.ProvideInput = true;
                }

                if (!runner.IsRunning)
                {
                    await runner.StartGame(new StartGameArgs()
                    {
                        GameMode = GameMode.AutoHostOrClient,
                        SessionName = "TestRoom",
                        Scene = SceneRef.FromIndex(gameObject.scene.buildIndex),
                        SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
                    });
                }

                Invoke(nameof(StartTimer), delay);
            }
        }

        private void StartTimer()
        {
            if (TimeManager.Instance != null && TimeManager.Instance.Object != null && TimeManager.Instance.Object.IsValid)
            {
                if (TimeManager.Instance.HasStateAuthority)
                {
                    TimeManager.Instance.StartLevel();
                    Debug.Log("TimeStarter: Level started.");
                }
            }
            else
            {
                // Try again if not ready
                Invoke(nameof(StartTimer), 0.5f);
            }
        }
    }
}
