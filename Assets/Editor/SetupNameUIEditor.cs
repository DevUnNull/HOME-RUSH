using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetupNameUIEditor : Editor
{
    [MenuItem("Tools/Setup Player In-Game UI")]
    public static void SetupPlayerPrefab()
    {
        string prefabPath = "Assets/_Prefaps/Player.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("Could not find Player prefab at " + prefabPath);
            return;
        }

        var display = prefabRoot.GetComponent<PlayerNameDisplay>();
        if (display == null)
        {
            display = prefabRoot.AddComponent<PlayerNameDisplay>();
        }

        Transform canvasT = prefabRoot.transform.Find("NameCanvas");
        if (canvasT == null)
        {
            GameObject canvasGo = new GameObject("NameCanvas");
            canvasGo.transform.SetParent(prefabRoot.transform, false);
            canvasGo.transform.localPosition = new Vector3(0, 2.2f, 0);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rect = canvasGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(3, 0.5f);

            GameObject textGo = new GameObject("NameText");
            textGo.transform.SetParent(canvasGo.transform, false);

            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.5f;
            tmp.fontSizeMax = 2f;

            rect.localScale = new Vector3(1, 1, 1);

            // Set private field using reflection since it's serialized
            var fieldInfo = display.GetType().GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(display, tmp);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Successfully setup Name UI on Player Prefab!");
    }

    [MenuItem("Tools/Setup Lobby Name UI")]
    public static void SetupUI()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "LobbyScene")
        {
            Debug.LogError("Please open LobbyScene first!");
            return;
        }

        var manager = Object.FindAnyObjectByType<LobbyManager>();
        if (manager == null)
        {
            Debug.LogError("LobbyManager not found in scene!");
            return;
        }

        Undo.RecordObject(manager, "Setup Name UI");

        // Find existing big texts
        var t1 = GameObject.Find("Player_1");
        var t2 = GameObject.Find("Player_2");
        var t3 = GameObject.Find("Player_3");
        var t4 = GameObject.Find("Player_4");

        GameObject[] texts = new GameObject[] { t1, t2, t3, t4 };

        // Add Button if missing
        foreach (var t in texts)
        {
            if (t != null && t.GetComponent<Button>() == null)
            {
                Undo.AddComponent<Button>(t);
            }
        }

        // Create an input panel if not exists
        var canvas = GameObject.Find("LobbyCanvas");
        var nameInputPanel = GameObject.Find("LobbyCanvas/NameInputPanel");
        TMP_InputField inputField = null;
        Button submitBtn = null;

        if (nameInputPanel == null)
        {
            nameInputPanel = new GameObject("NameInputPanel");
            Undo.RegisterCreatedObjectUndo(nameInputPanel, "Create NameInputPanel");
            nameInputPanel.transform.SetParent(canvas.transform, false);
            var rect = nameInputPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = nameInputPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            // Input field
            var inputGo = new GameObject("InputField");
            inputGo.transform.SetParent(nameInputPanel.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(400, 60);
            var inputImage = inputGo.AddComponent<Image>();

            inputField = inputGo.AddComponent<TMP_InputField>();

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGo.transform, false);
            var textRect = textArea.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(textArea.transform, false);
            var textRect2 = textGo.AddComponent<RectTransform>();
            textRect2.anchorMin = Vector2.zero;
            textRect2.anchorMax = Vector2.one;
            textRect2.offsetMin = Vector2.zero;
            textRect2.offsetMax = Vector2.zero;
            var tmpText = textGo.AddComponent<TextMeshProUGUI>();
            tmpText.color = Color.black;
            tmpText.fontSize = 40;
            tmpText.alignment = TextAlignmentOptions.Center;

            inputField.textViewport = textRect;
            inputField.textComponent = tmpText;
            
            // Assign a default background (like a white sprite) if available. We will leave it empty white box for now.

            // Submit Button
            var submitGo = new GameObject("SubmitButton");
            submitGo.transform.SetParent(nameInputPanel.transform, false);
            var submitRect = submitGo.AddComponent<RectTransform>();
            submitRect.anchoredPosition = new Vector2(0, -80);
            submitRect.sizeDelta = new Vector2(160, 50);
            submitGo.AddComponent<Image>();
            submitBtn = submitGo.AddComponent<Button>();

            var btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(submitGo.transform, false);
            var btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            var btnTmpText = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnTmpText.text = "OK";
            btnTmpText.color = Color.black;
            btnTmpText.fontSize = 30;
            btnTmpText.alignment = TextAlignmentOptions.Center;

            nameInputPanel.SetActive(false);
        }
        else
        {
            inputField = nameInputPanel.transform.Find("InputField").GetComponent<TMP_InputField>();
            submitBtn = nameInputPanel.transform.Find("SubmitButton").GetComponent<Button>();
        }

        // Link everything
        var so = new SerializedObject(manager);
        
        var textsProp = so.FindProperty("playerNameTexts");
        textsProp.arraySize = 4;
        textsProp.GetArrayElementAtIndex(0).objectReferenceValue = t1.GetComponent<TextMeshProUGUI>();
        textsProp.GetArrayElementAtIndex(1).objectReferenceValue = t2.GetComponent<TextMeshProUGUI>();
        textsProp.GetArrayElementAtIndex(2).objectReferenceValue = t3.GetComponent<TextMeshProUGUI>();
        textsProp.GetArrayElementAtIndex(3).objectReferenceValue = t4.GetComponent<TextMeshProUGUI>();

        so.FindProperty("nameInputPanel").objectReferenceValue = nameInputPanel;
        so.FindProperty("playerNameInputField").objectReferenceValue = inputField;
        so.FindProperty("submitNameButton").objectReferenceValue = submitBtn;

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("Successfully setup Name UI and linked to LobbyManager!");
    }
}
