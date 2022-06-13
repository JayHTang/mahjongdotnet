using System;
using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class Stat
    {
        public int RoundsPlayed { get; set; }
        public int Wins { get; set; }
        public decimal MoneyWon { get; set; }
        public int Loses { get; set; }
        public decimal MoneyLost { get; set; }
        public int Push { get; set; }
        public int Huangfan { get; set; }
        public int ZimoWin { get; set; }
        public decimal ZimoWinMoney { get; set; }
        public int DianpaoWin { get; set; }
        public decimal DianpaoWinMoney { get; set; }
        public int ZimoLose { get; set; }
        public decimal ZimoLoseMoney { get; set; }
        public int DianpaoLose { get; set; }
        public decimal DianpaoLoseMoney { get; set; }
        public int Menqing { get; set; }
        public int Gangkai { get; set; }
        public int Laoyue { get; set; }
        public int QianggangWin { get; set; }
        public int QianggangLose { get; set; }
        public int BaogangWin { get; set; }
        public int BaogangLose { get; set; }
        public int ChengbaoZimoWin { get; set; }
        public int ChengbaoZimoLose { get; set; }
        public int ChengbaoDianpaoWin { get; set; }
        public int Chengbao1Lose { get; set; }
        public int Chengbao2Lose { get; set; }
        public int Dadiaoche { get; set; }
        public int Yipaoliangxiang { get; set; }
        public decimal MenqingMoney { get; set; }
        public decimal GangkaiMoney { get; set; }
        public decimal LaoyueMoney { get; set; }
        public decimal QianggangWinMoney { get; set; }
        public decimal QianggangLoseMoney { get; set; }
        public decimal BaogangWinMoney { get; set; }
        public decimal BaogangLoseMoney { get; set; }
        public decimal ChengbaoZimoWinMoney { get; set; }
        public decimal ChengbaoZimoLoseMoney { get; set; }
        public decimal ChengbaoDianpaoWinMoney { get; set; }
        public decimal Chengbao1LoseMoney { get; set; }
        public decimal Chengbao2LoseMoney { get; set; }
        public decimal DadiaocheMoney { get; set; }
        public decimal YipaoliangxiangMoney { get; set; }
        public Dictionary<int, HandStats> Hands { get; set; }
        public Dictionary<int, decimal> UpMoneyWon { get; set; }
        public Dictionary<int, int> UpMoneyWonCount { get; set; }
        public Dictionary<int, decimal> HeadToHead { get; set; }
        public DateTime LastPlayed { get; set; }
    }

    public class HandStats
    {
        public int Count { get; set; }
        public decimal Money { get; set; }
    }
}