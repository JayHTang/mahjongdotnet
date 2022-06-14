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
        public int ChengbaoWin1 { get; set; }
        public int ChengbaoWin2 { get; set; }
        public int ChengbaoWin5 { get; set; }
        public int ChengbaoWin8 { get; set; }
        public int ChengbaoWin10 { get; set; }
        public int ChengbaoWin13 { get; set; }
        public int ChengbaoWin15 { get; set; }
        public int ChengbaoWin18 { get; set; }
        public int ChengbaoLose1 { get; set; }
        public int ChengbaoLose2 { get; set; }
        public int ChengbaoLose5 { get; set; }
        public int ChengbaoLose8 { get; set; }
        public int Dadiaoche { get; set; }
        public int Yipaoliangxiang { get; set; }
        public decimal MenqingMoney { get; set; }
        public decimal GangkaiMoney { get; set; }
        public decimal LaoyueMoney { get; set; }
        public decimal QianggangWinMoney { get; set; }
        public decimal QianggangLoseMoney { get; set; }
        public decimal BaogangWinMoney { get; set; }
        public decimal BaogangLoseMoney { get; set; }
        public decimal ChengbaoWinMoney { get; set; }
        public decimal ChengbaoWin1Money { get; set; }
        public decimal ChengbaoWin2Money { get; set; }
        public decimal ChengbaoWin5Money { get; set; }
        public decimal ChengbaoWin8Money { get; set; }
        public decimal ChengbaoWin10Money { get; set; }
        public decimal ChengbaoWin13Money { get; set; }
        public decimal ChengbaoWin15Money { get; set; }
        public decimal ChengbaoWin18Money { get; set; }
        public decimal ChengbaoLose1Money { get; set; }
        public decimal ChengbaoLose2Money { get; set; }
        public decimal ChengbaoLose5Money { get; set; }
        public decimal ChengbaoLose8Money { get; set; }
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