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
        [SerializeField] private float cardRiseDistance = 150f;
        [SerializeField] private float drawDuration = 0.3f;
        [SerializeField] private float startYRotation = 85f;
        [SerializeField] private float extraYSpin = 540f;
        [SerializeField] private float startZRotation = -8f;
        [SerializeField] private float overshootY = 0f;
        [SerializeField] private float minFlipScaleX = 0.3f;

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
            if (cardData == null)
            {
                Debug.LogWarning("CardHandUI ::: Cannot draw because CardData is null.");
                return false;
            }

            if (isDrawing)
                return false;

            int slotIndex = GetFirstEmptySlot();

            if (slotIndex < 0)
                return false;

            StartCoroutine(DrawCard(slotIndex, cardData, onCompleted));
            return true;
        }

        public void UseCard(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCards.Length)
                return;

            if (slotCards[slotIndex] == null)
                return;

            // 드로우 코루틴이 카드 RectTransform을 제어하는 중에는 삭제하지 않는다.
            if (isDrawing)
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
            {
                if (slotCards[i] == null)
                    return i;
            }

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
            card.SetAsLastSibling();
            ResetCardTransform(card);

            CardView cardView = card.GetComponent<CardView>();
            if (cardView != null)
                cardView.Setup(cardData);

            Vector2 endPos = Vector2.zero;
            Vector2 startPos = endPos + Vector2.down * cardRiseDistance;
            Vector2 overshootPos = endPos + Vector2.up * overshootY;

            if (IsDestroyed(card))
            {
                EndDrawingWithoutCallback();
                yield break;
            }

            SetAnchoredPosition(card, startPos);
            card.localScale = Vector3.one * 0.92f;
            card.localRotation = Quaternion.Euler(0f, startYRotation + extraYSpin, startZRotation);

            float t = 0f;

            while (t < drawDuration)
            {
                if (IsDestroyed(card))
                {
                    EndDrawingWithoutCallback();
                    yield break;
                }

                t += Time.deltaTime;

                float n = Mathf.Clamp01(t / drawDuration);
                float eased = EaseOutCubic(n);

                if (n < 0.82f)
                {
                    float moveT = EaseOutCubic(n / 0.82f);
                    SetAnchoredPosition(card, Vector2.LerpUnclamped(startPos, overshootPos, moveT));
                }
                else
                {
                    float settleT = EaseOutCubic((n - 0.82f) / 0.18f);
                    SetAnchoredPosition(card, Vector2.LerpUnclamped(overshootPos, endPos, settleT));
                }

                if (IsDestroyed(card))
                {
                    EndDrawingWithoutCallback();
                    yield break;
                }

                float yRotation = Mathf.Lerp(startYRotation + extraYSpin, 0f, eased);
                float zRotation = Mathf.Lerp(startZRotation, 0f, eased);
                card.localRotation = Quaternion.Euler(0f, yRotation, zRotation);

                float scale = Mathf.Lerp(0.92f, 1f, eased);
                float flipScaleX = Mathf.Lerp(minFlipScaleX, 1f, Mathf.Abs(Mathf.Cos(yRotation * Mathf.Deg2Rad)));
                card.localScale = new Vector3(scale * flipScaleX, scale, scale);

                yield return null;
            }

            if (IsDestroyed(card))
            {
                EndDrawingWithoutCallback();
                yield break;
            }

            SetAnchoredPosition(card, endPos);
            card.localScale = Vector3.one;
            card.localRotation = Quaternion.identity;

            isDrawing = false;
            onCompleted?.Invoke();
        }

        private void EndDrawingWithoutCallback()
        {
            isDrawing = false;
        }

        private bool IsDestroyed(RectTransform card)
        {
            return card == null;
        }

        private void ResetCardTransform(RectTransform card)
        {
            if (IsDestroyed(card))
                return;

            card.localScale = Vector3.one;
            card.localRotation = Quaternion.identity;
            card.localPosition = Vector3.zero;
            card.anchoredPosition3D = Vector3.zero;
        }

        private void SetAnchoredPosition(RectTransform card, Vector2 anchoredPosition)
        {
            if (IsDestroyed(card))
                return;

            card.anchoredPosition = anchoredPosition;
            card.localPosition = new Vector3(card.localPosition.x, card.localPosition.y, 0f);
        }

        private float EaseOutCubic(float value)
        {
            value = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - value, 3f);
        }
    }
}
