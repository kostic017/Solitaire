using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solitare : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    [Header("Piles")]
    [SerializeField] private Transform deckPile;
    [SerializeField] private Transform discardPile;
    [SerializeField] private Transform[] bottomPiles;

    [Header("Sprites")]
    [SerializeField] private Sprite emptySpotSprite;
    [SerializeField] private Sprite cardBackSprite;

    public Sprite[] cardSprites;

    private Card selectedCard;

    private float discardPileZOffset = 0f;
        
    private SpriteRenderer deckSpriteRenderer;

    private readonly CardStack deck = new(true);

    private void Start()
    {
        deck.Shuffle();
        deckSpriteRenderer = deckPile.gameObject.GetComponent<SpriteRenderer>();
        StartCoroutine(nameof(Deal));
    }

    private void Update()
    {   
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Deck"))
                    OnClick_Deck();
                else if (hit.collider.CompareTag("Card"))
                    OnClick_Card(hit.collider.gameObject.GetComponent<Card>());
                else if (hit.collider.CompareTag("Top"))
                    OnClick_Top(hit.collider.gameObject);
                else if (hit.collider.CompareTag("Bottom"))
                    OnClick_Bottom(hit.collider.gameObject);
            }
        }
    }

    private void OnClick_Deck()
    {
        if (deck.IsEmpty)
        {
            discardPileZOffset = 0f;
            foreach (Transform card in discardPile)
            {
                deck.Add(card.gameObject.name);
                Destroy(card.gameObject);
            }
        }
        else
        {
            var newCard = Instantiate(cardPrefab, new Vector3(discardPile.position.x, discardPile.position.y, discardPileZOffset), Quaternion.identity, discardPile);
            newCard.name = deck.Pop();
            var cardComponent = newCard.GetComponent<Card>();
            cardComponent.faceUp = true;
            cardComponent.pile = Pile.Discard;
            discardPileZOffset -= 0.1f;
            
            if (selectedCard != null)
            {
                selectedCard.selected = false;
                selectedCard = null;
            }
        }
        deckSpriteRenderer.sprite = deck.IsEmpty ? emptySpotSprite : cardBackSprite;
    }

    private void OnClick_Card(Card clickedCard)
    {
        if (!clickedCard.faceUp)
            return;

        if (selectedCard == null)
        {
            clickedCard.selected = true;
            selectedCard = clickedCard;
            return;
        }
     
        if (CanStackOnTop(clickedCard))
        {
            var oldColumn = selectedCard.transform.parent;
            
            if (clickedCard.pile == Pile.Bottom)
            {
                MoveCardStack(clickedCard.gameObject, oldColumn, false);
            }
            else if (clickedCard.pile == Pile.Top)
            {
                if (selectedCard.transform.GetSiblingIndex() == oldColumn.childCount - 1)
                {
                    selectedCard.transform.parent = clickedCard.transform.parent;
                    selectedCard.transform.position = clickedCard.transform.position - new Vector3(0f, 0f, 0.1f);
                }
            }
            
            FlipLastCard(oldColumn);
            selectedCard.pile = clickedCard.pile;
        }

        selectedCard.selected = false;
        selectedCard = null;
    }
    
    private void OnClick_Top(GameObject topPile)
    {
        if (selectedCard != null && selectedCard.Rank == "A")
        {
            var oldColumn = selectedCard.transform.parent;
            selectedCard.transform.parent = topPile.transform;
            selectedCard.transform.position = topPile.transform.position;
            FlipLastCard(oldColumn);
            selectedCard.GetComponent<Card>().pile = Pile.Top;
            selectedCard.selected = false;
            selectedCard = null;
        }
    }

    private void OnClick_Bottom(GameObject bottomPile)
    {
        if (selectedCard != null && selectedCard.Rank == "K")
        {
            var oldColumn = selectedCard.transform.parent;
            MoveCardStack(bottomPile, oldColumn, true);
            FlipLastCard(oldColumn);
            selectedCard.selected = false;
            selectedCard = null;
        }
    }

    private void MoveCardStack(GameObject spotToMove, Transform oldColumn, bool emptySpot)
    {
        var lastSpot = spotToMove;
        var cardsToMove = new List<Card>();

        for (int i = selectedCard.transform.GetSiblingIndex(); i < oldColumn.childCount; ++i)
            cardsToMove.Add(oldColumn.GetChild(i).GetComponent<Card>());

        for (int i = 0; i < cardsToMove.Count; ++i)
        {
            var card = cardsToMove[i];
            card.pile = Pile.Bottom;
            card.transform.parent = emptySpot ? spotToMove.transform : spotToMove.transform.parent;
            card.transform.position = lastSpot.transform.position - new Vector3(0f, emptySpot && i == 0 ? 0f : 0.5f, 0.1f);
            lastSpot = card.gameObject;
        }
    }

    private bool CanStackOnTop(Card clickedCard)
    {
        if (clickedCard.pile == Pile.Top && selectedCard.Value == clickedCard.Value + 1 && selectedCard.Suit == clickedCard.Suit)
            return true;
        if (clickedCard.pile == Pile.Bottom && selectedCard.Value == clickedCard.Value - 1 && clickedCard.Color != selectedCard.Color)
            return true;
        return false;
    }

    private void FlipLastCard(Transform column)
    {
        if (selectedCard.pile == Pile.Bottom)
        {
            int lastChildIndex = column.childCount - 1;
            if (lastChildIndex >= 0)
                column.GetChild(lastChildIndex).GetComponent<Card>().faceUp = true;
        }
    }

    private IEnumerator Deal()
    {
        float yOffset = 0f;
        float zOffset = 0f;
        for (int i = 0; i < 7; ++i)
        {
            for (int j = i; j < 7; ++j)
            {
                yield return new WaitForSeconds(0.05f);
                var card = deck.Pop();
                var newCard = Instantiate(
                    cardPrefab,
                    new Vector3(
                        bottomPiles[j].position.x,
                        bottomPiles[j].position.y - yOffset,
                        bottomPiles[j].position.z - zOffset
                    ),
                    Quaternion.identity,
                    bottomPiles[j]
                );
                newCard.name = card;
                var cardComponent = newCard.GetComponent<Card>();
                cardComponent.faceUp = i == j;
                cardComponent.pile = Pile.Bottom;
            }
            yOffset += 0.5f;
            zOffset += 0.1f;
        }
    }
}
