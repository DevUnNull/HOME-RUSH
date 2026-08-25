using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using BoomDog.Data;
using BoomDog.Core;
using Fusion;
using System.Collections.Generic;

public class BoomDogSetup : EditorWindow
{
    [MenuItem("BoomDog/Auto Setup Scene & Assets")]
    public static void AutoSetup()
    {
        // 1. Tạo thư mục
        if (!AssetDatabase.IsValidFolder("Assets/_ScriptableObjects"))
        {
            AssetDatabase.CreateFolder("Assets", "_ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder("Assets/_ScriptableObjects/Cards"))
        {
            AssetDatabase.CreateFolder("Assets/_ScriptableObjects", "Cards");
        }

        // 2. Tạo Card Assets
        CreateCardAsset("card_defuse", "Defuse", CardType.Defuse, "Gỡ mìn thành công!");
        CreateCardAsset("card_explode", "Exploding Dog", CardType.ExplodingDog, "Bùm! Bạn bị loại nếu không có Defuse.");
        CreateCardAsset("card_attack", "Attack", CardType.Attack, "Kết thúc lượt, người tiếp theo đi 2 lượt.");
        CreateCardAsset("card_skip", "Skip", CardType.Skip, "Kết thúc lượt không cần bốc bài.");
        
        AssetDatabase.SaveAssets();
        
        // 3. Khởi tạo GameManager trong Scene
        GameObject gmGo = GameObject.Find("GameManager");
        if (gmGo == null)
        {
            gmGo = new GameObject("GameManager");
        }
        
        if (gmGo.GetComponent<NetworkObject>() == null) gmGo.AddComponent<NetworkObject>();
        if (gmGo.GetComponent<CardGameManager>() == null) gmGo.AddComponent<CardGameManager>();
        
        DeckManager deckManager = gmGo.GetComponent<DeckManager>();
        if (deckManager == null) deckManager = gmGo.AddComponent<DeckManager>();

        // Load tất cả card và nạp vào DeckManager
        deckManager.allAvailableCards = new List<CardData>();
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null)
            {
                deckManager.allAvailableCards.Add(card);
            }
        }
        EditorUtility.SetDirty(gmGo);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        // 4. Tạo Player Prefab
        if (!AssetDatabase.IsValidFolder("Assets/_Prefaps/BoomDog"))
        {
            AssetDatabase.CreateFolder("Assets/_Prefaps", "BoomDog");
        }
        
        GameObject playerGo = new GameObject("CardPlayerPrefab");
        playerGo.AddComponent<NetworkObject>();
        playerGo.AddComponent<CardPlayer>();
        
        string prefabPath = "Assets/_Prefaps/BoomDog/CardPlayerPrefab.prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
        PrefabUtility.SaveAsPrefabAsset(playerGo, prefabPath);
        DestroyImmediate(playerGo);

        Debug.Log("✅ [BoomDog] Setup hoàn tất! Đã tạo Cards, GameManager và Player Prefab.");
    }

    private static void CreateCardAsset(string id, string name, CardType type, string desc)
    {
        string path = $"Assets/_ScriptableObjects/Cards/{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<CardData>(path) == null)
        {
            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.id = id;
            card.cardName = name;
            card.cardType = type;
            card.description = desc;
            AssetDatabase.CreateAsset(card, path);
        }
    }
}
