namespace Mahjong.Models.Mahjong
{
    public class Result
    {
        public int? WinnerId { get; set; }
        public int? HuaCount { get; set; }
        public int? HandId { get; set; }
        public bool Lezi { get; set; }
        public bool Menqing { get; set; }
        public bool Zimo { get; set; }
        public int? DianpaoId { get; set; }
        public bool Qianggang { get; set; }
        public bool Huangfan { get; set; }
        public bool Gangkai { get; set; }
        public bool Laoyue { get; set; }
        public int? ChengbaoId { get; set; }
        public string Dice { get; set; }
    }
}