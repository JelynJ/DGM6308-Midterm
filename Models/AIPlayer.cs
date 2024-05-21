using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Models
{
    public class AIPlayer : Player
    {
        public AIPlayer(string name) : base(name) { }

        public List<Card> PlayTurn(List<Card> currentPlay)
        {
            if (currentPlay.Count == 0)
            {
                return Hand.Take(1).ToList();
            }

            var bomb = Hand.GroupBy(x => x.Rank)
                           .Where(g => g.Count() == 4)
                           .FirstOrDefault();
            if (bomb != null)
            {
                return bomb.ToList();
            }

            var sameCountCards = Hand.GroupBy(x => x.Rank)
                                     .Where(g => g.Count() == currentPlay.Count)
                                     .SelectMany(g => g)
                                     .ToList();
            if (sameCountCards.Count >= currentPlay.Count)
            {
                var higherCards = sameCountCards.Where(card => card.Rank > currentPlay.Max(x => x.Rank))
                                                .Take(currentPlay.Count)
                                                .ToList();
                if (higherCards.Count == currentPlay.Count)
                {
                    return higherCards;
                }
            }

            var sortedHand = Hand.OrderBy(x => x.Rank).ToList();
            for (int i = 0; i <= sortedHand.Count - currentPlay.Count; i++)
            {
                var sequence = sortedHand.Skip(i).Take(currentPlay.Count).ToList();
                if (IsSequenceValid(sequence, currentPlay))
                {
                    return sequence;
                }
            }

            return new List<Card>();
        }

        private bool IsSequenceValid(List<Card> sequence, List<Card> currentPlay)
        {
            if (sequence.Count != currentPlay.Count)
            {
                return false;
            }

            var sortedSequence = sequence.OrderBy(x => x.Rank).ToList();
            var sortedCurrentPlay = currentPlay.OrderBy(x => x.Rank).ToList();

            for (int i = 0; i < sortedSequence.Count - 1; i++)
            {
                if (sortedSequence[i].Rank + 1 != sortedSequence[i + 1].Rank)
                {
                    return false;
                }
            }

            return sortedSequence[0].Rank > sortedCurrentPlay[0].Rank;
        }
    }
}
