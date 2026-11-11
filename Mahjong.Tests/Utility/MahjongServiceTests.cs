using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mahjong.Models.Mahjong;
using Mahjong.Utility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Mahjong.Tests.Utility;

public class MahjongServiceTests
{
    private const int P1 = 1, P2 = 2, P3 = 3, P4 = 4;

    [Fact]
    public void CalculateWinnings_NormalDianpao_MultiplierZero_UsesHuaFormula()
    {
        var result = new Result
        {
            WinnerId = P1,
            DianpaoId = P2,
            HuaCount = 4, // (4 + 2) * 1.0 = 6.0
            Zimo = false
        };

        var w = MahjongService.CalculateWinnings(result, multiplier: 0, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);

        Assert.Equal(6m, w[P1]);
        Assert.Equal(-6m, w[P2]);
        Assert.Equal(0m, w[P3]);
        Assert.Equal(0m, w[P4]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_NormalZimo_ChargesAllThreeOpponents()
    {
        var result = new Result
        {
            WinnerId = P3,
            HuaCount = 3, // (3 + 2) * 2.0 = 10.0 per opponent
            Zimo = true,
            Menqing = true // * 2 = 20.0 per opponent
        };

        var w = MahjongService.CalculateWinnings(result, multiplier: 0, P1, P2, P3, P4, huaValue: 2m, leziValue: 40m);

        Assert.Equal(60m, w[P3]);
        Assert.Equal(-20m, w[P1]);
        Assert.Equal(-20m, w[P2]);
        Assert.Equal(-20m, w[P4]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_LeziDadiaocheAndDoublings_UsesLeziBase()
    {
        var result = new Result
        {
            WinnerId = P2,
            DianpaoId = P4,
            HuaCount = 1,
            Lezi = true,     // base = 20
            Huangfan = true, // * 2 = 40
            Laoyue = true    // * 2 = 80
        };

        var w = MahjongService.CalculateWinnings(result, multiplier: 0, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);

        Assert.Equal(80m, w[P2]);
        Assert.Equal(-80m, w[P4]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_QianggangDianpao_TriplesAmount()
    {
        var result = new Result
        {
            WinnerId = P1,
            DianpaoId = P3,
            Qianggang = true, // * 3
            Zimo = false
        };

        // multiplier = 2 => base = 20 * 2 = 40; * 3 = 120
        var w = MahjongService.CalculateWinnings(result, multiplier: 2, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);

        Assert.Equal(120m, w[P1]);
        Assert.Equal(-120m, w[P3]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_Songgang_ChargesOnlyDianpaoPlayer()
    {
        var result = new Result
        {
            WinnerId = P4,
            DianpaoId = P1,
            Zimo = true,
            Qianggang = true, // * 3
            Gangkai = true    // * 2 => 20 * 6 = 120
        };

        var w = MahjongService.CalculateWinnings(result, multiplier: 1, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);

        Assert.Equal(120m, w[P4]);
        Assert.Equal(-120m, w[P1]);
        Assert.Equal(0m, w[P2]);
        Assert.Equal(0m, w[P3]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_ZimoChengbao_ChargesFiveShares()
    {
        var result = new Result
        {
            WinnerId = P1,
            ChengbaoId = P2,
            Zimo = true
        };

        var w = MahjongService.CalculateWinnings(result, multiplier: 1, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);

        Assert.Equal(100m, w[P1]);
        Assert.Equal(-100m, w[P2]);
        Assert.Equal(0m, w[P3]);
        Assert.Equal(0m, w[P4]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_SonggangWithChengbao_SplitsEightSharesFiveAndThree()
    {
        var result = new Result
        {
            WinnerId = P1,
            ChengbaoId = P2,
            DianpaoId = P3,
            Zimo = true,
            Qianggang = true,
            Gangkai = true
        };

        // multiplier 1 => base 20 * 2 (Gangkai) = 40; Qianggang *3 then /3 => unit 40; 8*40=320, -5*40=-200, -3*40=-120
        var w = MahjongService.CalculateWinnings(result, multiplier: 1, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);

        Assert.Equal(320m, w[P1]);
        Assert.Equal(-200m, w[P2]);
        Assert.Equal(-120m, w[P3]);
        Assert.Equal(0m, w[P4]);
        Assert.Equal(0m, w.Values.Sum());
    }

    [Fact]
    public void CalculateWinnings_DianpaoWithChengbao_ChargesBothOrDoubleWhenSamePlayer()
    {
        var distinct = new Result
        {
            WinnerId = P1,
            DianpaoId = P2,
            ChengbaoId = P3,
            Zimo = false
        };
        var w1 = MahjongService.CalculateWinnings(distinct, multiplier: 1, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);
        Assert.Equal(40m, w1[P1]);
        Assert.Equal(-20m, w1[P2]);
        Assert.Equal(-20m, w1[P3]);
        Assert.Equal(0m, w1.Values.Sum());

        var sameLoser = new Result
        {
            WinnerId = P1,
            DianpaoId = P2,
            ChengbaoId = P2,
            Zimo = false
        };
        var w2 = MahjongService.CalculateWinnings(sameLoser, multiplier: 1, P1, P2, P3, P4, huaValue: 1m, leziValue: 20m);
        Assert.Equal(40m, w2[P1]);
        Assert.Equal(-40m, w2[P2]);
        Assert.Equal(0m, w2.Values.Sum());
    }

    [Theory]
    [InlineData("11", true, 1, 1)]
    [InlineData("46", true, 4, 6)]
    [InlineData("66", true, 6, 6)]
    [InlineData("07", false, 0, 7)]
    [InlineData("70", false, 7, 0)]
    [InlineData("abc", false, 0, 0)]
    [InlineData("", false, 0, 0)]
    public void ParseDice_ValidatesDiceRange(string raw, bool expectedValid, int expectedD1, int expectedD2)
    {
        bool actual = MahjongService.ParseDice(raw, out int d1, out int d2);

        Assert.Equal(expectedValid, actual);
        if (expectedValid)
        {
            Assert.Equal(expectedD1, d1);
            Assert.Equal(expectedD2, d2);
        }
    }

    [Fact]
    public void IsYiPaoLiangXiang_And_IsMultipleChengBao_VerifyRules()
    {
        var now = DateTime.UtcNow;
        var r1 = new RoundDetail
        {
            Created = now,
            WinnerId = 1,
            DianpaoId = 3,
            ChengbaoId = -1,
            Zimo = false
        };
        var r2 = new RoundDetail
        {
            Created = now.AddSeconds(30),
            WinnerId = 2,
            DianpaoId = 3,
            ChengbaoId = -1,
            Zimo = false
        };
        var rLate = new RoundDetail
        {
            Created = now.AddSeconds(120),
            WinnerId = 2,
            DianpaoId = 3,
            ChengbaoId = -1,
            Zimo = false
        };

        Assert.True(MahjongService.IsYiPaoLiangXiang(r1, r2));
        Assert.False(MahjongService.IsYiPaoLiangXiang(r1, rLate));

        var cb1 = new RoundDetail
        {
            Created = now,
            WinnerId = 1,
            ChengbaoId = 2,
            Zimo = true,
            HandId = 3,
            HuaCount = 5
        };
        var cb2 = new RoundDetail
        {
            Created = now.AddSeconds(20),
            WinnerId = 1,
            ChengbaoId = 4,
            Zimo = true,
            HandId = 3,
            HuaCount = 5
        };
        Assert.True(MahjongService.IsMultipleChengBao(cb1, cb2));
    }

    [Fact]
    public void GetStreakRecords_AssignsCompetitionRanksWithTies()
    {
        var streak = new Dictionary<int, Dictionary<int, RoundRecord>>
        {
            [1] = new Dictionary<int, RoundRecord>
            {
                [5] = new RoundRecord { PlayerName = "A", Count = 5, Payouts = [new GamePayout { GameId = 5, Cashflow = 50m }] }
            },
            [2] = new Dictionary<int, RoundRecord>
            {
                [5] = new RoundRecord { PlayerName = "B", Count = 5, Payouts = [new GamePayout { GameId = 5, Cashflow = 40m }] }
            },
            [3] = new Dictionary<int, RoundRecord>
            {
                [4] = new RoundRecord { PlayerName = "C", Count = 4, Payouts = [new GamePayout { GameId = 5, Cashflow = 30m }] }
            }
        };
        var dates = new Dictionary<int, string> { [5] = "2026-06-10" };

        var ranked = MahjongService.GetStreakRecords(streak, dates, 10);

        Assert.Equal(3, ranked.Count);
        Assert.Equal(1, ranked[0].Rank);
        Assert.Equal(1, ranked[1].Rank);
        Assert.Equal(3, ranked[2].Rank);
        Assert.All(ranked, r => Assert.Equal("2026-06-10", r.Payouts[0].Date));
    }

    [Fact]
    public void GetStatsViewData_CachesMessageAndInvalidatesOnWrite()
    {
        string tempDb = Path.Combine(Path.GetTempPath(), $"mahjong_svc_test_{Guid.NewGuid():N}.db");
        string templatePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Mahjong/App_Data/db/Mahjong.db.template"));
        File.Copy(templatePath, tempDb, overwrite: true);

        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Mahjong"] = $"Data Source={tempDb}",
                    ["AppSettings:mahjong_huaThreshold"] = "0.5",
                    ["AppSettings:mahjong_roundCountThreshold"] = "30",
                    ["AppSettings:mahjong_gameIdThreshold"] = "4",
                    ["AppSettings:mahjong_lastPlayDateThreshold"] = "180"
                })
                .Build();

            using var cache = new MemoryCache(new MemoryCacheOptions());
            var dbUtility = DbFactory.GetDbUtility(Db.SQLite);
            var service = new MahjongService(config, dbUtility, cache);

            var missData = service.GetStatsViewData(0);
            Assert.True(cache.TryGetValue("StatsGlobalViewData", out _));
            Assert.NotNull(missData["message"]);

            var hitData = service.GetStatsViewData(0);
            Assert.Equal(missData["message"], hitData["message"]);

            service.AddPlayer("新测试玩家");
            Assert.False(cache.TryGetValue("StatsGlobalViewData", out _));
        }
        finally
        {
            if (File.Exists(tempDb))
            {
                try { File.Delete(tempDb); } catch { }
            }
        }
    }
}
