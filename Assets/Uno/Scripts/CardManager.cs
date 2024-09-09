using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    public Dictionary<int, Card> allCards = new Dictionary<int, Card>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ensure the manager persists between scenes
        }
        else
        {
            Destroy(gameObject); // Singleton pattern
        }
    }

    // Add a card to the manager's dictionary
    public void RegisterCard(Card card)
    {
        if (!allCards.ContainsKey(card.cardID))
        {
            allCards.Add(card.cardID, card);
        }
    }

    // Get a card by its unique ID
    public Card GetCardById(int cardID)
    {
        if (allCards.ContainsKey(cardID))
        {
            return allCards[cardID];
        }
        return null;
    }
}
