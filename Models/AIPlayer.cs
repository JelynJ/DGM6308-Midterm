using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Models
{
    public class AIPlayer : Player
    {
        public AIPlayer(string name) : base(name) { }

        public List<Card> PlayTurn(List<Card> currentPlay, bool canPlayAnyCard)
        {
            if (canPlayAnyCard)
            {
                // If the AI player can play any card freely

                // Check for a bomb (four cards of the same rank)
                var bomb = Hand.GroupBy(x => x.Rank)
                               .Where(g => g.Count() == 4)
                               .FirstOrDefault();
                if (bomb != null)
                {
                    return bomb.ToList();
                }

                // Check for a triplet (three cards of the same rank)
                var triplet = Hand.GroupBy(x => x.Rank)
                                  .Where(g => g.Count() == 3)
                                  .FirstOrDefault();
                if (triplet != null)
                {
                    return triplet.ToList();
                }

                // Check for a pair (two cards of the same rank)
                var pair = Hand.GroupBy(x => x.Rank)
                               .Where(g => g.Count() == 2)
                               .FirstOrDefault();
                if (pair != null)
                {
                    return pair.ToList();
                }

                // Check for sequences (five or more consecutive cards)
                var sequences = FindSequences();
                if (sequences.Count > 0)
                {
                    return sequences.First();
                }

                // Check for three-with-two (three cards of the same rank plus a pair)
                var threeWithTwo = FindThreeWithTwo();
                if (threeWithTwo != null)
                {
                    return threeWithTwo;
                }

                // Check for two pairs (two pairs of consecutive ranks)
                var twoPairs = FindTwoPairs();
                if (twoPairs != null)
                {
                    return twoPairs;
                }

                // If no specific hand is found, play a single card
                return Hand.Take(1).ToList();
            }
            else
            {
                // If the AI player needs to follow the current play

                // Find valid plays based on the current play
                var validPlays = GetValidPlays(currentPlay);
                if (validPlays.Count > 0)
                {
                    return validPlays.First();
                }
                else
                {
                    // If no valid play is found, pass the turn
                    return new List<Card>();
                }
            }
        }

        private List<Card> FindThreeWithTwo()
        {
            // Find a three-with-two hand (three cards of the same rank plus a pair)
            var groupedHand = Hand.GroupBy(x => x.Rank);
            var threeWithTwo = groupedHand.Where(g => g.Count() == 3)
                                          .SelectMany(g => groupedHand.Where(gg => gg.Count() == 2).Select(gg => g.Concat(gg).ToList()))
                                          .FirstOrDefault();
            return threeWithTwo;
        }

        private List<Card> FindTwoPairs()
        {
            // Find two pairs of consecutive ranks
            var groupedHand = Hand.GroupBy(x => x.Rank).OrderBy(g => g.Key);
            var pairs = groupedHand.Where(g => g.Count() == 2).ToList();

            if (pairs.Count >= 2)
            {
                for (int i = 0; i < pairs.Count - 1; i++)
                {
                    if (pairs[i + 1].Key - pairs[i].Key == 1)
                    {
                        var twoPairs = pairs[i].Concat(pairs[i + 1]).ToList();
                        return twoPairs;
                    }
                }
            }

            return null;
        }

        private List<List<Card>> GetValidPlays(List<Card> currentPlay)
        {
            // Find valid plays based on the current play
            var validPlays = new List<List<Card>>();

            if (currentPlay.Count == 1)
            {
                // If the current play is a single card, find higher single cards
                validPlays.AddRange(Hand.Where(card => card.Rank > currentPlay[0].Rank)
                                        .Select(card => new List<Card> { card }));
            }
            else if (currentPlay.Count == 2)
            {
                // If the current play is a pair, find higher pairs
                var currentPairRank = currentPlay[0].Rank;
                validPlays.AddRange(Hand.GroupBy(card => card.Rank)
                                        .Where(group => group.Count() == 2 && group.Key > currentPairRank)
                                        .Select(group => group.ToList()));
            }
            else if (currentPlay.Count == 3)
            {
                // If the current play is a triplet, find higher triplets
                var currentTripletRank = currentPlay[0].Rank;
                validPlays.AddRange(Hand.GroupBy(card => card.Rank)
                                        .Where(group => group.Count() == 3 && group.Key > currentTripletRank)
                                        .Select(group => group.ToList()));
            }
            else if (currentPlay.Count == 4)
            {
                if (currentPlay.Select(x => x.Rank).Distinct().Count() == 1)
                {
                    // If the current play is a bomb, find higher bombs
                    validPlays.AddRange(Hand.GroupBy(card => card.Rank)
                                            .Where(group => group.Count() == 4 && group.Key > currentPlay[0].Rank)
                                            .Select(group => group.ToList()));
                }
                else
                {
                    // If the current play is two pairs, find higher two pairs
                    var currentTwoPairsRanks = currentPlay.GroupBy(x => x.Rank).OrderBy(g => g.Key).Select(g => g.Key).ToList();
                    validPlays.AddRange(Hand.GroupBy(card => card.Rank)
                                            .OrderBy(g => g.Key)
                                            .Where(group => group.Count() == 2)
                                            .SelectMany(group => Hand.GroupBy(card => card.Rank)
                                                                     .OrderBy(gg => gg.Key)
                                                                     .Where(gg => gg.Count() == 2 && gg.Key == group.Key + 1)
                                                                     .Select(gg => group.Concat(gg).ToList())));
                }
            }
            else if (currentPlay.Count == 5)
            {
                if (currentPlay.GroupBy(x => x.Rank).Count() == 2)
                {
                    // If the current play is a three-with-two, find higher three-with-two
                    var currentThreeRank = currentPlay.GroupBy(x => x.Rank).OrderByDescending(g => g.Count()).First().Key;
                    validPlays.AddRange(Hand.GroupBy(card => card.Rank)
                                            .Where(group => group.Count() == 3 && group.Key > currentThreeRank)
                                            .SelectMany(group => Hand.GroupBy(card => card.Rank).Where(gg => gg.Count() == 2 && gg.Key != group.Key).Select(gg => group.Concat(gg).ToList())));
                }
                else
                {
                    // If the current play is a sequence, find higher sequences of the same length
                    var currentPlayRanks = currentPlay.Select(card => card.Rank).OrderBy(rank => rank);
                    validPlays.AddRange(FindSequences().Where(sequence =>
                        sequence.Count == currentPlay.Count &&
                        sequence.Select(card => card.Rank).OrderBy(rank => rank).First() > currentPlayRanks.First()));
                }
            }
            else
            {
                // If the current play is a sequence longer than 5 cards, find higher sequences of the same length
                var currentPlayRanks = currentPlay.Select(card => card.Rank).OrderBy(rank => rank);
                validPlays.AddRange(FindSequences().Where(sequence =>
                    sequence.Count == currentPlay.Count &&
                    sequence.Select(card => card.Rank).OrderBy(rank => rank).First() > currentPlayRanks.First()));
            }

            return validPlays;
        }

        private List<List<Card>> FindSequences()
        {
            // Find all possible sequences in the AI player's hand
            var sequences = new List<List<Card>>();
            var sortedHand = Hand.OrderBy(card => card.Rank).ToList();

            for (int length = 5; length <= 12; length++)
            {
                for (int start = 0; start <= sortedHand.Count - length; start++)
                {
                    var sequence = sortedHand.Skip(start).Take(length).ToList();
                    if (IsSequence(sequence))
                    {
                        sequences.Add(sequence);
                    }
                }
            }

            return sequences;
        }

        private bool IsSequence(List<Card> cards)
        {
            // Check if a given list of cards forms a valid sequence
            if (cards.Count < 5 || cards.Count > 12)
            {
                return false;
            }

            var sortedCards = cards.OrderBy(card => card.Rank).ToList();
            for (int i = 1; i < sortedCards.Count; i++)
            {
                if ((int)sortedCards[i].Rank - (int)sortedCards[i - 1].Rank != 1)
                {
                    return false;
                }
            }

            return true;
        }

    }
}