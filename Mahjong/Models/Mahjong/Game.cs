using System;
using System.ComponentModel.DataAnnotations;

namespace Mahjong.Models.Mahjong
{
    public class Game
    {
        [Required]
        public int Player1Id { get; set; }
        [Required]
        public int Player2Id { get; set; }
        [Required]
        public int Player3Id { get; set; }
        [Required]
        public int Player4Id { get; set; }
        [Required]
        public Decimal HuaValue { get; set; }
        [Required]
        public Decimal LeziValue { get; set; }
    }
}