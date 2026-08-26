using UnityEditor;
using UnityEngine;
using BoomDog.Data;
using System.IO;

public class CardGenerator : EditorWindow
{
    [MenuItem("BoomDog/Generate Missing Card Templates")]
    public static void GenerateCards()
    {
        string path = "Assets/_Scripts/BoomDog/_ScriptableObjects/Cards";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        CreateCardIfMissing(path, "card_explode", "Exploding Dog", CardType.ExplodingDog);
        CreateCardIfMissing(path, "card_defuse", "Defuse", CardType.Defuse);
        CreateCardIfMissing(path, "card_attack", "Attack", CardType.Attack);
        CreateCardIfMissing(path, "card_skip", "Skip", CardType.Skip);
        CreateCardIfMissing(path, "card_nope", "Nope", CardType.Nope);
        CreateCardIfMissing(path, "card_seethefuture", "See The Future", CardType.SeeTheFuture);
        CreateCardIfMissing(path, "card_shuffle", "Shuffle", CardType.Shuffle);
        CreateCardIfMissing(path, "card_favor", "Favor", CardType.Favor);
        CreateCardIfMissing(path, "card_tacodog", "Tacodog", CardType.CatCard);
        CreateCardIfMissing(path, "card_dogtermelon", "Dogtermelon", CardType.CatCard);
        CreateCardIfMissing(path, "card_hairypotato", "Hairy Potato Dog", CardType.CatCard);
        CreateCardIfMissing(path, "card_bearddog", "Beard Dog", CardType.CatCard);
        CreateCardIfMissing(path, "card_rainbow", "Rainbow-Ralphing Dog", CardType.CatCard);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Gọi luôn lệnh nạp thẻ bài để user khỏi phải bấm thêm 1 bước
        BoomDogAutoLinker.AutoLink();
        
        Debug.Log("✅ Đã tạo đủ các Template bài cơ bản và tự động nạp vào DeckManager!");
    }

    private static void CreateCardIfMissing(string folder, string fileName, string cardName, CardType type)
    {
        string fullPath = $"{folder}/{fileName}.asset";
        if (AssetDatabase.LoadAssetAtPath<CardData>(fullPath) == null)
        {
            CardData asset = ScriptableObject.CreateInstance<CardData>();
            asset.id = fileName;
            asset.cardName = cardName;
            asset.cardType = type;
            asset.description = "Template cho " + cardName;
            
            AssetDatabase.CreateAsset(asset, fullPath);
            Debug.Log($"Đã tạo mới thẻ: {cardName}");
        }
    }
}
