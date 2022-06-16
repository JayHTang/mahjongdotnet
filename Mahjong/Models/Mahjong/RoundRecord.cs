using System;
using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class RoundRecord
    {
        public string PlayerName { get; set; }
        public List<GamePayout> Payouts { get; set; }
        public int Count { get; set; }
        public int Rank { get; set; }
    }

    public class GamePayout
    {
        public int GameId { get; set; }
        public string Date { get; set; }
        public decimal Cashflow { get; set; }
    }
}