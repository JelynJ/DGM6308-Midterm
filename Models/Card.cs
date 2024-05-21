using System;

namespace WebAssembly.Models
{
    
        public enum Suit
        {
            Clubs = '♣',
            Diamonds = '♦',
            Hearts = '♥',
            Spades = '♠'
        }

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

        public class Card
        {
            public Suit Suit { get; set; }
            public Rank Rank { get; set; }
            public string ImagePath { get; set; }


        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
            ImagePath = $"images/cards/card_{GetSuitName(suit)}_{GetRankName(rank)}.png";
        }

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

        public override string ToString()
            {
                string rankString = "";
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
                return $"{(char)Suit}{rankString}";
            }

    }
    

}
