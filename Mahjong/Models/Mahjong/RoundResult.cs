using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class RoundResult
    {
        public Round Round { get; set; }
        public string Dice { get; set; }
        public List<RoundDetail> RoundDetails { get; set; }
        public string Description { get; set; }
    }
}