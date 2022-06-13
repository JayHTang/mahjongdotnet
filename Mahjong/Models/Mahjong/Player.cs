using System.ComponentModel.DataAnnotations;

namespace Mahjong.Models.Mahjong
{
    public class Player
    {
        public int Id { get; set; } = 0;
        [Required]
        public string Name { get; set; }
        public int Order { get; set; } = 0;
    }
}