using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class GameLog
    {
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int Player3Id { get; set; }
        public int Player4Id { get; set; }
        public List<RoundResult> Results { get; set; }
    }
}