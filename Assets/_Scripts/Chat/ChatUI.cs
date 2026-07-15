using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance;

    [Header("UI")]
    [SerializeField] private GameObject inputPanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private CanvasGroup chatCanvasGroup;

    [SerializeField] private float visibleTime = 5f;

    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    private bool isTyping = false;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        chatCanvasGroup.alpha = 0f;
        chatCanvasGroup.interactable = false;
        chatCanvasGroup.blocksRaycasts = false;
    }
    private void Update()
    {
        if (!isTyping)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                OpenChat();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CloseChat();
            }
        }
    }

    private void OpenChat()
    {
        isTyping = true;

        inputPanel.SetActive(true);

        inputField.text = "";

        inputField.ActivateInputField();

        inputField.Select();
    }

    private void CloseChat()
    {
        isTyping = false;

        string message = inputField.text.Trim();

        if (!string.IsNullOrEmpty(message))
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");

            ChatManager.Instance.RPC_SendChat(playerName, message);
        }

        inputField.text = "";

        inputField.DeactivateInputField();

        inputPanel.SetActive(false);
    }

    public void AddMessage(string playerName, string message)
    {
        GameObject obj = Instantiate(messagePrefab, content);

        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = $"{playerName}: {message}";

        Canvas.ForceUpdateCanvases();
        ShowChat();

        scrollRect.verticalNormalizedPosition = 0f;

    }
    private void ShowChat()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        chatCanvasGroup.alpha = 1f;
        chatCanvasGroup.interactable = true;
        chatCanvasGroup.blocksRaycasts = true;

        fadeCoroutine = StartCoroutine(FadeAfterDelay());
    }

    private IEnumerator FadeAfterDelay()
    {
        yield return new WaitForSeconds(visibleTime);

        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            chatCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);

            yield return null;
        }

        chatCanvasGroup.alpha = 0f;
        chatCanvasGroup.interactable = false;
        chatCanvasGroup.blocksRaycasts = false;
    }
}