using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Models
{
    public class Deck
    {
        // Private field to store the cards in the deck
        private List<Card> cards;

        // Private field to store a random number generator
        private Random random;

        // Public property to get the number of remaining cards in the deck
        public int RemainingCards => cards.Count;

        // Constructor for the Deck class
        public Deck()
        {
            // Initialize the cards list
            cards = new List<Card>();

            // Initialize the random number generator
            random = new Random();

            // Initialize the deck with a standard set of cards
            InitializeDeck();
        }

        // Method to initialize the deck with a standard set of cards
        public void InitializeDeck()
        {
            // Clear any existing cards in the deck
            cards.Clear();

            // Loop through each suit in the Suit enum
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                // Loop through each rank in the Rank enum
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    // Create a new card with the current suit and rank, and add it to the deck
                    cards.Add(new Card(suit, rank));
                }
            }
        }

        // Method to shuffle the cards in the deck
        public void Shuffle()
        {
            // Use LINQ to order the cards randomly using the random number generator
            cards = cards.OrderBy(x => random.Next()).ToList();
        }

        // Method to deal a specified number of cards from the top of the deck
        public List<Card> DealCards(int numCards)
        {
            // Get a sublist of the specified number of cards from the top of the deck
            List<Card> dealtCards = cards.GetRange(0, numCards);

            // Remove the dealt cards from the deck
            cards.RemoveRange(0, numCards);

            // Return the dealt cards
            return dealtCards;
        }
    }
}