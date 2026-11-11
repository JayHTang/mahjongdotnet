namespace Mahjong.SQL
{
    public static class SQL_Mahjong
    {
        // language=sql
        public const string getDiceRolled = """
            SELECT player_id,
                   Count(*) as count
            FROM   dice
            WHERE  dice1 = @dice1
                   AND dice2 = @dice2
                   AND ( @game_id = 0
                          OR game_id = @game_id )
            GROUP  BY player_id
            ORDER  BY player_id
            """;

        // language=sql
        public const string getDiceRolledByPlayer = """
            SELECT dice1,
                   dice2,
                   Count(*) as count
            FROM   dice
            WHERE  player_id = @player_id
                   AND ( @game_id = 0
                          OR game_id = @game_id )
            GROUP  BY dice1,
                      dice2
            ORDER  BY dice1,
                      dice2;
            """;

        // language=sql
        public const string getGame = """
            SELECT * FROM game WHERE id = @gameId
            """;

        // language=sql
        public const string getGameHistoryInfo = """
            SELECT g.id           AS game_id,
                   g.hua          AS hua,
                   g.lezi         AS lezi,
                   g.prev_game_id AS prev_game_id,
                   Min(g.created) AS starttime,
                   Max(r.created) AS endtime,
                   Count(r.id)    AS numOfRounds,
                   g.player1_id   AS player1_id,
                   g.player2_id   AS player2_id,
                   g.player3_id   AS player3_id,
                   g.player4_id   AS player4_id,
                   Sum(r.delta1)  AS delta1,
                   Sum(r.delta2)  AS delta2,
                   Sum(r.delta3)  AS delta3,
                   Sum(r.delta4)  AS delta4
            FROM   game AS g
                   JOIN round AS r
                     ON r.game_id = g.id
            GROUP  BY g.id,
                      g.hua,
                      g.lezi,
                      g.prev_game_id,
                      g.player1_id,
                      g.player2_id,
                      g.player3_id,
                      g.player4_id
            ORDER  BY g.id DESC
            """;

        // language=sql
        public const string getGames = """
            SELECT * FROM game
            """;

        // language=sql
        public const string getGamesUnfinished = """
            SELECT * FROM game where finished = 0
            """;

        // language=sql
        public const string getHand = """
            SELECT * FROM hand WHERE id=@handId
            """;

        // language=sql
        public const string getHands = """
            SELECT * FROM hand
            """;

        // language=sql
        public const string getHighestWinnings = """
            SELECT *
            FROM   (SELECT round.*,
                           delta1 AS winning,
                           game.created,
                           hua
                    FROM   round
                           INNER JOIN game
                                   ON round.game_id = game.id
                    WHERE  delta1 > 0
                    UNION
                    SELECT round.*,
                           delta2 AS winning,
                           game.created,
                           hua
                    FROM   round
                           INNER JOIN game
                                   ON round.game_id = game.id
                    WHERE  delta2 > 0
                    UNION
                    SELECT round.*,
                           delta3 AS winning,
                           game.created,
                           hua
                    FROM   round
                           INNER JOIN game
                                   ON round.game_id = game.id
                    WHERE  delta3 > 0
                    UNION
                    SELECT round.*,
                           delta4 AS winning,
                           game.created,
                           hua
                    FROM   round
                           INNER JOIN game
                                   ON round.game_id = game.id
                    WHERE  delta4 > 0) AS t
            WHERE  winning >= @winning
            ORDER  BY created
            """;

        // language=sql
        public const string getLastRoundDetail = """
            SELECT * FROM rounddetail WHERE round_id = (SELECT MAX(id) FROM round WHERE game_id = @gameId) ORDER BY created DESC
            """;

        // language=sql
        public const string getNextGame = """
            SELECT * FROM game WHERE prev_game_id = @gameId
            """;

        // language=sql
        public const string getNumOfRoundsPlayed = """
            SELECT Sum(t.numOfRounds) AS roundCount,
                   Max(t.lastPlayed)  AS lastPlayed
            FROM   (SELECT g.id          AS game_id,
                           g.hua         AS hua,
                           Count(r.id)   AS numOfRounds,
                           Max(r.created) AS lastPlayed,
                           g.player1_id  AS player1_id,
                           g.player2_id  AS player2_id,
                           g.player3_id  AS player3_id,
                           g.player4_id  AS player4_id
                    FROM   game AS g
                           JOIN round AS r
                             ON r.game_id = g.id
                    GROUP  BY g.id,
                              g.hua,
                              g.player1_id,
                              g.player2_id,
                              g.player3_id,
                              g.player4_id) t
            WHERE  ( t.player1_id = @player_id
                      OR t.player2_id = @player_id
                      OR t.player3_id = @player_id
                      OR t.player4_id = @player_id )
                   AND t.hua >= 0.5
                   AND t.game_id > 4
            """;

        // language=sql
        public const string getPlayers = """
            SELECT * FROM player
            """;

        // language=sql
        public const string getRound = """
            SELECT * FROM round WHERE game_id = @gameId AND id = @id
            """;

        // language=sql
        public const string getRoundDetail = """
            SELECT * FROM rounddetail WHERE @roundId = 0 OR round_id = @roundId
            """;

        // language=sql
        public const string getRounds = """
            SELECT *
            FROM   round
                   LEFT OUTER JOIN dice
                                ON dice.game_id = round.game_id
                                   AND dice.round_id = round.id
            WHERE  round.game_id = @gameId
            ORDER  BY round.id
            """;

        // language=sql
        public const string getRoundsWithGame = """
            SELECT *
            FROM   round r
                   JOIN game g
                     ON r.game_id = g.id
            WHERE  @game_id = 0 OR r.game_id = @game_id
            ORDER BY r.created
            """;

        // language=sql
        public const string insertDice = """
            INSERT INTO dice
                       (dice1,
                        dice2,
                        game_id,
                        round_id,
                        player_id,
                        created)
            VALUES     (@dice1,
                        @dice2,
                        @game_id,
                        @round_id,
                        @player_id,
                        datetime('now','localtime'))
            """;

        // language=sql
        public const string insertGame = """
            INSERT INTO game
                       (player1_id,
                        player2_id,
                        player3_id,
                        player4_id,
                        hua,
                        lezi,
                        huangfan,
                        huangfan_executed,
                        finished,
                        created,
                        prev_game_id)
            VALUES     (@player1_id,
                        @player2_id,
                        @player3_id,
                        @player4_id,
                        @hua,
                        @lezi,
                        0,
                        0,
                        0,
                        datetime('now','localtime'),
                        NULL);
            """;

        // language=sql
        public const string insertGameWithPrevGame = """
            INSERT INTO game
                       (player1_id,
                        player2_id,
                        player3_id,
                        player4_id,
                        hua,
                        lezi,
                        huangfan,
                        huangfan_executed,
                        finished,
                        created,
                        prev_game_id)
            VALUES     (@player1_id,
                        @player2_id,
                        @player3_id,
                        @player4_id,
                        @hua,
                        @lezi,
                        @huangfan,
                        0,
                        0,
                        datetime('now','localtime'),
                        @prev_game_id);
            """;

        // language=sql
        public const string insertPlayer = """
            INSERT INTO player (name) VALUES(@name)
            """;

        // language=sql
        public const string insertRound = """
            INSERT INTO round
                       (game_id,
                        delta1,
                        delta2,
                        delta3,
                        delta4,
                        created)
            VALUES     (@game_id,
                        @delta1,
                        @delta2,
                        @delta3,
                        @delta4,
                        datetime('now','localtime'));
            """;

        // language=sql
        public const string insertRoundDetail = """
            INSERT INTO rounddetail
                       (round_id,
                        delta1,
                        delta2,
                        delta3,
                        delta4,
                        winner_id,
                        hua_count,
                        hand_id,
                        lezi,
                        menqing,
                        zimo,
                        dianpao_id,
                        qianggang,
                        huangfan,
                        gangkai,
                        laoyue,
                        chengbao_id,
                        created)
            VALUES     (@round_id,
                        @delta1,
                        @delta2,
                        @delta3,
                        @delta4,
                        @winner_id,
                        @hua_count,
                        @hand_id,
                        @lezi,
                        @menqing,
                        @zimo,
                        @dianpao_id,
                        @qianggang,
                        @huangfan,
                        @gangkai,
                        @laoyue,
                        @chengbao_id,
                        datetime('now','localtime'));
            """;

        // language=sql
        public const string updateGameDecrementHuangfan = """
            UPDATE game SET huangfan = huangfan - 1 WHERE id = @gameId
            """;

        // language=sql
        public const string updateGameFinishGame = """
            UPDATE game SET finished = 1 WHERE id = @gameId
            """;

        // language=sql
        public const string updateGameIncrementHuangfan = """
            UPDATE game SET huangfan = huangfan + 1 WHERE id = @gameId
            """;

        // language=sql
        public const string updateGameIncrementHuangfanExecuted = """
            UPDATE game SET huangfan_executed = huangfan_executed + 1 WHERE id = @gameId
            """;

        // language=sql
        public const string updateRound = """
            UPDATE round
            SET    delta1 = rounddetail.delta1,
                   delta2 = rounddetail.delta2,
                   delta3 = rounddetail.delta3,
                   delta4 = rounddetail.delta4
            FROM (
                   SELECT round_id,
                          SUM(delta1) as delta1,
                          SUM(delta2) as delta2,
                          SUM(delta3) as delta3,
                          SUM(delta4) as delta4
                   FROM rounddetail
                   GROUP BY round_id) AS rounddetail
            WHERE  round.id = @id
                   AND rounddetail.round_id = round.id
            """;

        // language=sql
        public const string updateRoundDetail = """
            UPDATE rounddetail
            SET    delta1 = @delta1,
                   delta2 = @delta2,
                   delta3 = @delta3,
                   delta4 = @delta4,
                   winner_id = @winner_id,
                   hua_count = @hua_count,
                   hand_id = @hand_id,
                   lezi = @lezi,
                   menqing = @menqing,
                   zimo = @zimo,
                   dianpao_id = @dianpao_id,
                   qianggang = @qianggang,
                   huangfan = @huangfan,
                   gangkai = @gangkai,
                   laoyue = @laoyue,
                   chengbao_id = @chengbao_id
            WHERE  id = @id
            """;
    }
}
