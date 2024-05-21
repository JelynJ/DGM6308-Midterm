using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAssembly.Models;

namespace WebAssembly.Services
{
    public class GameService
    {
        private Deck deck;
        private Player human;
        private AIPlayer computer;
        private int currentPlayerIndex;
        private List<Card> currentPlay;
        private bool canPlayAnyCard = false;

        public bool CanPlayAnyCard
        {
            get { return canPlayAnyCard; }
            set { canPlayAnyCard = value; }
        }

        public Player Human => human;
        public AIPlayer Computer => computer;
        public List<Card> CurrentPlay => currentPlay;
        public int CurrentPlayerIndex => currentPlayerIndex;

        public event Action? OnStateChanged;

        public GameService()
        {
            deck = new Deck();
            human = new Player("Player");
            computer = new AIPlayer("Computer");
            currentPlayerIndex = 0;
            currentPlay = new List<Card>();
        }

        public void InitializeGame()
        {
            deck.InitializeDeck();
            deck.Shuffle();

            List<Player> players = new List<Player> { human, computer };
            DealCards(players);
            DetermineStartingPlayer(players);
            NotifyStateChanged();
        }

        private void DealCards(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.Hand = deck.DealCards(17);
                player.SortHand();
            }
        }

        private void DetermineStartingPlayer(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Hand.Any(x => x.Rank == Rank.Three && x.Suit == Suit.Spades))
                {
                    currentPlayerIndex = i;
                    break;
                }
            }

            if (currentPlayerIndex == 0)
            {
                currentPlayerIndex = new Random().Next(players.Count);
            }
        }

        public async Task PlayGameAsync()
        {
            int consecutivePasses = 0;

            while (!CheckGameOver())
            {
                if (currentPlayerIndex == 0)
                {
                    // Waiting for human player to play
                    await Task.Delay(100);
                }
                else
                {
                    await PlayAITurn();
                }

                if (consecutivePasses == 1)
                {
                    currentPlay.Clear();
                    consecutivePasses = 0;
                    canPlayAnyCard = true;
                }

                currentPlayerIndex = (currentPlayerIndex + 1) % 2;
                NotifyStateChanged();
            }

            DetermineWinner();
        }

        private async Task PlayAITurn()
        {
            var play = computer.PlayTurn(currentPlay);

            if (play != null && (CanPlayAnyCard || IsValidPlay(play, currentPlay)))
            {
                computer.DiscardCards(play);
                currentPlay = play;
                CanPlayAnyCard = false;
            }

            await Task.Delay(1000);
        }

        public void HumanPlay(List<Card> play)
        {
            if (IsValidPlay(play, currentPlay) || CanPlayAnyCard)
            {
                human.DiscardCards(play);
                currentPlay = play;
                CanPlayAnyCard = false;
                NotifyStateChanged();
            }
        }

        public void HumanPass()
        {
            NotifyStateChanged();
        }

        private bool IsValidPlay(List<Card> play, List<Card> currentPlay)
        {
            if (currentPlay.Count == 0)
                return true;

            if (play.Count != currentPlay.Count)
                return false;

            if (play.Count == 1)
                return play[0].Rank > currentPlay[0].Rank;

            if (play.Count == 2 && play[0].Rank == play[1].Rank)
                return play[0].Rank > currentPlay[0].Rank;

            if (play.Count == 3 && play[0].Rank == play[1].Rank && play[1].Rank == play[2].Rank)
                return play[0].Rank > currentPlay[0].Rank;

            if (play.Count == 4 && play.Select(x => x.Rank).Distinct().Count() == 1)
                return true;

            var playRanks = play.Select(x => x.Rank).OrderBy(x => x);
            var currentPlayRanks = currentPlay.Select(x => x.Rank).OrderBy(x => x);

            return playRanks.SequenceEqual(currentPlayRanks) && playRanks.First() > currentPlayRanks.First();
        }

        private bool CheckGameOver()
        {
            return human.Hand.Count == 0 || computer.Hand.Count == 0;
        }

        private void DetermineWinner()
        {
            // Notify the UI about the game result
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}
