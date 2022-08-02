using System;
using UnityEngine;

public enum Pile
{
    Top,
    Bottom,
    Discard,
}

public class Card : MonoBehaviour
{
    [SerializeField] private Sprite cardBack;

    public bool faceUp;

    public bool selected;

    public Pile pile;

    private Sprite cardFace;

    private Solitare solitaire;
    
    private SpriteRenderer spriteRenderer;
    
    public string Rank { get => name.Split(" of ")[0]; }
    
    public string Suit { get => name.Split(" of ")[1]; }

    public string Color { get => Suit == "spades" || Suit == "clubs" ? "black" : "red"; }

    public int Value { get => Array.FindIndex(CardStack.ranks, rank => rank == Rank); }

    private static readonly CardStack deck = new(true);

    private void Start()
    {
        solitaire = FindObjectOfType<Solitare>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardFace = solitaire.cardSprites[deck.FindIndex(card => name == card.ToString())];
    }

    private void Update()
    {
        spriteRenderer.sprite = faceUp ? cardFace : cardBack;
        spriteRenderer.color = selected ? UnityEngine.Color.yellow : UnityEngine.Color.white;
    }
}
