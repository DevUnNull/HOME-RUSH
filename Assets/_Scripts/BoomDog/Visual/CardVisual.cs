using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // Yêu cầu cài đặt thư viện DOTween
using BoomDog.Data;

namespace BoomDog.Visual
{
    public class CardVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("References")]
        public Image cardImage;
        public GameObject cardBackObj; // Mặt lưng lá bài
        public GameObject cardFrontObj; // Mặt trước lá bài
        public TMPro.TextMeshProUGUI cardNameText; // Dùng tạm text nếu chưa có ảnh
        
        [Header("Config")]
        public float moveDuration = 0.5f;
        public float jumpPower = 50f;
        public float flipDuration = 0.3f;

        private CardData _currentData;
        private bool _isPlayed = false;
        private Vector3 _originalFrontPos;
        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (cardFrontObj != null)
                _originalFrontPos = cardFrontObj.transform.localPosition;
        }

        public void Initialize(CardData data, bool isFaceUp = true)
        {
            _currentData = data;
            
            if (data != null)
            {
                if (cardImage != null && data.cardFrontSprite != null)
                    cardImage.sprite = data.cardFrontSprite;
                    
                if (cardNameText != null)
                    cardNameText.text = data.cardName;
            }

            SetFaceUp(isFaceUp);
        }

        public void SetFaceUp(bool isFaceUp)
        {
            if (cardBackObj != null) cardBackObj.SetActive(!isFaceUp);
            if (cardFrontObj != null) cardFrontObj.SetActive(isFaceUp);
        }

        public void AnimateDrawCard(System.Action onComplete = null)
        {
            if (_rectTransform != null)
            {
                // Thu nhỏ kích thước lá bài khi nằm trên tay để không che màn hình
                _rectTransform.sizeDelta = new Vector2(90, 135); 
            }

            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, moveDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void MarkAsPlayed()
        {
            _isPlayed = true;
        }

        // Animation đánh bài ra giữa bàn
        public void AnimatePlayCard(Vector3 tableCenterPosition, System.Action onComplete = null)
        {
            if (_rectTransform != null)
            {
                // Phóng to kích thước lá bài trở lại bình thường khi nằm ở giữa bàn
                _rectTransform.sizeDelta = new Vector2(130, 200); 
            }

            Sequence seq = DOTween.Sequence();
            
            // Lật mặt bài lên (nếu đang úp)
            seq.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2f).SetEase(Ease.InSine));
            seq.AppendCallback(() => SetFaceUp(true));
            seq.Append(transform.DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), flipDuration / 2f).SetEase(Ease.OutSine));

            // Đập mạnh xuống bàn
            seq.Join(transform.DOJump(tableCenterPosition, jumpPower * 2, 1, moveDuration).SetEase(Ease.InCubic));
            
            // Thu nhỏ lại một chút khi nằm trên bàn
            seq.Join(transform.DOScale(Vector3.one * 0.8f, moveDuration));

            seq.OnComplete(() =>
            {
                // Thêm hiệu ứng rung camera hoặc hạt (Particle) tại đây
                onComplete?.Invoke();
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isPlayed || cardFrontObj == null) return;
            // Nhô bài lên khi hover
            cardFrontObj.transform.DOLocalMoveY(_originalFrontPos.y + 30f, 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isPlayed || cardFrontObj == null) return;
            // Trả về vị trí cũ
            cardFrontObj.transform.DOLocalMoveY(_originalFrontPos.y, 0.2f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isPlayed) return;
            
            // KIỂM TRA LƯỢT ĐÁNH VÀ BỐC BÀI
            if (BoomDog.Core.CardGameManager.Instance != null && BoomDog.Core.CardPlayer.Local != null)
            {
                if (!BoomDog.Core.CardGameManager.Instance.IsMyTurn(BoomDog.Core.CardPlayer.Local.Object.StateAuthority))
                {
                    Debug.Log("Chưa tới lượt của bạn!");
                    return;
                }
                
                if (!BoomDog.Core.CardGameManager.Instance.HasDrawnCardThisTurn)
                {
                    Debug.Log("Bạn phải bốc 1 lá trước khi đánh!");
                    return;
                }
            }

            _isPlayed = true;

            // Xóa luôn lá bài thực tế khỏi tay (Hiệu ứng bay sẽ do RPC_PlayCard lo)
            Destroy(gameObject);
            
            // GỌI LỆNH MẠNG: Xin phép Host cho đánh lá bài này
            if (BoomDog.Core.CardPlayer.Local != null && _currentData != null)
            {
                BoomDog.Core.CardPlayer.Local.RPC_PlayCard(_currentData.id);
            }
        }
    }
}
