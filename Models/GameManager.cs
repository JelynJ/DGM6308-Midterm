using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WebAssembly.Models
{
    public class GameManager
    {
        // The deck of cards used in the game
        public Deck deck;

        // The human player
        public Player human;

        // The AI player (computer)
        public AIPlayer computer;

        // The index of the current player (0 for human, 1 for computer)
        public int currentPlayerIndex;

        // The current play (cards played by the current player)
        public List<Card> currentPlay;

        // The current play by the human player
        public List<Card> PlayerCurrentPlay { get; private set; }

        // The current play by the computer player
        public List<Card> ComputerCurrentPlay { get; private set; }

        // Indicates whether the game is waiting for the human player to make a move
        public bool WaitingForPlayer { get; set; }

        // Indicates whether the current player can play any card (not restricted by the previous play)
        public bool canPlayAnyCard = false;

        // The discard pile (cards that have been discarded by players)
        public List<Card> DiscardPile { get; private set; }

        // Event raised when the game state changes
        public event Action? GameStateChanged;

        // Event raised when the human player plays cards
        public event Action<List<Card>>? PlayerPlayed;

        // Event raised when the computer player plays cards
        public event Action<List<Card>>? ComputerPlayed;

        // Event raised when the human player passes their turn
        public event Action? PlayerPassed;

        // Event raised when the game is over and a winner is determined
        public event Action<Player>? GameOver;

        // Event raised when the human player's move is requested
        public event Action<string>? PlayerMoveRequested;

        // Event raised when the human player is requested to discard cards
        public event Action<int>? DiscardCardsRequested;

        // Property to get the current player (human or computer)
        public Player CurrentPlayer => currentPlayerIndex == 0 ? human : computer;

        // Constructor for the GameManager class
        public GameManager()
        {
            deck = new Deck();
            human = new Player("Player");
            computer = new AIPlayer("Computer");
            currentPlayerIndex = 0;
            currentPlay = new List<Card>();
            PlayerCurrentPlay = new List<Card>();
            ComputerCurrentPlay = new List<Card>();
            DiscardPile = new List<Card>();
        }

        // Method to initialize the game
        public void InitializeGame()
        {
            deck.InitializeDeck();
            deck.Shuffle();

            List<Player> players = new List<Player> { human, computer };
            DetermineStartingPlayer(players);
            DealCards(players);
        }

        // Method to deal cards to the players
        private void DealCards(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.ReceiveCards(deck.DealCards(17));
                player.SortHand();
            }

            DiscardPile.AddRange(deck.DealCards(18));
        }

        // Method to determine the starting player
        private void DetermineStartingPlayer(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Hand.Any(x => x.Rank == Rank.Three && x.Suit == Suit.Spades))
                {
                    currentPlayerIndex = i;
                    return;
                }
            }

            currentPlayerIndex = Random.Shared.Next(players.Count);
        }

        // Method to process the human player's play cards action
        public void ProcessPlayCards()
        {
            List<Card> selectedCards = GetSelectedCards(human);
            PlayCards(human, selectedCards);
            GameStateChanged?.Invoke();
        }

        // Method to process the human player's pass turn action
        public void ProcessPassTurn()
        {
            PassTurn(human);
            GameStateChanged?.Invoke();
        }

        // Method to process the human player's discard count action
        public void ProcessDiscardCount()
        {
            List<Card> selectedCards = GetSelectedCards(human);
            Debug.Assert(selectedCards.Count <= 9, "玩家不能弃置超过9张牌。");
            Debug.Assert(DiscardPile.Count >= selectedCards.Count, "弃牌堆中的牌数量不足。");

            human.DiscardCards(selectedCards);
            DiscardPile.AddRange(selectedCards);

            List<Card> drawnCards = DiscardPile.Take(selectedCards.Count).ToList();
            DiscardPile.RemoveRange(0, selectedCards.Count);

            human.DrawCards(drawnCards);
            human.SortHand();
            DiscardCardsRequested?.Invoke(selectedCards.Count);
        }

        // Method to process the restart game action
        public void ProcessRestartGame()
        {
            RestartGame();
        }

        // Method to process the exit game action
        public void ProcessExitGame()
        {
            // TODO: Handle the "Exit Game" option in the Blazor component
        }

        // Asynchronous method to play the game
        public async Task PlayGame()
        {
            while (!CheckGameOver())
            {
                Player currentPlayer = CurrentPlayer;
                Console.WriteLine($"Current player: {currentPlayer.Name}");

                if (currentPlayer is AIPlayer aiPlayer)
                {
                    List<Card> play = aiPlayer.PlayTurn(currentPlay, canPlayAnyCard);
                    Console.WriteLine($"AI play: {string.Join(", ", play)}");

                    if (play != null && play.Count > 0)
                    {
                        currentPlayer.DiscardCards(play);
                        currentPlay = play;
                        ComputerCurrentPlay = play;
                        canPlayAnyCard = false;
                        ComputerPlayed?.Invoke(play);
                        GameStateChanged?.Invoke();
                    }
                    else
                    {
                        ComputerCurrentPlay.Clear();
                        PlayerPassed?.Invoke();
                        GameStateChanged?.Invoke();
                        canPlayAnyCard = true;
                    }
                }
                else
                {
                    WaitingForPlayer = true;
                    GetPlayerMove(currentPlayer);
                    while (WaitingForPlayer)
                    {
                        await Task.Delay(100);
                    }

                    if (PlayerCurrentPlay.Count > 0)
                    {
                        canPlayAnyCard = false;
                    }
                    else
                    {
                        canPlayAnyCard = true;
                    }
                }

                Console.WriteLine($"Current play: {string.Join(", ", currentPlay)}");
                Console.WriteLine(canPlayAnyCard);

                currentPlayerIndex = (currentPlayerIndex + 1) % 2;
            }

            Player winner = DetermineWinner();
            GameOver?.Invoke(winner);
        }

        // Method to indicate that the human player has made a move
        public void PlayerMoved()
        {
            WaitingForPlayer = false;
        }

        // Method to request the human player's move
        private void GetPlayerMove(Player player)
        {
            PlayerMoveRequested?.Invoke(player.Name);
        }

        // Method to play cards by a player
        private void PlayCards(Player player, List<Card> selectedCards)
        {
            Console.WriteLine($"{player.Name} plays: {string.Join(", ", selectedCards)}");
            if (IsValidPlay(selectedCards, currentPlay))
            {
                player.DiscardCards(selectedCards);
                currentPlay = selectedCards;
                if (player == human)
                {
                    PlayerCurrentPlay = selectedCards;
                    ComputerCurrentPlay.Clear();
                }
                else
                {
                    ComputerCurrentPlay = selectedCards;
                    PlayerCurrentPlay.Clear();
                }
                PlayerPlayed?.Invoke(selectedCards);
            }
            else
            {
                // TODO: Display invalid play information in the Blazor component
            }
        }

        // Method to pass the turn by a player
        private void PassTurn(Player player)
        {
            currentPlay.Clear();
            PlayerCurrentPlay.Clear();
            ComputerCurrentPlay.Clear();
            PlayerPassed?.Invoke();
            GameStateChanged?.Invoke();
            canPlayAnyCard = true;
        }

        // Method to get the selected cards by a player
        private List<Card> GetSelectedCards(Player player)
        {
            List<Card> selectedCards = player.SelectedCards
                .Select((isSelected, index) => (isSelected, index))
                .Where(tuple => tuple.isSelected)
                .Select(tuple => player.Hand[tuple.index])
                .ToList();

            return selectedCards;
        }

        // Method to check if a play is valid
        public bool IsValidPlay(List<Card> play, List<Card> currentPlay)
        {
            if (currentPlay.Count == 0 || canPlayAnyCard)
            {
                if (play.Count == 1 ||
                    (play.Count == 2 && play[0].Rank == play[1].Rank) ||
                    (play.Count == 3 && play[0].Rank == play[1].Rank && play[1].Rank == play[2].Rank) ||
                    (play.Count == 4 && play.Select(x => x.Rank).Distinct().Count() == 1) ||
                    (play.Count == 4 && IsTwoPairs(play)) ||
                    (play.Count >= 5 && IsStraight(play)) ||
                    (play.Count == 5 && IsThreeWithTwo(play)))
                    return true;
                else
                    return false;
            }

            if (play.Count != currentPlay.Count)
            {
                if (play.Count == 4 && play.Select(x => x.Rank).Distinct().Count() == 1)
                    return true;

                return false;
            }

            if (play.Count == 1)
                return play[0].Rank > currentPlay[0].Rank;

            if (play.Count == 2 && play[0].Rank == play[1].Rank)
                return play[0].Rank > currentPlay[0].Rank;

            if (play.Count == 3 && play[0].Rank == play[1].Rank && play[1].Rank == play[2].Rank)
                return play[0].Rank > currentPlay[0].Rank;

            if (play.Count == 4 && play.Select(x => x.Rank).Distinct().Count() == 1)
            {
                if (currentPlay.Count == 4 && currentPlay.Select(x => x.Rank).Distinct().Count() == 1)
                    return play[0].Rank > currentPlay[0].Rank;

                return true;
            }

            if (play.Count == 4 && IsTwoPairs(play))
            {
                if (IsTwoPairs(play) && IsTwoPairs(currentPlay))
                    return play.GroupBy(x => x.Rank).OrderByDescending(g => g.Count()).First().Key > currentPlay.GroupBy(x => x.Rank).OrderByDescending(g => g.Count()).First().Key;
            }

            if (play.Count >= 5)
            {
                if (IsStraight(play) && IsStraight(currentPlay))
                    return play.Select(x => x.Rank).OrderBy(x => x).First() > currentPlay.Select(x => x.Rank).OrderBy(x => x).First();

                if (IsThreeWithTwo(play) && IsThreeWithTwo(currentPlay))
                {
                    var playThreeRank = play.GroupBy(x => x.Rank).OrderByDescending(g => g.Count()).First().Key;
                    var currentPlayThreeRank = currentPlay.GroupBy(x => x.Rank).OrderByDescending(g => g.Count()).First().Key;
                    return playThreeRank > currentPlayThreeRank;
                }
            }

            return false;
        }

        // Method to check if a list of cards is a two-pair
        private bool IsTwoPairs(List<Card> cards)
        {
            if (cards.Count != 4)
            {
                return false;
            }

            var groupedCards = cards.GroupBy(x => x.Rank).OrderBy(g => g.Key);

            if (groupedCards.Count() != 2 || !groupedCards.All(g => g.Count() == 2))
            {
                return false;
            }

            var ranks = groupedCards.Select(g => g.Key).ToList();

            return ranks[1] - ranks[0] == 1;
        }

        // Method to check if a list of cards is a straight
        private bool IsStraight(List<Card> cards)
        {
            if (cards.Count != 5)
                return false;

            var sortedCards = cards.OrderBy(x => x.Rank).ToList();

            for (int i = 1; i < sortedCards.Count; i++)
            {
                if ((int)sortedCards[i].Rank - (int)sortedCards[i - 1].Rank != 1)
                    return false;
            }

            return true;
        }

        // Method to check if a list of cards is a three-with-two
        private bool IsThreeWithTwo(List<Card> cards)
        {
            if (cards.Count != 5)
                return false;

            var groupedCards = cards.GroupBy(x => x.Rank);

            return groupedCards.Count(g => g.Count() == 3) == 1 && groupedCards.Count(g => g.Count() == 2) == 1;
        }

        // Method to check if the game is over
        private bool CheckGameOver()
        {
            return human.Hand.Count == 0 || computer.Hand.Count == 0;
        }

        // Method to determine the winner
        private Player DetermineWinner()
        {
            return human.Hand.Count == 0 ? human : computer;
        }

        // Method to restart the game
        private void RestartGame()
        {
            deck.InitializeDeck();
            deck.Shuffle();
            human.Hand.Clear();
            human.SelectedCards = new bool[0];
            computer.Hand.Clear();
            currentPlay.Clear();
            PlayerCurrentPlay.Clear();
            ComputerCurrentPlay.Clear();
            currentPlayerIndex = 0;
            DiscardPile.Clear();
            DealCards(new List<Player> { human, computer });
            GameStateChanged?.Invoke();
        }
    }
}