using System;
using System.Collections;
using UnityEngine;

namespace Arcanist
{
    public class CardHandUI : MonoBehaviour
    {
        [Header("Card Slots")]
        [SerializeField] private RectTransform[] cardSlots;
        [SerializeField] private RectTransform cardPrefab;

        [Header("Draw Motion")]
        [SerializeField] private float cardRiseDistance = 220f;
        [SerializeField] private float drawDuration = 0.35f;
        [SerializeField] private float startYRotation = 85f;
        [SerializeField] private float extraYSpin = 360f;
        [SerializeField] private float startZRotation = -8f;
        [SerializeField] private float overshootY = 14f;
        [SerializeField] private float minFlipScaleX = 0.18f;

        private CardData[] slotCards;
        private bool isDrawing;

        public bool IsDrawing => isDrawing;
        public bool HasEmptySlot => GetFirstEmptySlot() >= 0;

        private void Awake()
        {
            slotCards = new CardData[cardSlots.Length];
        }

        public bool TryDrawToFirstEmptySlot(CardData cardData, Action onCompleted = null)
        {
            if (isDrawing) return false;

            int slotIndex = GetFirstEmptySlot();

            if (slotIndex < 0) return false;

            StartCoroutine(DrawCard(slotIndex, cardData, onCompleted));
            return true;
        }

        // 기존 CardSystem 코드와 임시 호환용. 실제 덱 시스템 연결 후에는 CardData를 넘기는 버전을 사용한다.
        public bool TryDrawToFirstEmptySlot(Action onCompleted = null)
        {
            return TryDrawToFirstEmptySlot(null, onCompleted);
        }

        public void UseCard(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCards.Length)
                return;

            if (slotCards[slotIndex] == null)
                return;

            CardView cardView = cardSlots[slotIndex].GetComponentInChildren<CardView>();
            if (cardView != null)
                cardView.DebugUse();

            for (int i = cardSlots[slotIndex].childCount - 1; i >= 0; i--)
                Destroy(cardSlots[slotIndex].GetChild(i).gameObject);

            slotCards[slotIndex] = null;
        }

        private int GetFirstEmptySlot()
        {
            for (int i = 0; i < slotCards.Length; i++)
                if (slotCards[i] == null) return i;

            return -1;
        }

        public CardData GetCard(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCards.Length)
                return null;

            return slotCards[slotIndex];
        }

        private IEnumerator DrawCard(int slotIndex, CardData cardData, Action onCompleted)
        {
            isDrawing = true;
            slotCards[slotIndex] = cardData;

            RectTransform slot = cardSlots[slotIndex];
            RectTransform card = Instantiate(cardPrefab, slot);

            card.gameObject.SetActive(true);

            CardView cardView = card.GetComponent<CardView>();
            
            if (cardView != null) cardView.Setup(cardData);

            Vector2 endPos = Vector2.zero;
            Vector2 startPos = endPos + Vector2.down * cardRiseDistance;
            Vector2 overshootPos = endPos + Vector2.up * overshootY;

            card.anchoredPosition = startPos;
            card.localScale = Vector3.one * 0.92f;
            card.localRotation = Quaternion.Euler(0f, startYRotation + extraYSpin, startZRotation);

            float t = 0f;

            while (t < drawDuration)
            {
                t += Time.deltaTime;

                float n = Mathf.Clamp01(t / drawDuration);
                float eased = EaseOutCubic(n);

                if (n < 0.82f)
                {
                    float moveT = EaseOutCubic(n / 0.82f);
                    card.anchoredPosition = Vector2.LerpUnclamped(startPos, overshootPos, moveT);
                }
                else
                {
                    float settleT = EaseOutCubic((n - 0.82f) / 0.18f);
                    card.anchoredPosition = Vector2.LerpUnclamped(overshootPos, endPos, settleT);
                }

                float yRotation = Mathf.Lerp(startYRotation + extraYSpin, 0f, eased);
                float zRotation = Mathf.Lerp(startZRotation, 0f, eased);
                card.localRotation = Quaternion.Euler(0f, yRotation, zRotation);

                float scale = Mathf.Lerp(0.92f, 1f, eased);
                float flipScaleX = Mathf.Lerp(minFlipScaleX, 1f, Mathf.Abs(Mathf.Cos(yRotation * Mathf.Deg2Rad)));
                card.localScale = new Vector3(scale * flipScaleX, scale, scale);

                yield return null;
            }

            card.anchoredPosition = endPos;
            card.localScale = Vector3.one;
            card.localRotation = Quaternion.identity;

            isDrawing = false;
            onCompleted?.Invoke();
        }

        private float EaseOutCubic(float value)
        {
            value = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - value, 3f);
        }
    }
}
