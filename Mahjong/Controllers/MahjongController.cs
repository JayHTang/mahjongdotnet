#define SQLITE

using Highsoft.Web.Mvc.Charts;
using Mahjong.Models.Mahjong;
using Mahjong.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;

#if SQLITE
using DbParameter = System.Data.SQLite.SQLiteParameter;
#else
using DbParameter = System.Data.SqlClient.SqlParameter;
#endif

namespace Mahjong.Controllers
{
    public class MahjongController : Controller
    {
        static readonly public decimal huaThreshold = decimal.Parse(ConfigurationManager.AppSettings["mahjong_huaThreshold"]); // be greater than this number to be a valid game
        static readonly public int roundCountThreshold = int.Parse(ConfigurationManager.AppSettings["mahjong_roundCountThreshold"]); // greater than or equal to this number to be a real player
        static readonly public int gameIdThreshold = int.Parse(ConfigurationManager.AppSettings["mahjong_gameIdThreshold"]); // be greater than this number to be a valid game
        static readonly public int lastPlayDateThreshold = int.Parse(ConfigurationManager.AppSettings["mahjong_lastPlayDateThreshold"]); // difference between the last time played and last game in record must be less than this number to be a real player

        [HttpGet]
        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult AddPlayer()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPlayer(Player player)
        {
            if (ModelState.IsValid)
            {
                dbUtility.Insert(sqlServer, SQL.insertPlayer, new DbParameter("@name", player.Name));
                ModelState.Clear();
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult NewGame()
        {

            ViewData["players"] = GetAllPlayersByRoundsPlayed();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult NewGame(Game game)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Game", new
                {
                    gameId = dbUtility.InsertAndGetId(
                    sqlServer,
                    SQL.insertGame,
                    new DbParameter("@player1_id", game.Player1Id),
                    new DbParameter("@player2_id", game.Player2Id),
                    new DbParameter("@player3_id", game.Player3Id),
                    new DbParameter("@player4_id", game.Player4Id),
                    new DbParameter("@hua", game.HuaValue),
                    new DbParameter("@lezi", game.LeziValue)
                )
                });
            }

            ViewData["players"] = GetAllPlayersByRoundsPlayed();

            return View(game);
        }

        [HttpGet]
        public ActionResult Game(int gameId)
        {
            int lastGameId = GetLastGame(gameId);
            if (lastGameId != gameId)
                return RedirectToAction("Game", new { gameId = lastGameId });

            ViewData["gameId"] = gameId;
            DataRow game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
            int player1Id = Convert.ToInt32(game["player1_id"]);
            int player2Id = Convert.ToInt32(game["player2_id"]);
            int player3Id = Convert.ToInt32(game["player3_id"]);
            int player4Id = Convert.ToInt32(game["player4_id"]);

            int round = 1000; // for sorting, not possible to play 1000 rounds a time

            List<Player> players = new List<Player>();

            Dictionary<int, Decimal> balanceSheet = new Dictionary<int, Decimal>
            {
                { player1Id, 0 },
                { player2Id, 0 },
                { player3Id, 0 },
                { player4Id, 0 },
            }; // <id, balance>

            Dictionary<int, string> playerBook = GetPlayerBook();

            players.Add(new Player { Id = player1Id, Name = playerBook[player1Id], Order = round * 10 + 1 });
            players.Add(new Player { Id = player2Id, Name = playerBook[player2Id], Order = round * 10 + 2 });
            players.Add(new Player { Id = player3Id, Name = playerBook[player3Id], Order = round * 10 + 3 });
            players.Add(new Player { Id = player4Id, Name = playerBook[player4Id], Order = round * 10 + 4 });

            List<GameLog> gameLogs = new List<GameLog>();

            // gameLogs are in reverse order, i.e. latest game appears first in the list
            // rounds are in normal order, first round appears first.
            while (true)
            {
                GameLog gameLog = new()
                {
                    Player1Id = player1Id,
                    Player2Id = player2Id,
                    Player3Id = player3Id,
                    Player4Id = player4Id,
                    Results = new List<RoundResult>()
                };

                DataRowCollection roundRows = dbUtility.Read(sqlServer, SQL.getRounds, new DbParameter("@gameId", Convert.ToInt32(game["id"]))).Rows;
                foreach (DataRow dr in roundRows)
                {
                    RoundResult roundResult = GetRoundResult(dr, playerBook);

                    gameLog.Results.Add(roundResult);
                    balanceSheet[player1Id] += roundResult.Round.Delta1;
                    balanceSheet[player2Id] += roundResult.Round.Delta2;
                    balanceSheet[player3Id] += roundResult.Round.Delta3;
                    balanceSheet[player4Id] += roundResult.Round.Delta4;
                }

                gameLogs.Add(gameLog);

                if (game["prev_game_id"] == DBNull.Value)
                {
                    break;
                }
                else
                {
                    game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", Convert.ToInt32(game["prev_game_id"]))).Rows[0];
                    player1Id = Convert.ToInt32(game["player1_id"]);
                    player2Id = Convert.ToInt32(game["player2_id"]);
                    player3Id = Convert.ToInt32(game["player3_id"]);
                    player4Id = Convert.ToInt32(game["player4_id"]);
                    round--;

                    // update order if player already in list
                    // add player to list otherwise
                    if (!UpdatePlayerInList(players, player1Id, round * 10 + 1))
                    {
                        players.Add(new Player { Id = player1Id, Name = playerBook[player1Id], Order = round * 10 + 1 });
                        balanceSheet.Add(player1Id, 0);
                    }
                    if (!UpdatePlayerInList(players, player2Id, round * 10 + 2))
                    {
                        players.Add(new Player { Id = player2Id, Name = playerBook[player2Id], Order = round * 10 + 2 });
                        balanceSheet.Add(player2Id, 0);
                    }
                    if (!UpdatePlayerInList(players, player3Id, round * 10 + 3))
                    {
                        players.Add(new Player { Id = player3Id, Name = playerBook[player3Id], Order = round * 10 + 3 });
                        balanceSheet.Add(player3Id, 0);
                    }
                    if (!UpdatePlayerInList(players, player4Id, round * 10 + 4))
                    {
                        players.Add(new Player { Id = player4Id, Name = playerBook[player4Id], Order = round * 10 + 4 });
                        balanceSheet.Add(player4Id, 0);
                    }
                }
            }

            // sort the player list
            players.Sort((p1, p2) => p1.Order - p2.Order);

            ViewData["playerBook"] = playerBook;
            ViewData["players"] = players;
            ViewData["balanceSheet"] = balanceSheet;
            ViewData["gameLogs"] = gameLogs;

            // Highcharts
            Dictionary<int, List<decimal>> runningBalance = new Dictionary<int, List<decimal>>();// <id, list of accumulating balance>
            foreach (Player player in players)
            {
                runningBalance.Add(player.Id, new List<decimal> { 0 });
            };

            for (int i = gameLogs.Count - 1; i >= 0; i--)
            {
                GameLog gamelog = gameLogs[i];
                foreach (RoundResult roundResult in gamelog.Results)
                {
                    runningBalance[gamelog.Player1Id].Add(runningBalance[gamelog.Player1Id].Last() + roundResult.Round.Delta1);
                    runningBalance[gamelog.Player2Id].Add(runningBalance[gamelog.Player2Id].Last() + roundResult.Round.Delta2);
                    runningBalance[gamelog.Player3Id].Add(runningBalance[gamelog.Player3Id].Last() + roundResult.Round.Delta3);
                    runningBalance[gamelog.Player4Id].Add(runningBalance[gamelog.Player4Id].Last() + roundResult.Round.Delta4);

                    // for players not playing, add previous balance to the running balance
                    foreach (Player player in players)
                    {
                        if (player.Id != gamelog.Player1Id && player.Id != gamelog.Player2Id && player.Id != gamelog.Player3Id && player.Id != gamelog.Player4Id)
                        {
                            runningBalance[player.Id].Add(runningBalance[player.Id].Last());
                        }
                    }
                }
            }

            List<Series> hc_series = new List<Series>();
            foreach (Player player in players)
            {
                List<SplineSeriesData> data = new List<SplineSeriesData>();
                for (int i = 0; i < runningBalance[player.Id].Count; i++)
                {
                    decimal balance = runningBalance[player.Id][i];
                    data.Add(new SplineSeriesData
                    {
                        X = i,
                        Y = (double)balance
                    });
                }
                hc_series.Add(new SplineSeries
                {
                    Name = player.Name,
                    PointStart = 1,
                    Data = data
                });
            }
            ViewData["hc_series"] = hc_series;

            // form
            ViewData["hands"] = GetHands();

            game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
            int huangfanCount = Convert.ToInt32(game["huangfan"]) - Convert.ToInt32(game["huangfan_executed"]);
            Result result = new Result
            {
                Huangfan = huangfanCount > 0
            };
            ViewData["huangfanCount"] = huangfanCount;
            ViewData["finished"] = (bool)game["finished"];

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult Game(int gameId, Result result)
        {
            gameId = GetLastGame(gameId);

            if (ModelState.IsValid)
            {
                DataRow game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
                int player1Id = Convert.ToInt32(game["player1_id"]);
                int player2Id = Convert.ToInt32(game["player2_id"]);
                int player3Id = Convert.ToInt32(game["player3_id"]);
                int player4Id = Convert.ToInt32(game["player4_id"]);
                decimal huaValue = Convert.ToDecimal(game["hua"]);
                decimal leziValue = Convert.ToDecimal(game["lezi"]);
                bool finished = (bool)game["finished"];
                int roundId = 0;

                if (finished)
                {
                    // do nothing
                }
                else if (result.WinnerId != null)
                {
                    // this round
                    RoundDetail roundDetail = new()
                    {
                        WinnerId = result.WinnerId is null ? -1 : (int)result.WinnerId,
                        HuaCount = result.HuaCount is null ? 0 : (int)result.HuaCount,
                        HandId = result.HandId is null ? -1 : (int)result.HandId,
                        Lezi = result.Lezi,
                        Menqing = result.Menqing,
                        Zimo = result.Zimo,
                        DianpaoId = result.DianpaoId is null ? -1 : (int)result.DianpaoId,
                        Qianggang = result.Qianggang,
                        Huangfan = result.Huangfan,
                        Gangkai = result.Gangkai,
                        Laoyue = result.Laoyue,
                        ChengbaoId = result.ChengbaoId is null ? -1 : (int)result.ChengbaoId,
                        Created = DateTime.Now
                    };

                    // find last round played
                    DataRowCollection rows = dbUtility.Read(sqlServer, SQL.getLastRoundDetail, new DbParameter("gameId", gameId)).Rows;
                    bool useSameRound;
                    if (rows.Count == 0)
                    {
                        // new game
                        useSameRound = false;
                    }
                    else
                    {
                        RoundDetail prevRoundDetail = GetRoundDetailFromDataRow(rows[0]);
                        roundId = prevRoundDetail.RoundId;
                        useSameRound = IsYiPaoLiangXiang(prevRoundDetail, roundDetail) || IsMultipleChengBao(prevRoundDetail, roundDetail);
                        if (useSameRound)
                        {
                            result.Huangfan = prevRoundDetail.Huangfan;
                        }
                    }

                    Dictionary<int, decimal> winnings = GetWinnings(gameId, result, player1Id, player2Id, player3Id, player4Id, huaValue, leziValue);

                    // done with winnings
                    if (result.HuaCount == null)
                        result.HuaCount = 0;
                    DbParameter chengbao_id;
                    if (result.ChengbaoId == null)
                        chengbao_id = new DbParameter("@chengbao_id", DBNull.Value);
                    else
                        chengbao_id = new DbParameter("@chengbao_id", result.ChengbaoId);
                    DbParameter dianpao_id;
                    if (result.DianpaoId == null)
                        dianpao_id = new DbParameter("@dianpao_id", DBNull.Value);
                    else
                        dianpao_id = new DbParameter("@dianpao_id", result.DianpaoId);

                    roundId = useSameRound ? roundId : dbUtility.InsertAndGetId(
                            sqlServer,
                            SQL.insertRound,
                            new DbParameter("@game_id", gameId),
                            new DbParameter("@delta1", winnings[player1Id]),
                            new DbParameter("@delta2", winnings[player2Id]),
                            new DbParameter("@delta3", winnings[player3Id]),
                            new DbParameter("@delta4", winnings[player4Id])
                            );

                    dbUtility.Insert(
                            sqlServer,
                            SQL.insertRoundDetail,
                            new DbParameter("@round_id", roundId),
                            new DbParameter("@delta1", winnings[player1Id]),
                            new DbParameter("@delta2", winnings[player2Id]),
                            new DbParameter("@delta3", winnings[player3Id]),
                            new DbParameter("@delta4", winnings[player4Id]),
                            new DbParameter("@winner_id", result.WinnerId),
                            new DbParameter("@hua_count", result.HuaCount),
                            new DbParameter("@hand_id", result.HandId),
                            new DbParameter("@lezi", result.Lezi),
                            new DbParameter("@menqing", result.Menqing),
                            new DbParameter("@zimo", result.Zimo),
                            dianpao_id,
                            new DbParameter("@qianggang", result.Qianggang),
                            new DbParameter("@huangfan", result.Huangfan),
                            new DbParameter("@gangkai", result.Gangkai),
                            new DbParameter("@laoyue", result.Laoyue),
                            chengbao_id
                            );

                    if (useSameRound)
                    {
                        UpdateRound(roundId);
                    }
                    else
                    {
                        // 荒番executed++
                        if (result.Huangfan)
                        {
                            dbUtility.Insert(sqlServer, SQL.updateGameIncrementHuangfanExecuted, new DbParameter("gameId", gameId));
                        }
                    }
                }
                else
                {
                    // 荒番
                    roundId = dbUtility.InsertAndGetId(
                        sqlServer,
                        SQL.insertRound,
                        new DbParameter("@game_id", gameId),
                        new DbParameter("@delta1", DbUtility.Zero),
                        new DbParameter("@delta2", DbUtility.Zero),
                        new DbParameter("@delta3", DbUtility.Zero),
                        new DbParameter("@delta4", DbUtility.Zero)
                        );

                    dbUtility.Insert(
                        sqlServer,
                        SQL.insertRoundDetail,
                        new DbParameter("@round_id", roundId),
                        new DbParameter("@delta1", DbUtility.Zero),
                        new DbParameter("@delta2", DbUtility.Zero),
                        new DbParameter("@delta3", DbUtility.Zero),
                        new DbParameter("@delta4", DbUtility.Zero),
                        new DbParameter("@winner_id", DBNull.Value),
                        new DbParameter("@hua_count", DBNull.Value),
                        new DbParameter("@hand_id", DBNull.Value),
                        new DbParameter("@lezi", false),
                        new DbParameter("@menqing", false),
                        new DbParameter("@zimo", false),
                        new DbParameter("@dianpao_id", DBNull.Value),
                        new DbParameter("@qianggang", false),
                        new DbParameter("@huangfan", false),
                        new DbParameter("@gangkai", false),
                        new DbParameter("@laoyue", false),
                        new DbParameter("@chengbao_id", DBNull.Value)
                        );
                    // 荒番++
                    dbUtility.Insert(sqlServer, SQL.updateGameIncrementHuangfan, new DbParameter("gameId", gameId));
                }

                if (!finished)
                {
                    InsertDice(result.Dice, roundId, gameId, FindLastDicer(gameId, roundId));
                }
            }

            ModelState.Clear();

            return Game(gameId);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int gameId, int roundId, int roundDetailId, Result result)
        {
            if (ModelState.IsValid)
            {
                DataRow game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
                int player1Id = Convert.ToInt32(game["player1_id"]);
                int player2Id = Convert.ToInt32(game["player2_id"]);
                int player3Id = Convert.ToInt32(game["player3_id"]);
                int player4Id = Convert.ToInt32(game["player4_id"]);
                decimal huaValue = Convert.ToDecimal(game["hua"]);
                decimal leziValue = Convert.ToDecimal(game["lezi"]);

                if (result.WinnerId != null)
                {
                    DataRow round = dbUtility.Read(sqlServer, SQL.getRound, new DbParameter("@gameId", gameId), new DbParameter("@id", roundId)).Rows[0];
                    if (Convert.ToInt32(round["delta1"]) == 0
                        & Convert.ToInt32(round["delta2"]) == 0
                        & Convert.ToInt32(round["delta3"]) == 0
                        & Convert.ToInt32(round["delta4"]) == 0)
                    {
                        dbUtility.Update(sqlServer, SQL.updateGameDecrementHuangfan, new DbParameter("gameId", gameId));
                    }

                    Dictionary<int, decimal> winnings = GetWinnings(gameId, result, player1Id, player2Id, player3Id, player4Id, huaValue, leziValue);

                    // done with winnings
                    if (result.HuaCount == null)
                        result.HuaCount = 0;
                    DbParameter chengbao_id;
                    if (result.ChengbaoId == null)
                        chengbao_id = new DbParameter("@chengbao_id", DBNull.Value);
                    else
                        chengbao_id = new DbParameter("@chengbao_id", result.ChengbaoId);
                    DbParameter dianpao_id;
                    if (result.DianpaoId == null)
                        dianpao_id = new DbParameter("@dianpao_id", DBNull.Value);
                    else
                        dianpao_id = new DbParameter("@dianpao_id", result.DianpaoId);
                    // update round detail
                    dbUtility.Update(
                        sqlServer,
                        SQL.updateRoundDetail,
                        new DbParameter("@delta1", winnings[player1Id]),
                        new DbParameter("@delta2", winnings[player2Id]),
                        new DbParameter("@delta3", winnings[player3Id]),
                        new DbParameter("@delta4", winnings[player4Id]),
                        new DbParameter("@winner_id", result.WinnerId),
                        new DbParameter("@hua_count", result.HuaCount),
                        new DbParameter("@hand_id", result.HandId),
                        new DbParameter("@lezi", result.Lezi),
                        new DbParameter("@menqing", result.Menqing),
                        new DbParameter("@zimo", result.Zimo),
                        dianpao_id,
                        new DbParameter("@qianggang", result.Qianggang),
                        new DbParameter("@huangfan", result.Huangfan),
                        new DbParameter("@gangkai", result.Gangkai),
                        new DbParameter("@laoyue", result.Laoyue),
                        chengbao_id,
                        new DbParameter("@id", roundDetailId)
                        );
                }
                else
                {
                    // 荒庄
                    dbUtility.Update(
                        sqlServer,
                        SQL.updateRoundDetail,
                        new DbParameter("@game_id", gameId),
                        new DbParameter("@delta1", DbUtility.Zero),
                        new DbParameter("@delta2", DbUtility.Zero),
                        new DbParameter("@delta3", DbUtility.Zero),
                        new DbParameter("@delta4", DbUtility.Zero),
                        new DbParameter("@winner_id", DBNull.Value),
                        new DbParameter("@hua_count", DBNull.Value),
                        new DbParameter("@hand_id", DBNull.Value),
                        new DbParameter("@lezi", false),
                        new DbParameter("@menqing", false),
                        new DbParameter("@zimo", false),
                        new DbParameter("@dianpao_id", DBNull.Value),
                        new DbParameter("@qianggang", false),
                        new DbParameter("@huangfan", false),
                        new DbParameter("@gangkai", false),
                        new DbParameter("@laoyue", false),
                        new DbParameter("@chengbao_id", DBNull.Value),
                        new DbParameter("@id", roundDetailId)
                        );
                    // 荒番++
                    dbUtility.Update(sqlServer, SQL.updateGameIncrementHuangfan, new DbParameter("gameId", gameId));
                }

                UpdateRound(roundId);
            }
            return RedirectToAction("Game", new { gameId = gameId });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult Sub(int gameId)
        {
            DataRow prevGame = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
            Dictionary<int, Player> playersOnTable = new Dictionary<int, Player>
            {
                {1, new Player{Id = Convert.ToInt32(prevGame["player1_id"])} },
                {2, new Player{Id = Convert.ToInt32(prevGame["player2_id"])} },
                {3, new Player{Id = Convert.ToInt32(prevGame["player3_id"])} },
                {4, new Player{Id = Convert.ToInt32(prevGame["player4_id"])} }
            };

            List<Player> players = new List<Player>();
            DataTable dt = dbUtility.Read(sqlServer, SQL.getPlayers);
            foreach (DataRow dr in dt.Rows)
            {
                int id = Convert.ToInt32(dr["id"]);
                string name = dr["name"].ToString();
                players.Add(new Player
                {
                    Id = id,
                    Name = name
                });
                for (int i = 1; i <= 4; i++)
                {
                    if (playersOnTable[i].Id == id)
                    {
                        playersOnTable[i].Name = name;
                    }
                }
            }

            ViewData["players"] = players;
            ViewData["playersOnTable"] = playersOnTable;
            ViewData["huaValue"] = Convert.ToDecimal(prevGame["hua"]);
            ViewData["leziValue"] = Convert.ToDecimal(prevGame["lezi"]);
            ViewData["prevGameId"] = gameId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult Sub(int gameId, Game game)
        {
            if (ModelState.IsValid)
            {
                FinishGame(gameId);
                DataRow prevGame = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
                int huangfanCount = Convert.ToInt32(prevGame["huangfan"]) - Convert.ToInt32(prevGame["huangfan_executed"]);

                int newGameId = dbUtility.InsertAndGetId(
                    sqlServer,
                    SQL.insertGameWithPrevGame,
                    new DbParameter("@player1_id", game.Player1Id),
                    new DbParameter("@player2_id", game.Player2Id),
                    new DbParameter("@player3_id", game.Player3Id),
                    new DbParameter("@player4_id", game.Player4Id),
                    new DbParameter("@hua", game.HuaValue),
                    new DbParameter("@lezi", game.LeziValue),
                    new DbParameter("@huangfan", huangfanCount),
                    new DbParameter("@prev_game_id", gameId)
                );

                return RedirectToAction("Game", new { gameId = newGameId });
            }

            return Sub(gameId);
        }

        public ActionResult History()
        {
            // look for open games and close them
            foreach (DataRow game in dbUtility.Read(sqlServer, SQL.getGamesUnfinished).Rows){
                int id = Convert.ToInt32(game["id"]);
                DateTime created = Convert.ToDateTime(game["created"]);
                if ((DateTime.Now - created).TotalHours > 24)
                {
                    // no game should last more than 24 hours
                    FinishGame(id);
                }
            }

            ViewData["gameHistories"] = GetGameHistory();

            return View();
        }

        public ActionResult LeaderBoard()
        {
            List<Player> players = GetAllPlayers();
            Dictionary<int, List<SplineSeriesData>> playerBalanceSeriesData = new Dictionary<int, List<SplineSeriesData>>();
            List<Series> series = new List<Series>();
            Dictionary<int, int> playerRoundCount = new Dictionary<int, int>();
            Dictionary<int, decimal> playerBalanceSheet = new Dictionary<int, decimal>();
            DateTime begin = DateTime.Parse(ConfigurationManager.AppSettings["mahjong_begin"]);
            List<string> xAxis = new List<string> { begin.ToShortDateString() };
            Dictionary<int, Dictionary<int, decimal>> playerBalanceSheetByYear = new();
            Dictionary<int, Dictionary<int, int>> playerRoundCountByYear = new();

            foreach (Player player in players)
            {
                playerBalanceSeriesData.Add(player.Id, new List<SplineSeriesData> { new SplineSeriesData { Y = 0 } });
            }

            List<GameHistory> gameHistories = GetGameHistory();
            for (int i = gameHistories.Count - 1; i >= 0; i--)
            {
                GameHistory gameHistory = gameHistories[i];
                if (Skip(gameHistory.GameId, gameHistory.HuaValue))
                {
                    // don't count
                    continue;
                }

                xAxis.Add(gameHistory.Start.ToShortDateString());

                foreach (Player player in players)
                {
                    if (gameHistory.Players.Contains(player.Id))
                    {
                        playerBalanceSeriesData[player.Id].Add(new SplineSeriesData { Y = playerBalanceSeriesData[player.Id].Last().Y.GetValueOrDefault() + (double)gameHistory.BalanceSheet[player.Id] });
                    }
                    else
                    {
                        playerBalanceSeriesData[player.Id].Add(new SplineSeriesData { Y = playerBalanceSeriesData[player.Id].Last().Y });
                    }
                }
            }

            // by year
            List<GameHistory> separateGameHistories = GetGameHistory(false);
            foreach(GameHistory gameHistory in separateGameHistories)
            {
                if (Skip(gameHistory.GameId, gameHistory.HuaValue))
                {
                    // don't count
                    continue;
                }

                if (!playerBalanceSheetByYear.ContainsKey(gameHistory.Start.Year))
                {
                    playerBalanceSheetByYear[gameHistory.Start.Year] = new();
                    playerRoundCountByYear[gameHistory.Start.Year] = new();
                }
                foreach (int playerId in gameHistory.Players)
                {
                    if (playerBalanceSheetByYear[gameHistory.Start.Year].ContainsKey(playerId))
                    {
                        playerBalanceSheetByYear[gameHistory.Start.Year][playerId] += gameHistory.BalanceSheet[playerId];
                        playerRoundCountByYear[gameHistory.Start.Year][playerId] += gameHistory.NumOfRounds;
                    }
                    else
                    {
                        playerBalanceSheetByYear[gameHistory.Start.Year][playerId] = gameHistory.BalanceSheet[playerId];
                        playerRoundCountByYear[gameHistory.Start.Year][playerId] = gameHistory.NumOfRounds;
                    }
                }
            }

            List<int> years = playerBalanceSheetByYear.Keys.ToList();
            years.Sort((y1, y2) => y2 - y1);
            Dictionary<int, List<Player>> rankedPlayersByYear = new();
            foreach (int year in years)
            {
                rankedPlayersByYear[year] = new();
                foreach (Player player in players)
                {
                    if (playerBalanceSheetByYear[year].ContainsKey(player.Id))
                    {
                        rankedPlayersByYear[year].Add(player);
                    }
                }
                rankedPlayersByYear[year].Sort((p1, p2) => (int)(playerBalanceSheetByYear[year][p2.Id] - playerBalanceSheetByYear[year][p1.Id]));
            }

            List<Player> realPlayers = new List<Player>();
            foreach (Player player in players)
            {
                DataRow dr = dbUtility.Read(sqlServer, SQL.getNumOfRoundsPlayed, new DbParameter("@player_id", player.Id)).Rows[0];
                if (dr[0] != DBNull.Value)
                {
                    int count = Convert.ToInt32(dr[0]);
                    DateTime lastPlayed = Convert.ToDateTime(dr[1]);
                    if (count >= roundCountThreshold && (gameHistories.First().Start - lastPlayed).TotalDays < lastPlayDateThreshold)
                    {
                        realPlayers.Add(player);
                        playerRoundCount.Add(player.Id, count);
                        playerBalanceSheet.Add(player.Id, (decimal)playerBalanceSeriesData[player.Id].Last().Y.GetValueOrDefault());
                    }
                }
            }

            realPlayers.Sort((p1, p2) => (int)(playerBalanceSheet[p2.Id] - playerBalanceSheet[p1.Id]));

            foreach (Player player in realPlayers)
            {
                series.Add(new SplineSeries
                {
                    Name = player.Name,
                    Data = playerBalanceSeriesData[player.Id],
                    PointStart = 0
                });
            }

            ViewData["players"] = realPlayers;
            ViewData["series"] = series;
            ViewData["playerRoundCount"] = playerRoundCount;
            ViewData["playerBalanceSheet"] = playerBalanceSheet;
            ViewData["playerBalanceSheetByYear"] = playerBalanceSheetByYear;
            ViewData["playerRoundCountByYear"] = playerRoundCountByYear;
            ViewData["rankedPlayersByYear"] = rankedPlayersByYear;
            ViewData["years"] = years;
            ViewData["xAxis"] = xAxis;
            ViewData["message"] = GetRealPlayerMessage(gameHistories.First().Start);

            return View();
        }

        public ActionResult Stats(int id = 0)
        {
            List<Player> players = GetAllPlayers();
            List<Hand> hands = GetHands();
            DataTable dt = dbUtility.Read(sqlServer, SQL.getRoundsWithGame, new DbParameter("@game_id", id));
            DateTime lastRoundTime = Convert.ToDateTime(dt.Rows[dt.Rows.Count - 1]["created"]);

            // get all round details
            Dictionary<int, List<RoundDetail>> allRoundDetails = new();
            foreach (DataRow r in dbUtility.Read(sqlServer, SQL.getRoundDetail, new DbParameter("@roundId", DbUtility.Zero)).Rows)
            {
                RoundDetail roundDetail = GetRoundDetailFromDataRow(r);
                if (!allRoundDetails.ContainsKey(roundDetail.RoundId))
                {
                    allRoundDetails.Add(roundDetail.RoundId, new List<RoundDetail>());
                }
                allRoundDetails[roundDetail.RoundId].Add(roundDetail);
            }

            // take care of the games with substitutions
            if (id != 0)
            {
                DataTable dt1 = dt;
                while (dt1.Rows[0]["prev_game_id"] != DBNull.Value)
                {
                    // there is a previous game
                    // attach to dt
                    dt1 = dbUtility.Read(sqlServer, SQL.getRoundsWithGame, new DbParameter("@game_id", Convert.ToInt32(dt1.Rows[0]["prev_game_id"])));
                    foreach (DataRow dr in dt1.Rows)
                    {
                        dt.Rows.Add(dr.ItemArray);
                    }
                }
            }
            else
            {
                ViewData["message"] = GetRealPlayerMessage(GetGameHistory().First().Start);
            }

            // initialize stat book
            Dictionary<int, Stat> statBook = new Dictionary<int, Stat>();
            foreach (Player player in players)
            {
                Stat stat = new Stat { Hands = new Dictionary<int, HandStats>(), UpMoneyWon = new Dictionary<int, decimal>(), UpMoneyWonCount = new Dictionary<int, int>(), HeadToHead = new Dictionary<int, decimal>() };
                foreach (Hand hand in hands)
                {
                    stat.Hands.Add(hand.Id, new HandStats());
                }
                statBook.Add(player.Id, stat);
            }

            // go through all rounds of games
            foreach (DataRow dr in dt.Rows)
            {
                // skip rounds under threshold
                if (Skip(Convert.ToInt32(dr["game_id"]), Convert.ToDecimal(dr["hua"])))
                    continue;

                string[] indices = { "player1_id", "player2_id", "player3_id", "player4_id" };
                string[] deltas = { "Delta1", "Delta2", "Delta3", "Delta4" };

                DateTime lastPlayed = Convert.ToDateTime(dr["created"]);

                // players in this round
                int[] playerIds = { Convert.ToInt32(dr["player1_id"]), Convert.ToInt32(dr["player2_id"]), Convert.ToInt32(dr["player3_id"]), Convert.ToInt32(dr["player4_id"]) };

                // 荒庄
                bool isHuangfan = Convert.ToDecimal(dr["delta1"]) == 0m && Convert.ToDecimal(dr["delta2"]) == 0m && Convert.ToDecimal(dr["delta3"]) == 0m && Convert.ToDecimal(dr["delta4"]) == 0m;

                // total stats
                foreach (int playerId in playerIds)
                {
                    // round count
                    Stat stat = statBook[playerId];
                    stat.RoundsPlayed++;
                    if (stat.LastPlayed == null || stat.LastPlayed < lastPlayed)
                        stat.LastPlayed = lastPlayed;

                    if (isHuangfan)
                    {
                        stat.Huangfan++;
                    }

                    decimal cashflow = Convert.ToDecimal(dr[FindDelta(dr, playerId)]);
                    if (cashflow > 0)
                    {
                        // win
                        stat.Wins++;
                        stat.MoneyWon += cashflow;
                    }
                    else if (cashflow < 0)
                    {
                        // lose
                        stat.Loses++;
                        stat.MoneyLost -= cashflow;
                    }

                    // positional stats
                    int upId = -1; // 上家
                    int downId = -1; // 下家
                    int oppId = -1; // 对家
                    if (Convert.ToInt32(dr["player1_id"]) == playerId)
                    {
                        upId = Convert.ToInt32(dr["player4_id"]);
                        downId = Convert.ToInt32(dr["player2_id"]);
                        oppId = Convert.ToInt32(dr["player3_id"]);
                    }
                    else if (Convert.ToInt32(dr["player2_id"]) == playerId)
                    {
                        upId = Convert.ToInt32(dr["player1_id"]);
                        downId = Convert.ToInt32(dr["player3_id"]);
                        oppId = Convert.ToInt32(dr["player4_id"]);
                    }
                    else if (Convert.ToInt32(dr["player3_id"]) == playerId)
                    {
                        upId = Convert.ToInt32(dr["player2_id"]);
                        downId = Convert.ToInt32(dr["player4_id"]);
                        oppId = Convert.ToInt32(dr["player1_id"]);
                    }
                    else if (Convert.ToInt32(dr["player4_id"]) == playerId)
                    {
                        upId = Convert.ToInt32(dr["player3_id"]);
                        downId = Convert.ToInt32(dr["player1_id"]);
                        oppId = Convert.ToInt32(dr["player2_id"]);
                    }

                    if (upId > 0)
                    {
                        AddOrIncrement(stat.UpMoneyWon, upId, cashflow);
                        AddOrIncrement(stat.UpMoneyWonCount, upId, 1);
                    }
                }

                if (isHuangfan)
                    continue;

                // get round details
                int roundId = Convert.ToInt32(dr["id"]);
                List<RoundDetail> roundDetails = allRoundDetails[roundId];
                
                // look for 一炮两响
                if (roundDetails.Count > 1)
                {
                    if (IsYiPaoLiangXiang(roundDetails[0], roundDetails[1]))
                    {
                        statBook[roundDetails[0].DianpaoId].Yipaoliangxiang++;
                        statBook[roundDetails[0].DianpaoId].YipaoliangxiangMoney -= Convert.ToDecimal(dr[FindDelta(dr, roundDetails[0].DianpaoId)]);
                    }
                }

                // go through round details
                for (int i = 0; i < roundDetails.Count; i++)
                {
                    RoundDetail roundDetail = roundDetails[i];
                    // head to head stats
                    for (int p = 0; p < 4; p++)
                    {
                        decimal loss = (decimal)roundDetail.GetType().GetProperty(deltas[p]).GetValue(roundDetail); // reflection trick to get the specific delta
                        if(loss < 0)
                        {
                            AddOrIncrement(statBook[roundDetail.WinnerId].HeadToHead, playerIds[p], -loss);
                            AddOrIncrement(statBook[playerIds[p]].HeadToHead, roundDetail.WinnerId, loss);
                        }
                    }

                    decimal win = (decimal)ReflectionUtility.GetValue(roundDetail, FindDelta(dr, roundDetail.WinnerId), true);
                    decimal dianpaoLoss = roundDetail.DianpaoId == -1 ? 0 : -(decimal)ReflectionUtility.GetValue(roundDetail, FindDelta(dr, roundDetail.DianpaoId), true);

                    // 自摸
                    if (roundDetail.Zimo)
                    {
                        if(i == 0 || !IsMultipleChengBao(roundDetail, roundDetails[i - 1]))
                        {
                            statBook[roundDetail.WinnerId].ZimoWin++;
                        }
                        statBook[roundDetail.WinnerId].ZimoWinMoney += win;
                        foreach(int playerId in playerIds)
                        {
                            if(playerId != roundDetail.WinnerId)
                            {
                                decimal loss = -(decimal)ReflectionUtility.GetValue(roundDetail, FindDelta(dr, playerId), true); 
                                if (loss > 0)
                                {
                                    statBook[playerId].ZimoLose++;
                                    statBook[playerId].ZimoLoseMoney += loss;
                                }
                                else
                                {
                                    statBook[playerId].Push++;
                                }
                            }
                        }
                    }

                    // 点炮
                    else if (roundDetail.DianpaoId != -1)
                    {
                        statBook[roundDetail.WinnerId].DianpaoWin++;
                        statBook[roundDetail.WinnerId].DianpaoWinMoney += win;
                        if(i == 0 || !IsYiPaoLiangXiang(roundDetail, roundDetails[i - 1]))
                            statBook[roundDetail.DianpaoId].DianpaoLose++;
                        string delta = FindDelta(dr, roundDetail.DianpaoId);
                        statBook[roundDetail.DianpaoId].DianpaoLoseMoney += dianpaoLoss;
                        foreach (int playerId in playerIds)
                        {
                            if (playerId != roundDetail.WinnerId && playerId != roundDetail.DianpaoId && playerId != roundDetail.ChengbaoId)
                            {
                                statBook[playerId].Push++;
                            }
                        }
                    }

                    // 门清
                    if (roundDetail.Menqing)
                    {
                        statBook[roundDetail.WinnerId].Menqing++;
                        statBook[roundDetail.WinnerId].MenqingMoney += win;
                    }

                    // 杠开
                    if (roundDetail.Gangkai)
                    {
                        statBook[roundDetail.WinnerId].Gangkai++;
                        statBook[roundDetail.WinnerId].GangkaiMoney += win;
                    }

                    // 海底捞月
                    if (roundDetail.Laoyue)
                    {
                        statBook[roundDetail.WinnerId].Laoyue++;
                        statBook[roundDetail.WinnerId].LaoyueMoney += win;
                    }

                    // 抢杠
                    if (roundDetail.Qianggang)
                    {
                        if (roundDetail.Zimo && roundDetail.Gangkai)
                        {
                            statBook[roundDetail.WinnerId].BaogangWin++;
                            statBook[roundDetail.WinnerId].BaogangWinMoney += win;
                            statBook[roundDetail.DianpaoId].BaogangLose++;
                            statBook[roundDetail.DianpaoId].BaogangLoseMoney += dianpaoLoss;
                        }
                        else
                        {
                            statBook[roundDetail.WinnerId].QianggangWin++;
                            statBook[roundDetail.WinnerId].QianggangWinMoney += win; 
                            statBook[roundDetail.DianpaoId].QianggangLose++;
                            statBook[roundDetail.DianpaoId].QianggangLoseMoney += dianpaoLoss;
                        }                        
                    }

                    // 承包赢
                    if (roundDetail.ChengbaoId != -1)
                    {
                        decimal loss = -(decimal)ReflectionUtility.GetValue(roundDetail, FindDelta(dr, roundDetail.ChengbaoId), true);
                        // 自摸承包
                        if (roundDetail.Zimo)
                        {
                            statBook[roundDetail.WinnerId].ChengbaoWinMoney += win; // temp winning 

                            if (i == roundDetails.Count - 1)
                            {
                                // only calculates at the last detail
                                int count = 5 * roundDetails.Count;
                                foreach(RoundDetail rd in roundDetails)
                                {
                                    if (rd.Qianggang)
                                    {
                                        count += 3;
                                        break;
                                    }
                                }
                                
                                string winCountProperty = $"ChengbaoWin{count}";
                                string WinMoneyProperty = $"ChengbaoWin{count}Money";
                                ReflectionUtility.Increment(statBook[roundDetail.WinnerId], winCountProperty);
                                ReflectionUtility.Increment(statBook[roundDetail.WinnerId], WinMoneyProperty, statBook[roundDetail.WinnerId].ChengbaoWinMoney);

                                statBook[roundDetail.WinnerId].ChengbaoWinMoney = 0; // reset
                            }
                            
                            if (roundDetail.Qianggang)
                            {
                                statBook[roundDetail.ChengbaoId].ChengbaoLose8++;
                                statBook[roundDetail.ChengbaoId].ChengbaoLose8Money += loss;
                            }
                            else
                            {
                                statBook[roundDetail.ChengbaoId].ChengbaoLose5++;
                                statBook[roundDetail.ChengbaoId].ChengbaoLose5Money += loss;
                            }
                        }
                        // 一家/两家
                        else
                        {
                            if (roundDetail.ChengbaoId == roundDetail.DianpaoId)
                            {
                                // 两家
                                statBook[roundDetail.WinnerId].ChengbaoWin2++;
                                statBook[roundDetail.WinnerId].ChengbaoWin2Money += win;
                                statBook[roundDetail.ChengbaoId].ChengbaoLose2++;
                                statBook[roundDetail.ChengbaoId].ChengbaoLose2Money += loss;
                            }
                            else
                            {
                                // 一家
                                statBook[roundDetail.WinnerId].ChengbaoWin1++;
                                statBook[roundDetail.WinnerId].ChengbaoWin1Money += win;
                                statBook[roundDetail.ChengbaoId].ChengbaoLose1++;
                                statBook[roundDetail.ChengbaoId].ChengbaoLose1Money += loss;
                            }
                            
                        }
                    }

                    // 大吊车
                    if (roundDetail.Lezi)
                    {
                        statBook[roundDetail.WinnerId].Dadiaoche++;
                        statBook[roundDetail.WinnerId].DadiaocheMoney += win;
                    }

                    // 牌型
                    statBook[roundDetail.WinnerId].Hands[roundDetail.HandId].Count++;
                    statBook[roundDetail.WinnerId].Hands[roundDetail.HandId].Money += win;
                }

            }

            List<Player> realPlayers = new List<Player>();
            foreach (Player player in players)
            {
                if ((id != 0 && statBook[player.Id].RoundsPlayed > 0) || (id == 0 && statBook[player.Id].RoundsPlayed >= roundCountThreshold && (lastRoundTime - statBook[player.Id].LastPlayed).TotalDays < lastPlayDateThreshold))
                {
                    realPlayers.Add(player);
                }
            }

            realPlayers.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));

            // make Dependencywheel data
            List<DependencywheelSeriesData> cashFlowData = new List<DependencywheelSeriesData>();
            foreach (Player player in realPlayers)
            {
                Stat stat = statBook[player.Id];
                    
                foreach (Player opponent in realPlayers)
                {
                    if (opponent.Id != player.Id && stat.HeadToHead.ContainsKey(opponent.Id) && stat.HeadToHead[opponent.Id] > 0)
                    {
                        cashFlowData.Add(new DependencywheelSeriesData
                        {
                            From = opponent.Name,
                            To = player.Name,
                            Weight = (double)stat.HeadToHead[opponent.Id]
                        });
                    }
                }
            }

            ViewData["players"] = players;
            ViewData["realPlayers"] = realPlayers;
            ViewData["hands"] = hands;
            ViewData["statBook"] = statBook;
            ViewData["cashFlowData"] = cashFlowData;
            ViewBag.gameId = id;

            // Dice
            List<Dice> dices = new List<Dice>();
            List<DiceRoll> diceRolls = new List<DiceRoll>();
            Dictionary<int, int> playerTotalRolls = new Dictionary<int, int> { { 0, 0 } };
            List<Player> diceRollers = new List<Player>();

            foreach (Player player in realPlayers)
            {
                playerTotalRolls.Add(player.Id, 0);
                diceRollers.Add(player);
            }
            diceRollers.Add(new Player { Id = 0, Name = "总计" });

            for (int i = 1; i <= 6; i++)
            {
                for (int j = i; j <= 6; j++)
                {
                    Dice dice = new Dice(i, j);
                    dices.Add(dice);
                    DiceRoll diceRoll = new DiceRoll(dice);
                    int total = 0;

                    DataTable dtDice = dbUtility.Read(sqlServer, SQL.getDiceRolled, new DbParameter("@dice1", i), new DbParameter("@dice2", j), new DbParameter("game_id", id));
                    foreach(DataRow dr in dtDice.Rows)
                    {
                        int playerId = Convert.ToInt32(dr["player_id"]);
                        int count = Convert.ToInt32(dr["count"]);
                        if (playerTotalRolls.ContainsKey(playerId))
                        {
                            playerTotalRolls[playerId] += count;
                            diceRoll.AddRollCount(playerId, count);
                            total += count;
                        }
                    }
                    diceRoll.AddRollCount(0, total);
                    playerTotalRolls[0] += total;
                    diceRolls.Add(diceRoll);
                }
            }

            // clean up
            foreach (Player player in diceRollers)
            {
                if (playerTotalRolls[player.Id] < 100)
                    playerTotalRolls.Remove(player.Id);
            }

            ViewData["diceRollers"] = diceRollers;
            ViewData["diceRolls"] = diceRolls;
            ViewData["playerTotalRolls"] = playerTotalRolls;

            return View();
        }

        public ActionResult Records()
        {
            Dictionary<int, string> playerBook = GetPlayerBook();

            // single round
            int winningThreshold = 100;
            int winningSingleRoundThreshold = 50;
            DataRowCollection winningRows = dbUtility.Read(sqlServer, SQL.getHighestWinnings, new DbParameter("@winning", winningSingleRoundThreshold)).Rows;
            List<Record> records = new();

            foreach (DataRow dr in winningRows)
            {
                RoundResult result = GetRoundResult(dr, playerBook);

                if (result.Round.Winning >= winningThreshold) {
                    string description = result.Description == "" ? "" : result.Description + "<br />";
                    foreach(RoundDetail roundDetail in result.RoundDetails)
                    {
                        description += roundDetail.Description + "<br />";
                    }
                    records.Add(new Record
                    {
                        Player = playerBook[result.RoundDetails[0].WinnerId],
                        Date = result.Round.Created.ToString("d"),
                        Winning = result.Round.Winning,
                        Description = description
                    });
                }
                
            }
            ViewBag.records = records.OrderByDescending(y => y.Winning).ThenByDescending(y => y.Date).ToList();

            // single game
            List<Record> recordsWinning = new List<Record>();
            List<Record> recordsLoss = new List<Record>();
            foreach (GameHistory game in GetGameHistory())
            {
                foreach(int playerID in game.Players)
                {
                    Record record = new Record
                    {
                        Player = playerBook[playerID],
                        Date = game.Start.ToString("d"),
                        Winning = game.BalanceSheet[playerID]
                    };
                    if (record.Winning > 0) recordsWinning.Add(record);
                    else recordsLoss.Add(record);
                }
            }
            // sort
            ViewBag.recordsWinning = recordsWinning.Where(y => y.Winning >= winningThreshold).OrderByDescending(y => y.Winning).ThenByDescending(y => y.Date).ToList();
            ViewBag.recordsLoss = recordsLoss.Where(y => y.Winning <= -winningThreshold).OrderBy(y => y.Winning).ThenByDescending(y => y.Date).ToList();

            return View();
        }

        public ActionResult RoundRecords()
        {
            Dictionary<int, string> playerBook = GetPlayerBook();
            DataTable dt = dbUtility.Read(sqlServer, SQL.getRoundsWithGame, new DbParameter("@game_id", DbUtility.Zero));

            // no PriorityQueue in .net framework :(
            // have to sort at the end
            Dictionary<int, Dictionary<int, RoundRecord>> winningStreak = new();
            Dictionary<int, Dictionary<int, RoundRecord>> zimoStreak = new();
            Dictionary<int, Dictionary<int, RoundRecord>> losingStreak = new();
            Dictionary<int, Dictionary<int, RoundRecord>> dianpaoStreak = new();

            #region Get game and round info
            // get game info
            DataTable dt2 = dbUtility.Read(sqlServer, SQL.getGames);
            Dictionary<int, string> gameDates = new();
            foreach (DataRow dr in dt2.Rows)
            {
                int id = Convert.ToInt32(dr["id"]);
                if (Skip(id, Convert.ToDecimal(dr["hua"])))
                {
                    continue;
                }
                int prev_id = dr["prev_game_id"] == DBNull.Value ? -1 : Convert.ToInt32(dr["prev_game_id"]);
                if (gameDates.ContainsKey(prev_id))
                {
                    gameDates.Add(id, gameDates[prev_id]);
                }
                else
                {
                    gameDates.Add(id, Convert.ToDateTime(dr["created"]).ToShortDateString());
                }
            }

            // get all round details
            Dictionary<int, List<RoundDetail>> allRoundDetails = new();
            foreach (DataRow r in dbUtility.Read(sqlServer, SQL.getRoundDetail, new DbParameter("@roundId", DbUtility.Zero)).Rows)
            {
                RoundDetail roundDetail = GetRoundDetailFromDataRow(r);
                if (!allRoundDetails.ContainsKey(roundDetail.RoundId))
                {
                    allRoundDetails.Add(roundDetail.RoundId, new List<RoundDetail>());
                }
                allRoundDetails[roundDetail.RoundId].Add(roundDetail);
            }
            #endregion

            Dictionary<int, StreakInfo> winners = new();
            Dictionary<int, StreakInfo> zimos = new();
            Dictionary<int, StreakInfo> losers = new();
            Dictionary<int, StreakInfo> dianpaos = new();
            int gameId = 0;
            int count = 0;
            foreach(DataRow dr in dt.Rows)
            {
                count++;
                if (Skip(Convert.ToInt32(dr["game_id"]), Convert.ToDecimal(dr["hua"])))
                {
                    continue;
                }
                Round round = GetRoundFromDataRow(dr);
                DateTime created = Convert.ToDateTime(dr["created1"]);
                if (round.GameId != gameId)
                {
                    if (dr["prev_game_id"] == DBNull.Value)
                    {
                        // no substitution happened, this is another game
                        EndStreaks(winningStreak, winners, playerBook, round.GameId);
                        EndStreaks(zimoStreak, zimos, playerBook, round.GameId);
                        EndStreaks(losingStreak, losers, playerBook, round.GameId);
                        EndStreaks(dianpaoStreak, dianpaos, playerBook, round.GameId);
                    }
                    else if (Convert.ToInt32(dr["prev_game_id"]) != gameId)
                    {
                        // there is substitution
                        // not the same game, shouldn't really happen
                        EndStreaks(winningStreak, winners, playerBook, round.GameId);
                        EndStreaks(zimoStreak, zimos, playerBook, round.GameId);
                        EndStreaks(losingStreak, losers, playerBook, round.GameId);
                        EndStreaks(dianpaoStreak, dianpaos, playerBook, round.GameId);
                    }
                    gameId = round.GameId;
                }

                if (allRoundDetails[round.Id][0].WinnerId == -1)
                    continue; //荒庄

                // players in this round
                int[] playerIds = new int[4] { Convert.ToInt32(dr["player1_id"]), Convert.ToInt32(dr["player2_id"]), Convert.ToInt32(dr["player3_id"]), Convert.ToInt32(dr["player4_id"]) };

                #region winner logic
                // find winner
                HashSet<int> roundWinners = new();
                foreach (RoundDetail rd in allRoundDetails[round.Id])
                {
                    roundWinners.Add(rd.WinnerId);
                }

                int[] winningPlayers = winners.Keys.ToArray();
                foreach (int playerId in winningPlayers)
                {
                    if (!roundWinners.Contains(playerId))
                    {
                        // streak ends
                        EndStreaks(winningStreak, winners, playerBook, gameId, playerId);
                    }
                }
                foreach(int playerId in roundWinners)
                {
                    decimal cashflow = Convert.ToDecimal(dr[FindDelta(dr, playerId)]);
                    if (winners.ContainsKey(playerId))
                    {
                        // streak continues
                        winners[playerId].Count++;
                        winners[playerId].Cashflow += cashflow;
                    }
                    else
                    {
                        // streak starts
                        winners.Add(playerId, new StreakInfo() { Count = 1, Cashflow = cashflow });
                    }
                }
                #endregion

                #region zimo logic
                // find zimo
                if (allRoundDetails[round.Id][0].Zimo)
                {
                    int roundZimoPlayerId = allRoundDetails[round.Id][0].WinnerId;
                    decimal cashflow = Convert.ToDecimal(dr[FindDelta(dr, roundZimoPlayerId)]);
                    if (zimos.ContainsKey(roundZimoPlayerId))
                    {
                        // streak continues
                        zimos[roundZimoPlayerId].Count++;
                        zimos[roundZimoPlayerId].Cashflow += cashflow;
                    }
                    else
                    {
                        // previous streak ends
                        EndStreaks(zimoStreak, zimos, playerBook, gameId);

                        // new streak starts
                        zimos.Add(roundZimoPlayerId, new StreakInfo() { Count = 1, Cashflow = cashflow });
                    }
                }
                else
                {
                    // all streak ends
                    EndStreaks(zimoStreak, zimos, playerBook, gameId);
                }
                #endregion

                #region loser logic
                // find loser
                Dictionary<int, decimal> roundLosers = new();
                foreach (int playerId in playerIds)
                {
                    decimal cashflow = -Convert.ToDecimal(dr[FindDelta(dr, playerId)]);
                    if (cashflow < 0m)
                    {
                        // this player loses
                        roundLosers.Add(playerId, cashflow);
                    }
                }
                
                int[] losingPlayers = losers.Keys.ToArray();
                foreach(int playerId in losingPlayers)
                {
                    if (!roundLosers.ContainsKey(playerId))
                    {
                        // streak ends
                        EndStreaks(losingStreak, losers, playerBook, gameId, playerId);
                    }
                }
                foreach(int playerId in roundLosers.Keys)
                {
                    if (losers.ContainsKey(playerId))
                    {
                        // steak continues
                        losers[playerId].Count++;
                        losers[playerId].Cashflow += roundLosers[playerId];
                    }
                    else
                    {
                        // streak starts
                        losers.Add(playerId, new StreakInfo() { Count = 1, Cashflow = roundLosers[playerId] });
                    }
                }
                #endregion

                #region dianpao logic
                // find dianpao
                if (allRoundDetails[round.Id][0].DianpaoId != -1)
                {
                    int roundDianpaoPlayerId = allRoundDetails[round.Id][0].DianpaoId;
                    decimal cashflow = -Convert.ToDecimal(dr[FindDelta(dr, roundDianpaoPlayerId)]);
                    if (dianpaos.ContainsKey(roundDianpaoPlayerId))
                    {
                        // streak continues
                        dianpaos[roundDianpaoPlayerId].Count++;
                        dianpaos[roundDianpaoPlayerId].Cashflow += cashflow;
                    }
                    else
                    {
                        // previous streak ends
                        EndStreaks(dianpaoStreak, dianpaos, playerBook, gameId);

                        // new streak starts
                        dianpaos.Add(roundDianpaoPlayerId, new StreakInfo() { Count = 1, Cashflow = cashflow });
                    }
                }
                else
                {
                    // all streak ends
                    EndStreaks(dianpaoStreak, dianpaos, playerBook, gameId);
                }
                #endregion

                // streak ends at the last round
                if (count == dt.Rows.Count)
                {
                    EndStreaks(winningStreak, winners, playerBook, round.GameId);
                    EndStreaks(zimoStreak, zimos, playerBook, round.GameId);
                    EndStreaks(losingStreak, losers, playerBook, round.GameId);
                    EndStreaks(dianpaoStreak, dianpaos, playerBook, round.GameId);
                }
            }

            ViewBag.winningStreakRecords = GetStreakRecords(winningStreak, gameDates, 10);
            ViewBag.zimoStreakRecords = GetStreakRecords(zimoStreak, gameDates, 10);
            ViewBag.losingStreakRecords = GetStreakRecords(losingStreak, gameDates, 10);
            ViewBag.dianpaoStreakRecords = GetStreakRecords(dianpaoStreak, gameDates, 10);

            return View();
        }

        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult Huangfan(int gameId, int count)
        {
            DataRow game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];
            if (!(bool)game["finished"])
            {
                for (int i = 0; i < count; i++)
                {
                    dbUtility.Insert(sqlServer, SQL.updateGameIncrementHuangfan, new DbParameter("gameId", gameId));
                }
                for (int i = 0; i > count; i--)
                {
                    dbUtility.Insert(sqlServer, SQL.updateGameDecrementHuangfan, new DbParameter("gameId", gameId));
                }
            }
            return RedirectToAction("Game", new { gameId = gameId });
        }

        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult Finish(int gameId)
        {
            FinishGame(gameId);
            return RedirectToAction("Game", new { gameId = gameId });
        }

        private Dictionary<int, string> GetPlayerBook()
        {
            Dictionary<int, string> playerBook = new Dictionary<int, string>(); // <id, name>
            DataTable dt = dbUtility.Read(sqlServer, SQL.getPlayers);
            foreach (DataRow dr in dt.Rows)
            {
                playerBook.Add(Convert.ToInt32(dr["id"]), dr["name"].ToString());
            }
            return playerBook;
        }

        private bool Skip(int gameId, decimal hua)
        {
            return hua < huaThreshold || gameId <= gameIdThreshold;
        }

        private void InsertDice(string dice, int roundId, int gameId, int playerId)
        {
            if (ParseDice(dice, out int dice1, out int dice2))
            {
                dbUtility.Insert(
                    sqlServer,
                    SQL.insertDice,
                    new DbParameter("@dice1", dice1),
                    new DbParameter("@dice2", dice2),
                    new DbParameter("@game_id", gameId),
                    new DbParameter("@round_id", roundId),
                    new DbParameter("@player_id", playerId));
            }
        }

        private bool ParseDice(string dice, out int dice1, out int dice2)
        {
            if (Int32.TryParse(dice, out int diceNum))
            {
                dice1 = diceNum / 10;
                dice2 = diceNum % 10;
                if (DiceIsValid(dice1) && DiceIsValid(dice2))
                {
                    return true;
                }
                else return false;
            }
            // cannot parse
            dice1 = 0;
            dice2 = 0;
            return false;
        }

        private bool DiceIsValid(int dice)
        {
            return dice >= 1 && dice <= 6;
        }

        private int FindLastDicer(int gameId, int roundId)
        {
            int playerId = -1;
            DataRow game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", gameId)).Rows[0];

            while (true)
            {
                DataRowCollection rows = dbUtility.Read(sqlServer, SQL.getRounds, new DbParameter("@gameId", Convert.ToInt32(game["id"]))).Rows;
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    Round round = GetRoundFromDataRow(rows[i]);
                    if (round.Id >= roundId)
                        continue;

                    DataRowCollection roundDetailRows = dbUtility.Read(sqlServer, SQL.getRoundDetail, new DbParameter("@roundId", round.Id)).Rows;
                    List<RoundDetail> roundDetailList = new();
                    foreach (DataRow row in roundDetailRows)
                    {
                        RoundDetail roundDetail = GetRoundDetailFromDataRow(row);
                        roundDetailList.Add(roundDetail);
                    }
                    if (roundDetailList.Count == 1 && roundDetailList[0].WinnerId == -1)
                    {
                        // 荒庄
                        continue;
                    }

                    playerId = roundDetailList[0].WinnerId;
                    if (roundDetailList.Count > 1)
                    {
                        if (IsYiPaoLiangXiang(roundDetailList[0], roundDetailList[1]))
                        {
                            // 一炮两响
                            playerId = roundDetailList[0].DianpaoId;
                        }
                    }
                    return playerId;
                }

                if (game["prev_game_id"] == DBNull.Value)
                {
                    if (playerId == -1)
                    {
                        // first round of the game, player1 roll the dice
                        playerId = Convert.ToInt32(game["player1_id"]);
                    }
                    break;
                }
                else
                {
                    game = dbUtility.Read(sqlServer, SQL.getGame, new DbParameter("@gameId", Convert.ToInt32(game["prev_game_id"]))).Rows[0];
                }
            }


            return playerId;
        }

        private Dictionary<int, Decimal> GetWinnings(int gameId, Result result, int player1Id, int player2Id, int player3Id, int player4Id, Decimal huaValue, Decimal leziValue)
        {
            int winnerId = result.WinnerId.GetValueOrDefault();
            decimal amount = 0;
            Dictionary<int, Decimal> winnings = new Dictionary<int, Decimal>
            {
                { player1Id, 0 },
                { player2Id, 0 },
                { player3Id, 0 },
                { player4Id, 0 }
            };
            int multiplier = Convert.ToInt32(dbUtility.Read(sqlServer, SQL.getHand, new DbParameter(@"handId", result.HandId)).Rows[0]["multiplier"]);
            if (multiplier == 0)
            {
                if (result.Lezi)
                {
                    amount = leziValue;
                }
                else
                {
                    amount = (result.HuaCount.GetValueOrDefault() + 2) * huaValue;
                }
            }
            else
            {
                amount = leziValue * multiplier;
            }

            if (result.Menqing)
                amount *= 2;
            if (result.Huangfan)
                amount *= 2;
            if (result.Gangkai)
                amount *= 2;
            if (result.Laoyue)
                amount *= 2;
            if (result.Qianggang)
                amount *= 3;

            if (result.Zimo)
            {
                // 自摸
                if (result.ChengbaoId == null)
                {
                    if (result.Qianggang && result.Gangkai)
                    {
                        // 送杠
                        // 抢杠 is already calcuated
                        winnings[winnerId] = amount;
                        winnings[result.DianpaoId.GetValueOrDefault()] = -amount;
                    }
                    else
                    {
                        // 正常自摸
                        winnings[player1Id] = -amount;
                        winnings[player2Id] = -amount;
                        winnings[player3Id] = -amount;
                        winnings[player4Id] = -amount;
                        winnings[winnerId] = amount * 3; // overwrite the previous -amount for winner
                    }
                }
                else
                {
                    // 承包
                    if (result.Qianggang)
                    {
                        // 送杠算自摸 赢8家
                        amount /= 3;
                        winnings[winnerId] = amount * 8;
                        winnings[result.ChengbaoId.GetValueOrDefault()] = -amount * 5;
                        winnings[result.DianpaoId.GetValueOrDefault()] -= amount * 3;
                    }
                    else
                    {
                        winnings[winnerId] = amount * 5;
                        winnings[result.ChengbaoId.GetValueOrDefault()] = -amount * 5;
                    }
                }
            }
            else
            {
                int dianpaoId = result.DianpaoId.GetValueOrDefault();
                if (result.ChengbaoId == null)
                {
                    // 点炮
                    // 抢杠 is already calcuated
                    winnings[winnerId] = amount;
                    winnings[dianpaoId] = -amount;
                }
                else
                {
                    // 点炮且承包
                    winnings[winnerId] = amount * 2;
                    winnings[dianpaoId] -= amount;
                    winnings[result.ChengbaoId.GetValueOrDefault()] -= amount;
                }
            }

            return winnings;
        }

        private bool UpdatePlayerInList(List<Player> players, int playerId, int order)
        {
            foreach (Player player in players)
            {
                if (player.Id == playerId)
                {
                    player.Order = order;
                    return true;
                }
            }
            return false;
        }

        private string GetRoundDescription(RoundDetail round, Dictionary<int, string> playerBook)
        {
            StringBuilder sb = new StringBuilder();
            if (round.WinnerId != -1)
            {
                int multiplier = -1;
                string handName = "";
                // somebody wins
                string huaCount = "";
                if (round.HandId != -1)
                {
                    DataRow hand = dbUtility.Read(sqlServer, SQL.getHand, new DbParameter("@handId", round.HandId)).Rows[0];
                    multiplier = Convert.ToInt32(hand["multiplier"]);
                    handName = hand["name"].ToString();
                    if (multiplier == 0)
                    {
                        huaCount = $"{round.HuaCount.ToString()}花 ";
                    }
                }

                if (round.Zimo)
                {
                    sb.Append($"{playerBook[round.WinnerId]} 自摸 ");
                    if (round.Lezi)
                    {
                        sb.Append($"{handName}, 大吊车");
                    }
                    else
                    {
                        sb.Append($"{huaCount} {handName}");
                    }

                    if (round.Gangkai)
                    {
                        sb.Append($", 杠开");
                    }

                    if (round.Qianggang)
                    {
                        sb.Append($", {playerBook[round.DianpaoId]} 送杠");
                    }
                }
                else if (round.Qianggang)
                {
                    sb.Append($"{playerBook[round.WinnerId]} ");
                    if (round.Lezi)
                    {
                        sb.Append($"{handName}, 大吊车, 抢杠 {playerBook[round.DianpaoId]}");
                    }
                    else
                    {
                        sb.Append($"{huaCount}{handName}, 抢杠 {playerBook[round.DianpaoId]}");
                    }

                }
                else
                {
                    sb.Append($"{playerBook[round.WinnerId]} ");
                    if (round.Lezi)
                    {
                        sb.Append($"{handName}, 大吊车, {playerBook[round.DianpaoId]} 点炮");
                    }
                    else
                    {
                        sb.Append($"{huaCount}{handName}, {playerBook[round.DianpaoId]} 点炮");
                    }

                    if (round.Gangkai)
                    {
                        sb.Append($", 杠开");
                    }
                }

                if (round.Huangfan)
                {
                    sb.Append(", 荒番");
                }
                if (round.Menqing)
                {
                    sb.Append(", 门清");
                }
                if (round.Laoyue)
                {
                    sb.Append(", 海底捞月");
                }
                if (round.ChengbaoId != -1)
                {
                    sb.Append($", {playerBook[round.ChengbaoId]} 承包");
                }
            }
            else
            {
                sb.Append("荒庄");
            }

            return sb.ToString();
        }

        private Round GetRoundFromDataRow(DataRow dr)
        {
            return new Round
            {
                Id = Convert.ToInt32(dr["id"]),
                GameId = Convert.ToInt32(dr["game_id"]),
                Delta1 = Convert.ToDecimal(dr["delta1"]),
                Delta2 = Convert.ToDecimal(dr["delta2"]),
                Delta3 = Convert.ToDecimal(dr["delta3"]),
                Delta4 = Convert.ToDecimal(dr["delta4"]),
                Created = Convert.ToDateTime(dr["created"]),
                Winning = dr.Table.Columns.Contains("winning") ? Convert.ToDecimal(dr["winning"]) : 0m
            };
        }

        private RoundDetail GetRoundDetailFromDataRow(DataRow dr)
        {
            return new RoundDetail
            {
                Id = Convert.ToInt32(dr["id"]),
                RoundId = Convert.ToInt32(dr["round_id"]),
                Delta1 = Convert.ToDecimal(dr["delta1"]),
                Delta2 = Convert.ToDecimal(dr["delta2"]),
                Delta3 = Convert.ToDecimal(dr["delta3"]),
                Delta4 = Convert.ToDecimal(dr["delta4"]),
                WinnerId = dr["winner_id"] == DBNull.Value ? -1 : Convert.ToInt32(dr["winner_id"]),
                HuaCount = dr["hua_count"] == DBNull.Value ? -1 : Convert.ToInt32(dr["hua_count"]),
                HandId = dr["hand_id"] == DBNull.Value ? -1 : Convert.ToInt32(dr["hand_id"]),
                Lezi = (bool)dr["lezi"],
                Menqing = (bool)dr["menqing"],
                Zimo = (bool)dr["zimo"],
                DianpaoId = dr["dianpao_id"] == DBNull.Value ? -1 : Convert.ToInt32(dr["dianpao_id"]),
                Qianggang = (bool)dr["qianggang"],
                Huangfan = (bool)dr["huangfan"],
                Gangkai = (bool)dr["gangkai"],
                Laoyue = (bool)dr["laoyue"],
                ChengbaoId = dr["chengbao_id"] == DBNull.Value ? -1 : Convert.ToInt32(dr["chengbao_id"]),
                Created = Convert.ToDateTime(dr["created"]),
                Winning = dr.Table.Columns.Contains("winning") ? Convert.ToDecimal(dr["winning"]) : 0m
            };
        }

        private RoundResult GetRoundResult(DataRow dr, Dictionary<int, string> playerBook)
        {
            RoundResult roundResult = GetRoundInfo(dr, playerBook);
            DataRowCollection roundDetailRows = dbUtility.Read(sqlServer, SQL.getRoundDetail, new DbParameter("@roundId", roundResult.Round.Id)).Rows;
            foreach(DataRow row in roundDetailRows)
            {
                RoundDetail roundDetail = GetRoundDetailFromDataRow(row);
                roundDetail.Description = GetRoundDescription(roundDetail, playerBook);
                roundResult.RoundDetails.Add(roundDetail);
            }

            if (roundResult.RoundDetails.Count > 1)
            {
                if (IsYiPaoLiangXiang(roundResult.RoundDetails[0], roundResult.RoundDetails[1]))
                {
                    if (roundResult.RoundDetails.Count == 2)
                    {
                        roundResult.Description = "一炮两响";
                    }
                    else if (roundResult.RoundDetails.Count == 3)
                    {
                        roundResult.Description = "一炮三响";
                    }
                }
                else if (IsMultipleChengBao(roundResult.RoundDetails[0], roundResult.RoundDetails[1]))
                {
                    if (roundResult.RoundDetails.Count == 2)
                    {
                        roundResult.Description = "包十家";
                        foreach(RoundDetail roundDetail in roundResult.RoundDetails)
                        {
                            if (roundDetail.Gangkai && roundDetail.Zimo && roundDetail.Qianggang)
                            {
                                roundResult.Description = "包十三家";
                                break;
                            }
                        }
                    }
                    else if (roundResult.RoundDetails.Count == 3)
                    {
                        roundResult.Description = "包十五家";
                        foreach (RoundDetail roundDetail in roundResult.RoundDetails)
                        {
                            if (roundDetail.Gangkai && roundDetail.Zimo && roundDetail.Qianggang)
                            {
                                roundResult.Description = "包十八家";
                                break;
                            }
                        }
                    }
                }
            }

            return roundResult;
        }

        private void FinishGame(int gameId)
        {
            dbUtility.Insert(sqlServer, SQL.updateGameFinishGame, new DbParameter("@gameId", gameId));
        }

        private List<GameHistory> GetGameHistory(bool combineGameWithSubs = true)
        {
            List<GameHistory> gameHistories = new List<GameHistory>();
            DataTable dt = dbUtility.Read(sqlServer, SQL.getGameHistoryInfo);
            List<int> gameIds = new List<int>();
            Dictionary<int, DataRow> historyTable = new Dictionary<int, DataRow>();

            foreach (DataRow dr in dt.Rows)
            {
                int gameId = Convert.ToInt32(dr["game_id"]);
                gameIds.Add(gameId);
                historyTable.Add(gameId, dr);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // for each game
                DataRow dr = dt.Rows[i];

                int gameId = Convert.ToInt32(dr["game_id"]);
                if (!historyTable.ContainsKey(gameId))
                {
                    // already visited as part of game
                    continue;
                }
                historyTable.Remove(gameId);

                DateTime endTime = Convert.ToDateTime(dr["endtime"]);
                int numOfRounds = Convert.ToInt32(dr["numOfRounds"]);
                decimal huaValue = Convert.ToDecimal(dr["hua"]);
                decimal leziValue = Convert.ToDecimal(dr["lezi"]);
                List<int> players = new List<int>();
                Dictionary<int, decimal> balanceSheet = new Dictionary<int, decimal>();

                int player1Id = Convert.ToInt32(dr["player1_id"]);
                int player2Id = Convert.ToInt32(dr["player2_id"]);
                int player3Id = Convert.ToInt32(dr["player3_id"]);
                int player4Id = Convert.ToInt32(dr["player4_id"]);
                decimal delta1 = Convert.ToDecimal(dr["delta1"]);
                decimal delta2 = Convert.ToDecimal(dr["delta2"]);
                decimal delta3 = Convert.ToDecimal(dr["delta3"]);
                decimal delta4 = Convert.ToDecimal(dr["delta4"]);

                UpdateHistoryAttr(balanceSheet, players, player1Id, delta1);
                UpdateHistoryAttr(balanceSheet, players, player2Id, delta2);
                UpdateHistoryAttr(balanceSheet, players, player3Id, delta3);
                UpdateHistoryAttr(balanceSheet, players, player4Id, delta4);

                bool goForPrevious;
                int prevGameId = 0;

                if (dr["prev_game_id"] == DBNull.Value)
                {
                    goForPrevious = false;
                }
                else
                {
                    goForPrevious = true;
                    prevGameId = Convert.ToInt32(dr["prev_game_id"]);
                }

                while (combineGameWithSubs && goForPrevious)
                {
                    dr = historyTable[prevGameId];
                    historyTable.Remove(prevGameId);

                    numOfRounds += Convert.ToInt32(dr["numOfRounds"]);

                    player1Id = Convert.ToInt32(dr["player1_id"]);
                    player2Id = Convert.ToInt32(dr["player2_id"]);
                    player3Id = Convert.ToInt32(dr["player3_id"]);
                    player4Id = Convert.ToInt32(dr["player4_id"]);
                    delta1 = Convert.ToDecimal(dr["delta1"]);
                    delta2 = Convert.ToDecimal(dr["delta2"]);
                    delta3 = Convert.ToDecimal(dr["delta3"]);
                    delta4 = Convert.ToDecimal(dr["delta4"]);

                    UpdateHistoryAttr(balanceSheet, players, player1Id, delta1);
                    UpdateHistoryAttr(balanceSheet, players, player2Id, delta2);
                    UpdateHistoryAttr(balanceSheet, players, player3Id, delta3);
                    UpdateHistoryAttr(balanceSheet, players, player4Id, delta4);

                    if (dr["prev_game_id"] == DBNull.Value)
                    {
                        goForPrevious = false;
                    }
                    else
                    {
                        goForPrevious = true;
                        prevGameId = Convert.ToInt32(dr["prev_game_id"]);
                    }
                }

                DateTime startTime = Convert.ToDateTime(dr["starttime"]);

                gameHistories.Add(new GameHistory
                {
                    GameId = gameId,
                    Start = startTime,
                    End = endTime,
                    HuaValue = huaValue,
                    LeziValue = leziValue,
                    NumOfRounds = numOfRounds,
                    Players = players,
                    BalanceSheet = balanceSheet
                });
            }

            return gameHistories;
        }

        private List<Player> GetAllPlayers()
        {
            List<Player> players = new List<Player>();
            DataTable dt = dbUtility.Read(sqlServer, SQL.getPlayers);
            foreach (DataRow dr in dt.Rows)
            {
                players.Add(new Player
                {
                    Id = Convert.ToInt32(dr["id"]),
                    Name = dr["name"].ToString()
                });
            }
            return players;
        }

        private List<Player> GetAllPlayersByRoundsPlayed()
        {
            List<Player> players = GetAllPlayers();
            List<(Player player, int count)> playerList = new List<(Player, int)>();

            foreach(Player player in players)
            {
                int count = 0;
                var rounds = dbUtility.ReadFirstRow(sqlServer, SQL.getNumOfRoundsPlayed, new DbParameter("@player_id", player.Id))["roundCount"];
                if (rounds != DBNull.Value)
                    count = Convert.ToInt32(rounds);
                playerList.Add((player, count));
            }

            return playerList.OrderByDescending(t => t.count).ThenBy(t => t.player.Name).Select(t => t.player).ToList();
        }

        private List<Hand> GetHands()
        {
            List<Hand> hands = new List<Hand>();
            foreach (DataRow dr in dbUtility.Read(sqlServer, SQL.getHands).Rows)
            {
                hands.Add(new Hand { Id = Convert.ToInt32(dr["id"]), Name = dr["name"].ToString() });
            }
            return hands;
        }

        private string FindDelta(DataRow dr, int playerId)
        {
            if (Convert.ToInt32(dr["player1_id"]) == playerId)
                return "delta1";
            if (Convert.ToInt32(dr["player2_id"]) == playerId)
                return "delta2";
            if (Convert.ToInt32(dr["player3_id"]) == playerId)
                return "delta3";
            if (Convert.ToInt32(dr["player4_id"]) == playerId)
                return "delta4";
            return "";
        }

        private void UpdateHistoryAttr(Dictionary<int, Decimal> balanceSheet, List<int> players, int playerId, Decimal delta)
        {
            if (!balanceSheet.ContainsKey(playerId))
            {
                balanceSheet.Add(playerId, delta);
                players.Add(playerId);
            }
            else
            {
                balanceSheet[playerId] += delta;
            }
        }

        private void AddOrIncrement<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue val) 
        {
            Type[] types = { typeof(int), typeof(decimal) };
            if (!types.Contains(val.GetType()))
                throw new ArgumentException($"{val.GetType()} not supported");

            if (dict.ContainsKey(key))
                dict[key] = (dynamic)dict[key] + (dynamic)val;
            else
                dict.Add(key, val);
        }

        private RoundResult GetRoundInfo(DataRow dr, Dictionary<int, string> playerBook)
        {
            RoundResult roundInfo = new() { 
                Round = GetRoundFromDataRow(dr),
                RoundDetails = new(),
                Description = ""
            };
            if (dr.Table.Columns.Contains("dice1") && dr.Table.Columns.Contains("dice2") && dr["dice1"] != DBNull.Value && dr["dice2"] != DBNull.Value)
            {
                roundInfo.Dice = $"{playerBook[Convert.ToInt32(dr["player_id"])]} &nbsp;-&nbsp;<img src=\"/Content/images/dice-0{Convert.ToInt32(dr["dice1"])}.png\" style=\"height:1.5em;\"/> <img src=\"/Content/images/dice-0{Convert.ToInt32(dr["dice2"])}.png\" style=\"height:1.5em;\"/>";
            }
            return roundInfo;
        }

        private int GetLastGame(int gameId)
        {
            // get the last game in the chain
            DataTable dt = dbUtility.Read(sqlServer, SQL.getNextGame, new DbParameter("@gameId", gameId));
            while (dt.Rows.Count > 0)
            {
                gameId = Convert.ToInt32(dt.Rows[0]["id"]);
                dt = dbUtility.Read(sqlServer, SQL.getNextGame, new DbParameter("@gameId", gameId));
            }
            return gameId;
        }

        private List<RoundRecord> GetStreakRecords(Dictionary<int, Dictionary<int, RoundRecord>> streak, Dictionary<int, string> gameDates, int n)
        {
            // consolidate
            List<RoundRecord> streakRecords = new();
            foreach (var v in streak.Values)
            {
                foreach (var r in v.Values)
                {
                    streakRecords.Add(r);
                }
            }

            // sort
            streakRecords = streakRecords.OrderByDescending(s => s.Count).ThenBy(s => s.PlayerName).ToList();

            // increase n because of tied records
            while (n < streakRecords.Count())
            {
                if (streakRecords[n].Count != streakRecords[n - 1].Count)
                    break;
                n++;
            }

            // take the records needed
            streakRecords = streakRecords.Take(n).ToList();

            // make rank
            if (streakRecords.Count > 0)
            {
                streakRecords[0].Rank = 1;
                for (int i = 1; i < streakRecords.Count(); i++)
                {
                    if (streakRecords[i].Count == streakRecords[i - 1].Count)
                    {
                        streakRecords[i].Rank = streakRecords[i - 1].Rank;
                    }
                    else
                    {
                        streakRecords[i].Rank = i + 1;
                    }
                }
            }

            // get dates
            foreach (RoundRecord roundRecord in streakRecords)
            {
                foreach (GamePayout payout in roundRecord.Payouts)
                {
                    payout.Date = gameDates[payout.GameId];
                }
                roundRecord.Payouts.Sort((p1, p2) => Convert.ToInt32(p2.Cashflow * 100 - p1.Cashflow * 100));
            }

            return streakRecords.OrderByDescending(s => s.Count).ThenByDescending(s => s.Payouts.Count).ThenByDescending(s => s.Payouts.Max(p => p.Cashflow)).ThenBy(s => s.PlayerName).ToList();
        }

        private void EndStreaks(Dictionary<int, Dictionary<int, RoundRecord>> recordBook, Dictionary<int, StreakInfo> streaks, Dictionary<int, string> playerBook, int gameId, int endPlayerId = -1)
        {
            int[] ids = endPlayerId == -1 ? streaks.Keys.ToArray() : new int[] { endPlayerId };

            foreach (int playerId in ids)
            {
                int count = streaks[playerId].Count;
                if (count >= 3)
                {
                    // only save 3peat or better
                    GamePayout payout = new GamePayout() {
                        GameId = gameId,
                        Cashflow = streaks[playerId].Cashflow
                    };

                    if (recordBook.ContainsKey(playerId))
                    {
                        if (recordBook[playerId].ContainsKey(count))
                        {
                            recordBook[playerId][count].Payouts.Add(payout);
                        }
                        else
                        {
                            recordBook[playerId].Add(count, new RoundRecord() { Payouts = new List<GamePayout>() { payout }, PlayerName = playerBook[playerId], Count = streaks[playerId].Count });
                        }
                    }
                    else
                    {
                        Dictionary<int, RoundRecord> record = new()
                        {
                            { count, new RoundRecord() { Payouts = new List<GamePayout>() { payout }, PlayerName = playerBook[playerId], Count = streaks[playerId].Count } }
                        };
                        recordBook[playerId] = record;
                    }
                }
                streaks.Remove(playerId);
            }
        }

        private void UpdateRound(int roundId)
        {
            dbUtility.Update(
                sqlServer,
                SQL.updateRound,
                new DbParameter("@id", roundId)
                );
        }

        private bool IsYiPaoLiangXiang(RoundDetail r1, RoundDetail r2)
        {
            return Math.Abs((r1.Created - r2.Created).TotalSeconds) < 60
                && r1.DianpaoId != -1
                && r2.DianpaoId != -1
                && !r1.Zimo
                && !r2.Zimo
                && r1.DianpaoId == r2.DianpaoId
                && r1.WinnerId != -1
                && r2.WinnerId != -1
                && r1.WinnerId != r2.WinnerId;
        }

        private bool IsMultipleChengBao(RoundDetail r1, RoundDetail r2)
        {
            return Math.Abs((r1.Created - r2.Created).TotalSeconds) < 60
                && r1.Zimo
                && r2.Zimo
                && r1.WinnerId == r2.WinnerId
                && r1.ChengbaoId != -1
                && r2.ChengbaoId != -1
                && r1.ChengbaoId != r2.ChengbaoId
                && r1.HandId == r2.HandId
                && r1.HuaCount == r2.HuaCount
                && r1.Lezi == r2.Lezi
                && r1.Menqing == r2.Menqing
                && r1.Gangkai == r2.Gangkai
                && r1.Laoyue == r2.Laoyue;
        }

        private string GetRealPlayerMessage(DateTime lastGameDate)
        {
            return $"*仅显示盘数超过 {roundCountThreshold} 盘以及在 {lastGameDate.AddDays(-lastPlayDateThreshold).ToShortDateString()}(最后一盘前{lastPlayDateThreshold}天) 之后有记录的玩家 ";
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly DbUtility dbUtility = DbFactory.GetDbUtility(Db.SQLite);
        private static readonly string sqlServer = ConfigurationManager.ConnectionStrings["Mahjong"].ConnectionString;
    }
}