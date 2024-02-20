using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityToolkit
{
    [DisallowMultipleComponent]
    public class CardSlot : MonoBehaviour, IDropHandler
    {
        private Card _card;
        public Card card => _card;
        public event Action<Card> OnCardPlaced = delegate { };
        public event Action OnCardReturn = delegate { };

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnDrop".Red());
            // Debug.Log($"{gameObject},OnDrop, eventData.pointerDrag: {eventData.pointerDrag}");
            var newCard = eventData.pointerDrag.GetComponent<Card>();
            PlaceCard(newCard);
        }

        public void RemoveCard()
        {
            _card = null;
        }


        public void PlaceCard(Card card)
        {
            if (card == null)
            {
                Debug.LogError("Card is null");
                return;
            }

            _card = card;
            _card.SetNewSlot(this);
            OnCardPlaced(_card);
        }

        public void ReutrnCard()
        {
            OnCardReturn();
        }
    }
}