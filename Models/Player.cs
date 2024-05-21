using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Models
{
    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; }
        public List<Card> DiscardPile { get; set; }

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            DiscardPile = new List<Card>();
        }

        public void SortHand()
        {
            Hand = Hand.OrderBy(x => x.Rank).ThenBy(x => x.Suit).ToList();
        }

        public void DiscardCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                Hand.Remove(card);
                DiscardPile.Add(card);
            }
        }

        public void DrawCards(List<Card> cards)
        {
            Hand.AddRange(cards);
        }
    }
}
