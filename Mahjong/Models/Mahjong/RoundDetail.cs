using System;

namespace Mahjong.Models.Mahjong
{
    public class RoundDetail
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public decimal Delta1 { get; set; }
        public decimal Delta2 { get; set; }
        public decimal Delta3 { get; set; }
        public decimal Delta4 { get; set; }
        public int WinnerId { get; set; } = -1;
        public int HuaCount { get; set; } = -1;
        public int HandId { get; set; } = -1;
        public bool Lezi { get; set; }
        public bool Menqing { get; set; }
        public bool Zimo { get; set; }
        public int DianpaoId { get; set; } = -1;
        public bool Qianggang { get; set; }
        public bool Huangfan { get; set; }
        public bool Gangkai { get; set; }
        public bool Laoyue { get; set; }
        public int ChengbaoId { get; set; } = -1;
        public DateTime Created { get; set; }
        public decimal Winning { get; set; }
        public string Description { get; set; }
    }
}