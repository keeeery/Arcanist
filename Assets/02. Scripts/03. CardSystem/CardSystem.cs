using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Arcanist
{
    public class CardSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardGaugeController cardGaugeController;
        [SerializeField] private CardHandUI cardHandUI;

        private bool isGaugeFull;

        private void Awake()
        {
            if (cardGaugeController == null)
                cardGaugeController = GetComponent<CardGaugeController>();

            if (cardHandUI == null)
                cardHandUI = GetComponent<CardHandUI>();
        }

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Q))
                UseCard(0);

            if (SimpleInput.GetKeyDown(Key.W))
                UseCard(1);

            if (SimpleInput.GetKeyDown(Key.E))
                UseCard(2);

            TryDrawIfReady();
        }

        public void OnGaugeFull()
        {
            isGaugeFull = true;
            TryDrawIfReady();
        }

        private void TryDrawIfReady()
        {
            if (isGaugeFull == false) return;

            if (cardHandUI == null || cardHandUI.IsDrawing) return;

            if (cardHandUI.HasEmptySlot == false) return;

            bool drawStarted = cardHandUI.TryDrawToFirstEmptySlot(OnCardDrawCompleted);

            if (drawStarted) isGaugeFull = false;
        }

        private void OnCardDrawCompleted()
        {
            if (cardGaugeController != null)
                cardGaugeController.ResetGaugeAndRecharge();
        }

        private void UseCard(int slotIndex)
        {
            if (cardHandUI == null)
                return;

            cardHandUI.UseCard(slotIndex);
            TryDrawIfReady();
        }
    }
}
