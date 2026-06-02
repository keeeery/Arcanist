using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHand : MonoBehaviour
{
    [Header("배치")]
    [SerializeField] private float cardSpacing = 110f;
    [SerializeField] private float hoverPushDistance = 45f;
    [SerializeField] private float hoverUpDistance = 60f;
    [SerializeField] private float hoverScale = 1.25f;
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float handYOffset = 40f;
    [SerializeField] private float maxCurvePower = 8f;
    [SerializeField] private float minCurvePower = 3f;
    [SerializeField] private float maxRotationPower = 4.5f;
    [SerializeField] private float minRotationPower = 2f;
    [SerializeField] private int curveSoftStartCount = 5;
    [SerializeField] private int curveSoftEndCount = 10;

    [Header("드로우")]
    [SerializeField] private RectTransform cardPrefab;
    [SerializeField] private Button drawButton;
    [SerializeField] private int initialDrawCount = 5;

    [Header("카드 사용 연출")]
    [SerializeField] private RectTransform playArea;
    [SerializeField] private Color hoverOutlineColor = new Color(1f, 0.95f, 0.1f, 1f);
    [SerializeField] private float playScale = 1.6f;
    [SerializeField] private float playMoveDuration = 0.25f;
    [SerializeField] private float playHoldDuration = 0.25f;
    [SerializeField] private float settleDuration = 0.35f;

    private readonly List<RectTransform> cards = new();
    private readonly List<Vector2> basePositions = new();
    private readonly List<float> baseRotations = new();

    private int hoverIndex = -1;
    private bool isPlayingCard;

    private void Start()
    {
        AutoAssignDrawButton();
        CacheCardPrefabFromChild();

        RefreshCards();
        ArrangeCards();

        if (drawButton != null)
            drawButton.onClick.AddListener(DrawCard);

        for (int i = 0; i < initialDrawCount; i++)
        {
            DrawCard();
        }
    }

    private void Update()
    {
        MoveCards();
    }

    private void CacheCardPrefabFromChild()
    {
        if (cardPrefab == null && transform.childCount > 0)
            cardPrefab = transform.GetChild(0).GetComponent<RectTransform>();

        if (cardPrefab != null)
            cardPrefab.gameObject.SetActive(false);
    }

    public void RefreshCards()
    {
        cards.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform card = transform.GetChild(i).GetComponent<RectTransform>();
            if (card == cardPrefab || !card.gameObject.activeSelf)
                continue;

            int cardIndex = cards.Count;
            cards.Add(card);

            CardHover hover = card.GetComponent<CardHover>();
            if (hover == null)
                hover = card.gameObject.AddComponent<CardHover>();

            hover.Init(this, cardIndex);

            Outline outline = card.GetComponent<Outline>();
            if (outline == null)
            {
                outline = card.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(4f, -4f);
            }

            outline.effectColor = hoverOutlineColor;
            outline.enabled = false;
        }
    }

    public void ArrangeCards()
    {
        basePositions.Clear();
        baseRotations.Clear();

        int count = cards.Count;
        float startX = -(count - 1) * cardSpacing * 0.5f;
        float center = (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * cardSpacing;
            float distanceFromCenter = i - center;
            float softenRatio = Mathf.InverseLerp(curveSoftStartCount, curveSoftEndCount, count);
            float curvePower = Mathf.Lerp(maxCurvePower, minCurvePower, softenRatio);
            float rotationPower = Mathf.Lerp(maxRotationPower, minRotationPower, softenRatio);

            float y = handYOffset - (distanceFromCenter * distanceFromCenter) * curvePower;

            basePositions.Add(new Vector2(x, y));

            float rotation = distanceFromCenter * -rotationPower;
            baseRotations.Add(rotation);
        }
    }

    public void DrawCard()
    {
        if (cardPrefab == null)
            return;

        RectTransform newCard = Instantiate(cardPrefab, transform);
        newCard.gameObject.SetActive(true);
        newCard.name = $"Card_{cards.Count + 1}";
        newCard.localScale = Vector3.one;
        newCard.localRotation = Quaternion.identity;
        newCard.anchoredPosition = Vector2.zero;

        RefreshCards();
        ArrangeCards();
    }

    public void OnCardHover(int index)
    {
        if (isPlayingCard)
            return;

        hoverIndex = index;
    }

    public void OnCardExit()
    {
        if (isPlayingCard)
            return;

        hoverIndex = -1;
    }

    public void OnCardClick(int index)
    {
        if (isPlayingCard)
            return;

        if (index < 0 || index >= cards.Count)
            return;

        StartCoroutine(MoveClickedCardToCenter(cards[index]));
    }

    private IEnumerator MoveClickedCardToCenter(RectTransform card)
    {
        isPlayingCard = true;
        hoverIndex = -1;

        Outline outline = card.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        CardHover hover = card.GetComponent<CardHover>();
        if (hover != null)
            Destroy(hover);

        Button button = card.GetComponent<Button>();
        if (button != null)
        {
            button.transition = Selectable.Transition.None;
            button.interactable = false;
        }

        Canvas canvas = card.GetComponent<Canvas>();
        if (canvas == null)
            canvas = card.gameObject.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        Vector2 startPosition = card.anchoredPosition;
        Vector3 startScale = card.localScale;
        Quaternion startRotation = card.localRotation;

        Vector2 targetPosition = GetPlayAreaLocalPosition();
        Vector3 targetScale = Vector3.one * playScale;
        Quaternion targetRotation = Quaternion.identity;

        float time = 0f;

        while (time < playMoveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / playMoveDuration);
            t = t * t * (3f - 2f * t);

            card.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            card.localScale = Vector3.Lerp(startScale, targetScale, t);
            card.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        card.anchoredPosition = targetPosition;
        card.localScale = targetScale;
        card.localRotation = targetRotation;

        yield return new WaitForSeconds(playHoldDuration);

        Vector3 settleStartScale = card.localScale;
        Vector3 settleTargetScale = Vector3.one;
        float settleTime = 0f;

        while (settleTime < settleDuration)
        {
            settleTime += Time.deltaTime;
            float t = Mathf.Clamp01(settleTime / settleDuration);
            t = t * t * (3f - 2f * t);

            card.localScale = Vector3.Lerp(settleStartScale, settleTargetScale, t);
            card.anchoredPosition = targetPosition;
            card.localRotation = targetRotation;

            yield return null;
        }

        card.localScale = settleTargetScale;
        card.anchoredPosition = targetPosition;
        card.localRotation = targetRotation;

        RectTransform centerParent = playArea != null ? playArea : transform.parent.GetComponent<RectTransform>();

        Transform existingCenterCard = centerParent.Find("CenterCard");
        if (existingCenterCard != null)
            Destroy(existingCenterCard.gameObject);

        RectTransform centerCard = Instantiate(card, centerParent);
        centerCard.name = "CenterCard";
        centerCard.anchorMin = new Vector2(0.5f, 0.5f);
        centerCard.anchorMax = new Vector2(0.5f, 0.5f);
        centerCard.pivot = new Vector2(0.5f, 0.5f);
        centerCard.anchoredPosition = Vector2.zero;
        centerCard.localScale = Vector3.one;
        centerCard.localRotation = targetRotation;
        centerCard.SetAsLastSibling();

        ResetCardVisual(centerCard);

        cards.Remove(card);
        Destroy(card.gameObject);

        yield return null;

        RefreshCards();
        ArrangeCards();

        isPlayingCard = false;
    }

    private void ResetCardVisual(RectTransform card)
    {
        Image[] images = card.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.color = Color.white;
        }

        Button button = card.GetComponent<Button>();
        if (button != null)
        {
            button.transition = Selectable.Transition.None;
            button.interactable = false;
        }

        CardHover hover = card.GetComponent<CardHover>();
        if (hover != null)
            Destroy(hover);

        Outline outline = card.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        Canvas canvas = card.GetComponent<Canvas>();
        if (canvas != null)
            Destroy(canvas);
    }

    private Vector2 GetPlayAreaLocalPosition()
    {
        if (playArea == null)
            return Vector2.zero;

        Vector3 worldPosition = playArea.position;
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        return new Vector2(localPosition.x, localPosition.y);
    }

    private void MoveCards()
    {
        if (isPlayingCard)
            return;

        if (cards.Count != basePositions.Count || cards.Count != baseRotations.Count)
            return;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null)
            {
                RefreshCards();
                ArrangeCards();
                return;
            }

            Vector2 targetPos = basePositions[i];
            float targetScale = 1f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, baseRotations[i]);

            if (hoverIndex != -1)
            {
                if (i < hoverIndex)
                    targetPos.x -= hoverPushDistance;
                else if (i > hoverIndex)
                    targetPos.x += hoverPushDistance;
                else
                {
                    targetPos.y += hoverUpDistance;
                    targetScale = hoverScale;
                    targetRotation = Quaternion.identity;
                }
            }

            cards[i].anchoredPosition = Vector2.Lerp(
                cards[i].anchoredPosition,
                targetPos,
                Time.deltaTime * moveSpeed
            );

            cards[i].localScale = Vector3.Lerp(
                cards[i].localScale,
                Vector3.one * targetScale,
                Time.deltaTime * moveSpeed
            );

            cards[i].localRotation = Quaternion.Lerp(
                cards[i].localRotation,
                targetRotation,
                Time.deltaTime * moveSpeed
            );

            Outline outline = cards[i].GetComponent<Outline>();
            outline.enabled = i == hoverIndex;
        }
    }

    private void AutoAssignDrawButton()
    {
        if (drawButton != null)
            return;

        Transform parent = transform.parent;
        if (parent == null)
            return;

        Transform buttonTransform = parent.Find("DrawButton");
        if (buttonTransform == null)
            return;

        drawButton = buttonTransform.GetComponent<Button>();
    }
}