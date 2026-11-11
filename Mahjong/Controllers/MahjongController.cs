using System.Collections.Generic;
using Mahjong.Models.Mahjong;
using Mahjong.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mahjong.Controllers
{
    public class MahjongController(IMahjongService mahjongService) : Controller
    {
        private readonly IMahjongService _mahjongService = mahjongService;

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
                _mahjongService.AddPlayer(player.Name);
                TempData["SuccessMessage"] = $"玩家 \"{player.Name}\" 已成功创建！";
                ModelState.Clear();
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult NewGame()
        {
            ViewData["players"] = _mahjongService.GetAllPlayersByRoundsPlayed();
            return View(new Game { HuaValue = 0.5m, LeziValue = 5m });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult NewGame(Game game)
        {
            if (ModelState.IsValid)
            {
                int newId = _mahjongService.CreateGame(game);
                return RedirectToAction("Game", new { gameId = newId });
            }

            ViewData["players"] = _mahjongService.GetAllPlayersByRoundsPlayed();
            return View(game);
        }

        [HttpGet]
        public ActionResult Game(int gameId)
        {
            int lastGameId = _mahjongService.GetLastGame(gameId);
            if (lastGameId != gameId)
                return RedirectToAction("Game", new { gameId = lastGameId });

            foreach (var kvp in _mahjongService.GetGameViewData(gameId))
            {
                ViewData[kvp.Key] = kvp.Value;
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult Game(int gameId, Result result)
        {
            gameId = _mahjongService.GetLastGame(gameId);

            if (ModelState.IsValid)
            {
                _mahjongService.RecordRound(gameId, result);
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
                _mahjongService.EditRoundDetail(gameId, roundId, roundDetailId, result);
            }
            return RedirectToAction("Game", new { gameId });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult Sub(int gameId)
        {
            foreach (var kvp in _mahjongService.GetSubViewData(gameId))
            {
                ViewData[kvp.Key] = kvp.Value;
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Mahjong")]
        [ValidateAntiForgeryToken]
        public ActionResult Sub(int gameId, Game game)
        {
            if (ModelState.IsValid)
            {
                int newGameId = _mahjongService.SubstitutePlayers(gameId, game);
                return RedirectToAction("Game", new { gameId = newGameId });
            }

            return Sub(gameId);
        }

        public ActionResult History()
        {
            _mahjongService.CloseStaleGames();
            ViewData["gameHistories"] = _mahjongService.GetGameHistory();
            return View();
        }

        public ActionResult LeaderBoard()
        {
            foreach (var kvp in _mahjongService.GetLeaderBoardViewData())
            {
                ViewData[kvp.Key] = kvp.Value;
            }
            return View();
        }

        public ActionResult Stats(int id = 0)
        {
            foreach (var kvp in _mahjongService.GetStatsViewData(id))
            {
                ViewData[kvp.Key] = kvp.Value;
            }
            ViewBag.gameId = id;
            return View();
        }

        public ActionResult Records()
        {
            Dictionary<string, object> data = _mahjongService.GetRecordsViewData();
            ViewBag.records = data["records"];
            ViewBag.recordsWinning = data["recordsWinning"];
            ViewBag.recordsLoss = data["recordsLoss"];
            return View();
        }

        public ActionResult RoundRecords()
        {
            Dictionary<string, object> data = _mahjongService.GetRoundRecordsViewData();
            ViewBag.winningStreakRecords = data["winningStreakRecords"];
            ViewBag.zimoStreakRecords = data["zimoStreakRecords"];
            ViewBag.losingStreakRecords = data["losingStreakRecords"];
            ViewBag.dianpaoStreakRecords = data["dianpaoStreakRecords"];
            return View();
        }

        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult Huangfan(int gameId, int count)
        {
            _mahjongService.AdjustHuangfan(gameId, count);
            return RedirectToAction("Game", new { gameId });
        }

        [Authorize(Roles = "Administrator, Mahjong")]
        public ActionResult Finish(int gameId)
        {
            _mahjongService.FinishGame(gameId);
            return RedirectToAction("Game", new { gameId });
        }
    }
}
