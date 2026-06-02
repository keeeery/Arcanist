using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private PlayerHand playerHand;
    private int index;

    public void Init(PlayerHand playerHand, int index)
    {
        this.playerHand = playerHand;
        this.index = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        playerHand.OnCardHover(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        playerHand.OnCardExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        playerHand.OnCardClick(index);
    }
}