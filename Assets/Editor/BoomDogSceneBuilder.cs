using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using BoomDog.Visual;
using BoomDog.Core;
using UnityEditor.SceneManagement;
using Fusion;
using System.Collections.Generic;
using BoomDog.Data;

public class BoomDogSceneBuilder : EditorWindow
{
    [MenuItem("BoomDog/Build Mock Scene (Chơi thử)")]
    public static void BuildMockScene()
    {
        // 1. Setup Camera 2.5D (giống góc nhìn EK2)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 12, -10);
            mainCam.transform.rotation = Quaternion.Euler(50, 0, 0);
            mainCam.orthographic = false;
        }

        // 2. TẠO EVENTSYSTEM (Rất quan trọng để tương tác click bài)
        if (GameObject.FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // 3. Tạo bàn chơi (Màu gỗ)
        GameObject table = GameObject.Find("Table");
        if (table == null) table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.position = new Vector3(0, -0.5f, 0);
        table.transform.localScale = new Vector3(25, 1, 15);
        table.GetComponent<Renderer>().sharedMaterial.color = new Color(0.85f, 0.6f, 0.4f); 

        // 4. Tạo Nọc (Draw Pile) và Mộ (Discard Pile) giả lập trên bàn
        GameObject drawPile = GameObject.Find("DrawPile");
        if (drawPile == null) drawPile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        drawPile.name = "DrawPile";
        drawPile.transform.position = new Vector3(-3, 0.5f, 0); // Đặt bên trái
        drawPile.transform.localScale = new Vector3(2, 0.5f, 3);
        drawPile.GetComponent<Renderer>().sharedMaterial.color = new Color(0.7f, 0.1f, 0.1f);

        GameObject discardPile = GameObject.Find("DiscardPile");
        if (discardPile == null) discardPile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        discardPile.name = "DiscardPile";
        discardPile.transform.position = new Vector3(3, 0.01f, 0); // Đặt bên phải
        discardPile.transform.rotation = Quaternion.Euler(90, 0, 0);
        discardPile.transform.localScale = new Vector3(2, 3, 1);
        discardPile.GetComponent<Renderer>().sharedMaterial.color = new Color(1f, 1f, 1f, 0.3f); // Trong suốt mờ

        // 5. Tạo Mock Players (Xếp hàng ngang phía trên giống ảnh)
        Vector3[] playerPositions = {
            new Vector3(-6, 0.5f, 5), // Player 2 (Trái cùng)
            new Vector3(-2, 0.5f, 5), // Player 3
            new Vector3(2, 0.5f, 5),  // Player 4
            new Vector3(6, 0.5f, 5)   // Player 5 (Phải cùng)
        };

        GameObject playersParent = GameObject.Find("Players");
        if (playersParent == null) playersParent = new GameObject("Players");
        
        for (int i = 0; i < playerPositions.Length; i++)
        {
            if (GameObject.Find("MockPlayer " + (i + 1)) != null) continue;
            GameObject pGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pGo.name = "MockPlayer " + (i + 1);
            pGo.transform.SetParent(playersParent.transform);
            pGo.transform.position = playerPositions[i];
            pGo.GetComponent<Renderer>().sharedMaterial.color = Color.HSVToRGB(i * 0.25f, 0.8f, 0.8f);
        }

        // 6. Tạo UI Canvas
        GameObject canvasGo = GameObject.Find("MainCanvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("MainCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        // 7. Tạo Card Prefab (có thể click)
        if (!AssetDatabase.IsValidFolder("Assets/_Prefaps/BoomDog")) AssetDatabase.CreateFolder("Assets/_Prefaps", "BoomDog");
        string cardPrefabPath = "Assets/_Prefaps/BoomDog/MockCardPrefab.prefab";
        Object cPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);
        
        if (cPrefab == null)
        {
            GameObject cardGo = new GameObject("MockCardPrefab");
            RectTransform cardRect = cardGo.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(130, 200); // Tỉ lệ card
            
            GameObject cardFront = new GameObject("Front");
            cardFront.transform.SetParent(cardGo.transform, false);
            Image frontImage = cardFront.AddComponent<Image>();
            frontImage.color = Color.white;
            frontImage.rectTransform.sizeDelta = cardRect.sizeDelta;
            
            GameObject cardText = new GameObject("Text");
            cardText.transform.SetParent(cardFront.transform, false);
            TextMeshProUGUI tmp = cardText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Card Name";
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = cardRect.sizeDelta;
            
            GameObject cardBack = new GameObject("Back");
            cardBack.transform.SetParent(cardGo.transform, false);
            Image backImage = cardBack.AddComponent<Image>();
            backImage.color = new Color(0.8f, 0.2f, 0.2f); 
            backImage.rectTransform.sizeDelta = cardRect.sizeDelta;
            
            CardVisual cardVisual = cardGo.AddComponent<CardVisual>();
            cardVisual.cardImage = frontImage;
            cardVisual.cardFrontObj = cardFront;
            cardVisual.cardBackObj = cardBack;
            cardVisual.cardNameText = tmp;
            
            cPrefab = PrefabUtility.SaveAsPrefabAsset(cardGo, AssetDatabase.GenerateUniqueAssetPath(cardPrefabPath));
            DestroyImmediate(cardGo);
        }

        // 8. Tạo bài mẫu trên tay (Local Player) - Căn dưới cùng màn hình
        GameObject mockHand = GameObject.Find("MockHand_TestOnly");
        if (mockHand == null)
        {
            mockHand = new GameObject("MockHand_TestOnly");
            mockHand.transform.SetParent(canvasGo.transform, false);
            HorizontalLayoutGroup layout = mockHand.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.spacing = 10f; // Khoảng cách giữa các lá bài
            
            RectTransform handRect = mockHand.GetComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0, 0);
            handRect.anchorMax = new Vector2(1, 0.25f);
            handRect.sizeDelta = Vector2.zero;
            handRect.anchoredPosition = Vector2.zero;

            var deckCards = new List<CardData>();
            string[] guids = AssetDatabase.FindAssets("t:CardData");
            foreach (string guid in guids)
            {
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid));
                if (card != null) deckCards.Add(card);
            }

            if (cPrefab != null && deckCards.Count > 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    GameObject c = (GameObject)PrefabUtility.InstantiatePrefab(cPrefab);
                    c.transform.SetParent(mockHand.transform, false);
                    c.name = "MockCard " + (i+1);
                    CardData rData = deckCards[Random.Range(0, deckCards.Count)];
                    c.GetComponent<CardVisual>().Initialize(rData, true);
                }
            }
        }

        // 9. Sinh GameUIManager
        GameObject uiManagerGo = GameObject.Find("GameUIManager");
        if (uiManagerGo == null)
        {
            uiManagerGo = new GameObject("GameUIManager");
            uiManagerGo.transform.SetParent(canvasGo.transform, false);
            uiManagerGo.AddComponent<GameUIManager>();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Đã cập nhật Layout giống Exploding Kittens 2, thêm Nọc/Mộ, xếp Player lên trên và Thêm EventSystem!");
    }
}
