using UnityEditor;
using UnityEngine;
using BoomDog.Core;
using BoomDog.Visual;
using BoomDog.Data;
using Fusion;
using UnityEditor.SceneManagement;

public class BoomDogAutoLinker : EditorWindow
{
    [MenuItem("BoomDog/Auto Link Network Prefabs (Tự động gán Prefab)")]
    public static void AutoLink()
    {
        bool isChanged = false;

        // 1. Dọn dẹp bài nháp trong MockHand
        GameObject mockHand = GameObject.Find("MockHand_TestOnly");
        if (mockHand != null)
        {
            int childCount = mockHand.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mockHand.transform.GetChild(i).gameObject);
            }
            if (childCount > 0)
            {
                Debug.Log($"✅ Đã xóa {childCount} lá bài nháp trong MockHand.");
                isChanged = true;
            }
        }

        // 2. Load Prefabs
        string playerPrefabPath = "Assets/_Prefaps/BoomDog/CardPlayerPrefab.prefab";
        string cardPrefabPath = "Assets/_Prefaps/BoomDog/MockCardPrefab.prefab";
        
        GameObject pPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        GameObject cPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);

        // Sinh CardPlayerPrefab nếu chưa tồn tại
        if (pPrefab == null)
        {
            GameObject newPlayerGo = new GameObject("CardPlayerPrefab");
            newPlayerGo.AddComponent<NetworkObject>();
            newPlayerGo.AddComponent<CardPlayer>();
            pPrefab = PrefabUtility.SaveAsPrefabAsset(newPlayerGo, playerPrefabPath);
            DestroyImmediate(newPlayerGo);
            isChanged = true;
        }

        // 3. Gán MockCardPrefab vào CardPlayerPrefab.cardUIPrefab
        if (pPrefab != null && cPrefab != null)
        {
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
            {
                var cp = editingScope.prefabContentsRoot.GetComponent<CardPlayer>();
                if (cp != null && cp.cardUIPrefab != cPrefab)
                {
                    cp.cardUIPrefab = cPrefab;
                    Debug.Log("✅ Đã gán MockCardPrefab vào CardPlayerPrefab.cardUIPrefab.");
                    isChanged = true;
                }
            }

            // Gán ÉP TextMeshPro vào MockCardPrefab để chắc chắn 100% không bị Missing
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(cardPrefabPath))
            {
                var cv = editingScope.prefabContentsRoot.GetComponent<CardVisual>();
                if (cv != null)
                {
                    var tmp = editingScope.prefabContentsRoot.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        cv.cardNameText = tmp;
                        isChanged = true;
                    }
                }
            }
        }

        // 4. Gán CardPlayerPrefab vào GameManager trong Scene
        GameObject gmGo = GameObject.Find("GameManager");
        if (gmGo != null)
        {
            CardGameManager cgm = gmGo.GetComponent<CardGameManager>();
            if (cgm != null && pPrefab != null)
            {
                var netObj = pPrefab.GetComponent<NetworkObject>();
                if (cgm.playerPrefab != netObj)
                {
                    cgm.playerPrefab = netObj;
                    Debug.Log("✅ Đã gán CardPlayerPrefab vào GameManager.");
                    isChanged = true;
                }
            }

            // 5. Nạp toàn bộ thẻ bài vào DeckManager
            var deck = gmGo.GetComponent<BoomDog.Core.DeckManager>();
            if (deck != null)
            {
                deck.allAvailableCards.Clear();
                string[] guids = AssetDatabase.FindAssets("t:CardData");
                foreach (string guid in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    var c = AssetDatabase.LoadAssetAtPath<CardData>(p);
                    if (c != null) deck.allAvailableCards.Add(c);
                }
                EditorUtility.SetDirty(deck);
                Debug.Log($"✅ Đã nạp {deck.allAvailableCards.Count} thẻ bài vào bộ bài.");
                isChanged = true;
            }
        }

        if (isChanged)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("🎉 Hoàn tất tự động liên kết các Prefabs!");
        }
        else
        {
            Debug.Log("Mọi thứ đã được gán sẵn, không cần thay đổi gì thêm.");
        }
    }
}
