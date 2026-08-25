using UnityEngine;

namespace BoomDog.Data
{
    [CreateAssetMenu(fileName = "NewCardData", menuName = "BoomDog/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Card Information")]
        public string id;
        public string cardName;
        public CardType cardType;
        [TextArea(3, 5)]
        public string description;
        
        [Header("Visuals")]
        public Sprite cardFrontSprite; // Mặt trước lá bài
        public Color highlightColor = Color.white; // Màu viền khi hover (cho phần Game Feel)
    }
}
