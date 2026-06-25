using Fusion;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MatchmakingManager : MonoBehaviour
{
    [Header("Fusion")]
    public NetworkRunner runnerPrefab;

    [Header("UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TextMeshProUGUI warningText;

    [SerializeField] private int sceneIndex = 2;


    private NetworkRunner _currentRunner;

    public async void OnJoinRoomClicked()
    {
        string roomCode = roomCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(roomCode))
        {
            warningText.text = "Enter room code!";
            warningText.gameObject.SetActive(true);
            return;
        }

        await ConnectToRoom(roomCode);
    }

    public async void OnCreateRoomClicked()
    {
        string roomCode = GenerateRoomCode();
        await ConnectToRoom(roomCode);
    }

    private async Task ConnectToRoom(string sessionName)
    {
        if (_currentRunner == null)
        {
            _currentRunner = Instantiate(runnerPrefab);
        }

        var sceneManager = _currentRunner.gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = _currentRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await _currentRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            if (!_currentRunner.IsSharedModeMasterClient)
            {
                return;
            }
            await _currentRunner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
