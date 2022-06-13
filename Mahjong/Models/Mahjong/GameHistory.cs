using System;
using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class GameHistory
    {
        public int GameId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Decimal HuaValue { get; set; }
        public Decimal LeziValue { get; set; }
        public TimeSpan Duration()
        {
            return End.Subtract(Start);
        }
        public int NumOfRounds { get; set; }
        public List<int> Players { get; set; }
        public Dictionary<int, Decimal> BalanceSheet { get; set; }
    }
}