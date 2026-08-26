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
        public CardData CurrentData => _currentData;
        
        private bool _isPlayed = false;
        private bool _isSelected = false;
        public bool IsSelected => _isSelected;
        
        private Vector3 _originalFrontPos;
        private RectTransform _rectTransform;
        private Canvas _canvas;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (cardFrontObj != null)
                _originalFrontPos = cardFrontObj.transform.localPosition;
                
            // Lấy hoặc tự thêm Canvas để can thiệp thứ tự vẽ (hiển thị bài đè lên nhau)
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 0;
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

        public void SetFanOffset(float yOffset, float angle)
        {
            if (cardFrontObj == null || _isPlayed) return;
            
            _originalFrontPos = new Vector3(_originalFrontPos.x, yOffset, _originalFrontPos.z);
            
            cardFrontObj.transform.DOKill(); // Dừng tween đang chạy
            cardFrontObj.transform.localPosition = _originalFrontPos;
            cardFrontObj.transform.localRotation = Quaternion.Euler(0, 0, angle);
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

        public void UnmarkAsPlayed()
        {
            _isPlayed = false;
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
            if (_isPlayed || cardFrontObj == null || _isSelected) return;
            // Nhô bài lên khi hover và ưu tiên hiển thị lên trên cùng
            cardFrontObj.transform.DOLocalMoveY(_originalFrontPos.y + 30f, 0.2f);
            if (_canvas != null) _canvas.sortingOrder = 10;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isPlayed || cardFrontObj == null || _isSelected) return;
            // Trả về vị trí cũ và trả về thứ tự vẽ gốc
            cardFrontObj.transform.DOLocalMoveY(_originalFrontPos.y, 0.2f);
            if (_canvas != null) _canvas.sortingOrder = 0;
        }

        public void SelectCard()
        {
            if (_isPlayed || cardFrontObj == null) return;
            _isSelected = true;
            cardFrontObj.transform.DOLocalMoveY(_originalFrontPos.y + 60f, 0.2f); // Nhô cao hẳn
            if (_canvas != null) _canvas.sortingOrder = 20; // Lên trên cùng
        }

        public void DeselectCard()
        {
            if (_isPlayed || cardFrontObj == null) return;
            _isSelected = false;
            cardFrontObj.transform.DOLocalMoveY(_originalFrontPos.y, 0.2f); // Trả về cũ
            if (_canvas != null) _canvas.sortingOrder = 0;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isPlayed) return;
            
            // KIỂM TRA LƯỢT ĐÁNH
            if (BoomDog.Core.CardGameManager.Instance != null && BoomDog.Core.CardPlayer.Local != null)
            {
                if (!BoomDog.Core.CardGameManager.Instance.IsMyTurn(BoomDog.Core.CardPlayer.Local.Object.StateAuthority))
                {
                    Debug.Log("Chưa tới lượt của bạn!");
                    return;
                }
                
                // Giao việc xử lý click cho Player (để tính toán Combo & Selection)
                BoomDog.Core.CardPlayer.Local.HandleCardClick(this);
            }
        }
    }
}
