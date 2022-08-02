using System;
using System.Collections.Generic;

public class CardStack
{
    public static readonly string[] suits = new string[] { "clubs", "diamonds", "hearts", "spades" }; // tref, kocka, herc, pik
    public static readonly string[] ranks = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private List<string> Cards { get; } = new();

    public int CardCount { get => Cards.Count; }

    public bool IsEmpty { get => CardCount == 0; }

    public CardStack(bool isDeck = false)
    {
        if (isDeck)
            foreach (string suit in suits)
                foreach (string rank in ranks)
                    Cards.Add(rank + " of " + suit);
    }

    public void Shuffle()
    {
        int n = Cards.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (Cards[n], Cards[k]) = (Cards[k], Cards[n]);
        }
    }

    public string Pop()
    {
        var card = Cards[0];
        Cards.RemoveAt(0);
        return card;
    }

    public void Add(string card)
    {
        Cards.Add(card);
    }

    public int FindIndex(Predicate<string> match)
    {
        return Cards.FindIndex(match);
    }
}
