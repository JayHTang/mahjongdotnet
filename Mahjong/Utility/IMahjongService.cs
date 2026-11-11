using System.Collections.Generic;
using Mahjong.Models.Mahjong;

namespace Mahjong.Utility
{
    public interface IMahjongService
    {
        void AddPlayer(string name);
        List<Player> GetAllPlayers();
        List<Player> GetAllPlayersByRoundsPlayed();
        int CreateGame(Game game);
        int GetLastGame(int gameId);
        Dictionary<string, object> GetGameViewData(int gameId);
        void RecordRound(int gameId, Result result);
        void EditRoundDetail(int gameId, int roundId, int roundDetailId, Result result);
        Dictionary<string, object> GetSubViewData(int gameId);
        int SubstitutePlayers(int gameId, Game game);
        void CloseStaleGames();
        List<GameHistory> GetGameHistory(bool combineGameWithSubs = true);
        Dictionary<string, object> GetLeaderBoardViewData();
        Dictionary<string, object> GetStatsViewData(int id = 0);
        Dictionary<string, object> GetRecordsViewData();
        Dictionary<string, object> GetRoundRecordsViewData();
        void AdjustHuangfan(int gameId, int count);
        void FinishGame(int gameId);
        void InvalidateCache();
    }
}
