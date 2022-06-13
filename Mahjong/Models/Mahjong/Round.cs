using System;

namespace Mahjong.Models.Mahjong
{
    public class Round
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public decimal Delta1 { get; set; }
        public decimal Delta2 { get; set; }
        public decimal Delta3 { get; set; }
        public decimal Delta4 { get; set; }
        public DateTime Created { get; set; }
        public decimal Winning { get; set; }
    }
}