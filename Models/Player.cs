using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Models
{
    public class Player
    {
        // Property representing the name of the player
        public string Name { get; set; }

        // Property representing the player's hand (list of cards)
        public List<Card> Hand { get; set; }

        // Property representing the selected cards in the player's hand
        // Each element corresponds to a card in the Hand list
        // If an element is true, the corresponding card is selected
        public bool[] SelectedCards { get; set; }

        // Constructor for the Player class
        // Initializes a new instance of the Player class with the specified name
        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            SelectedCards = new bool[0];
        }

        // Method to sort the player's hand
        // Sorts the cards in the hand by rank and then by suit
        public void SortHand()
        {
            Hand = Hand.OrderBy(x => x.Rank).ThenBy(x => x.Suit).ToList();
        }

        // Method to discard cards from the player's hand
        // Removes the specified cards from the player's hand
        public void DiscardCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                Hand.Remove(card);
            }
            UpdateSelectedCardsLength();
        }

        // Method to receive cards and add them to the player's hand
        public void ReceiveCards(List<Card> cards)
        {
            Hand.AddRange(cards);
            UpdateSelectedCardsLength();
        }

        // Method to draw cards from the deck and add them to the player's hand
        public void DrawCards(List<Card> cards)
        {
            Hand.AddRange(cards);
            UpdateSelectedCardsLength();
        }

        // Private method to update the length of the SelectedCards array
        // Ensures that the length of SelectedCards matches the number of cards in the player's hand
        private void UpdateSelectedCardsLength()
        {
            SelectedCards = new bool[Hand.Count];
        }
    }
}