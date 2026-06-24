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
                    Debug.Log("TimeStarter: Level started. Nhấn W để thắng, F để thua, T để trừ 10s.");
                }
            }
            else
            {
                // Try again if not ready
                Invoke(nameof(StartTimer), 0.5f);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I)) TestWin();
            if (Input.GetKeyDown(KeyCode.O)) TestFail();
            if (Input.GetKeyDown(KeyCode.P)) TestRemove10s();
        }

        [ContextMenu("Test: Force Win (Hoàn thành 1/1 objective)")]
        public void TestWin()
        {
            if (TimeManager.Instance != null && TimeManager.Instance.HasStateAuthority)
            {
                TimeManager.Instance.CompleteLevel(1, 1);
            }
        }

        [ContextMenu("Test: Force Fail")]
        public void TestFail()
        {
            if (TimeManager.Instance != null && TimeManager.Instance.HasStateAuthority)
            {
                TimeManager.Instance.FailLevel();
            }
        }

        [ContextMenu("Test: Trừ 10s")]
        public void TestRemove10s()
        {
            if (TimeManager.Instance != null && TimeManager.Instance.HasStateAuthority)
            {
                TimeManager.Instance.RemoveTime(10f);
            }
        }
    }
}
