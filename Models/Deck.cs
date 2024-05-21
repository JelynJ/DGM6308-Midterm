using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Models
{
    public class Deck
    {
        private List<Card> cards;
        private Random random;
        public int RemainingCards => cards.Count;

        public Deck()
        {
            cards = new List<Card>();
            random = new Random();
            InitializeDeck();
        }

        public void InitializeDeck()
        {
            cards.Clear();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }
        }

        public void Shuffle()
        {
            cards = cards.OrderBy(x => random.Next()).ToList();
        }

        public List<Card> DealCards(int numCards)
        {
            List<Card> dealtCards = cards.GetRange(0, numCards);
            cards.RemoveRange(0, numCards);
            return dealtCards;
        }
    }
}
