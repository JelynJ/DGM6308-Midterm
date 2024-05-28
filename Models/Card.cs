using System;

namespace WebAssembly.Models
{
    // Enum representing the suits of a deck of cards
    public enum Suit
    {
        Clubs = '♣',
        Diamonds = '♦',
        Hearts = '♥',
        Spades = '♠'
    }

    // Enum representing the ranks of a deck of cards
    public enum Rank
    {
        Three = 3,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace,
        Two
    }

    // Class representing a single playing card
    public class Card
    {
        // Property representing the suit of the card
        public Suit Suit { get; set; }

        // Property representing the rank of the card
        public Rank Rank { get; set; }

        // Property representing the image path of the card
        public string ImagePath { get; set; }

        // Constructor for the Card class
        // Initializes a new instance of the Card class with the specified suit and rank
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
            ImagePath = $"images/cards/card_{GetSuitName(suit)}_{GetRankName(rank)}.png";
        }

        // Private method to get the suit name as a string
        // Returns the string representation of the suit
        private string GetSuitName(Suit suit)
        {
            return suit switch
            {
                Suit.Clubs => "clubs",
                Suit.Diamonds => "diamonds",
                Suit.Hearts => "hearts",
                Suit.Spades => "spades",
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
            };
        }

        // Private method to get the rank name as a string
        // Returns the string representation of the rank
        private string GetRankName(Rank rank)
        {
            return rank switch
            {
                Rank.Ace => "A",
                Rank.Two => "02",
                Rank.Three => "03",
                Rank.Four => "04",
                Rank.Five => "05",
                Rank.Six => "06",
                Rank.Seven => "07",
                Rank.Eight => "08",
                Rank.Nine => "09",
                Rank.Ten => "10",
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        // Override of the ToString method to provide a string representation of the card
        public override string ToString()
        {
            string rankString = "";

            // Switch statement to determine the string representation of the rank
            switch (Rank)
            {
                case Rank.Ace:
                    rankString = "A";
                    break;
                case Rank.Two:
                    rankString = "2";
                    break;
                case Rank.Jack:
                    rankString = "J";
                    break;
                case Rank.Queen:
                    rankString = "Q";
                    break;
                case Rank.King:
                    rankString = "K";
                    break;
                default:
                    rankString = ((int)Rank).ToString();
                    break;
            }

            // Return the string representation of the card in the format "SuitRank"
            return $"{(char)Suit}{rankString}";
        }
    }
}