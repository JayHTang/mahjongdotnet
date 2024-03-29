﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mahjong {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SQL {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SQL() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Mahjong.SQL", typeof(SQL).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT player_id, 
        ///       Count(*) as count
        ///FROM   dice 
        ///WHERE  dice1 = @dice1
        ///       AND dice2 = @dice2
        ///       AND ( @game_id = 0 
        ///              OR game_id = @game_id ) 
        ///GROUP  BY player_id 
        ///ORDER  BY player_id.
        /// </summary>
        internal static string getDiceRolled {
            get {
                return ResourceManager.GetString("getDiceRolled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT dice1, 
        ///       dice2, 
        ///       Count(*) as count
        ///FROM   dice 
        ///WHERE  player_id = @player_id 
        ///       AND ( @game_id = 0 
        ///              OR game_id = @game_id ) 
        ///GROUP  BY dice1, 
        ///          dice2 
        ///ORDER  BY dice1, 
        ///          dice2; .
        /// </summary>
        internal static string getDiceRolledByPlayer {
            get {
                return ResourceManager.GetString("getDiceRolledByPlayer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM game WHERE id = @gameId.
        /// </summary>
        internal static string getGame {
            get {
                return ResourceManager.GetString("getGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT g.id           AS game_id, 
        ///       g.hua          AS hua, 
        ///       g.lezi         AS lezi, 
        ///       g.prev_game_id AS prev_game_id, 
        ///       Min(g.created) AS starttime, 
        ///       Max(r.created) AS endtime, 
        ///       Count(r.id)    AS numOfRounds,
        ///	   g.player1_id   AS player1_id,
        ///	   g.player2_id   AS player2_id,
        ///	   g.player3_id   AS player3_id,
        ///	   g.player4_id   AS player4_id,
        ///	   Sum(r.delta1)  AS delta1,
        ///	   Sum(r.delta2)  AS delta2,
        ///	   Sum(r.delta3)  AS delta3,
        ///	   Sum(r.delta4)  AS de [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string getGameHistoryInfo {
            get {
                return ResourceManager.GetString("getGameHistoryInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM game.
        /// </summary>
        internal static string getGames {
            get {
                return ResourceManager.GetString("getGames", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM game where finished = 0.
        /// </summary>
        internal static string getGamesUnfinished {
            get {
                return ResourceManager.GetString("getGamesUnfinished", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM hand WHERE id=@handId.
        /// </summary>
        internal static string getHand {
            get {
                return ResourceManager.GetString("getHand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM hand.
        /// </summary>
        internal static string getHands {
            get {
                return ResourceManager.GetString("getHands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * 
        ///FROM   (SELECT round.*,
        ///               delta1 AS winning, 
        ///               game.created, 
        ///               hua
        ///        FROM   round 
        ///               INNER JOIN game 
        ///                       ON round.game_id = game.id 
        ///        WHERE  delta1 &gt; 0 
        ///        UNION 
        ///        SELECT round.*,
        ///               delta2 AS winning, 
        ///               game.created, 
        ///               hua
        ///        FROM   round 
        ///               INNER JOIN game 
        ///                       ON round.game_id = game.id 
        ///        WHERE  de [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string getHighestWinnings {
            get {
                return ResourceManager.GetString("getHighestWinnings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM rounddetail WHERE round_id = (SELECT MAX(id) FROM round WHERE game_id = @gameId) ORDER BY created DESC.
        /// </summary>
        internal static string getLastRoundDetail {
            get {
                return ResourceManager.GetString("getLastRoundDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM game WHERE prev_game_id = @gameId.
        /// </summary>
        internal static string getNextGame {
            get {
                return ResourceManager.GetString("getNextGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT Sum(t.numOfRounds) AS roundCount, 
        ///       Max(t.lastPlayed)     AS lastPlayed
        ///FROM   (SELECT g.id         AS game_id, 
        ///               g.hua        AS hua, 
        ///               Count(r.id)  AS numOfRounds, 
        ///               Max(r.created)     AS lastPlayed,
        ///               g.player1_id AS player1_id, 
        ///               g.player2_id AS player2_id, 
        ///               g.player3_id AS player3_id, 
        ///               g.player4_id AS player4_id
        ///        FROM   game AS g 
        ///               JOIN round AS r 
        ///            [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string getNumOfRoundsPlayed {
            get {
                return ResourceManager.GetString("getNumOfRoundsPlayed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM player.
        /// </summary>
        internal static string getPlayers {
            get {
                return ResourceManager.GetString("getPlayers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM round WHERE game_id = @gameId AND id = @id.
        /// </summary>
        internal static string getRound {
            get {
                return ResourceManager.GetString("getRound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * FROM rounddetail WHERE @roundId = 0 OR round_id = @roundId.
        /// </summary>
        internal static string getRoundDetail {
            get {
                return ResourceManager.GetString("getRoundDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * 
        ///FROM   round 
        ///       LEFT OUTER JOIN dice 
        ///                    ON dice.game_id = round.game_id 
        ///                       AND dice.round_id = round.id 
        ///WHERE  round.game_id = @gameId 
        ///ORDER  BY round.id .
        /// </summary>
        internal static string getRounds {
            get {
                return ResourceManager.GetString("getRounds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT * 
        ///FROM   round r 
        ///       JOIN game g 
        ///         ON r.game_id = g.id 
        ///WHERE  @game_id = 0 OR r.game_id = @game_id
        ///ORDER BY r.created.
        /// </summary>
        internal static string getRoundsWithGame {
            get {
                return ResourceManager.GetString("getRoundsWithGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO dice
        ///           (dice1,
        ///            dice2,
        ///            game_id,
        ///            round_id,
        ///            player_id,
        ///            created) 
        ///VALUES     (@dice1,
        ///            @dice2,
        ///            @game_id,
        ///            @round_id,
        ///            @player_id,
        ///            datetime(&apos;now&apos;,&apos;localtime&apos;)) 
        ///.
        /// </summary>
        internal static string insertDice {
            get {
                return ResourceManager.GetString("insertDice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO game
        ///           (player1_id,
        ///            player2_id,
        ///            player3_id,
        ///            player4_id,
        ///            hua,
        ///            lezi,
        ///            huangfan,
        ///			huangfan_executed,
        ///			finished,
        ///            created,
        ///			prev_game_id) 
        ///VALUES     (@player1_id,
        ///            @player2_id,
        ///            @player3_id,
        ///            @player4_id,
        ///            @hua,
        ///            @lezi,
        ///            0,
        ///			0,
        ///			0,
        ///            datetime(&apos;now&apos;,&apos;localtime&apos;),
        ///			NULL);
        ///--SELECT CAST(SCOPE_IDENTITY() [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string insertGame {
            get {
                return ResourceManager.GetString("insertGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO game
        ///           (player1_id,
        ///            player2_id,
        ///            player3_id,
        ///            player4_id,
        ///            hua,
        ///            lezi,
        ///            huangfan,
        ///			huangfan_executed,
        ///			finished,
        ///            created,
        ///			prev_game_id) 
        ///VALUES     (@player1_id,
        ///            @player2_id,
        ///            @player3_id,
        ///            @player4_id,
        ///            @hua,
        ///            @lezi,
        ///            @huangfan,
        ///			0,
        ///			0,
        ///            datetime(&apos;now&apos;,&apos;localtime&apos;),
        ///			@prev_game_id);
        ///--SELECT CAST [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string insertGameWithPrevGame {
            get {
                return ResourceManager.GetString("insertGameWithPrevGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO player (name) VALUES(@name).
        /// </summary>
        internal static string insertPlayer {
            get {
                return ResourceManager.GetString("insertPlayer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO round
        ///           (game_id,
        ///            delta1,
        ///            delta2,
        ///            delta3,
        ///            delta4,
        ///            created) 
        ///VALUES     (@game_id,
        ///            @delta1,
        ///            @delta2,
        ///            @delta3,
        ///            @delta4,
        ///            datetime(&apos;now&apos;,&apos;localtime&apos;));
        ///--SELECT CAST(SCOPE_IDENTITY() AS INT);.
        /// </summary>
        internal static string insertRound {
            get {
                return ResourceManager.GetString("insertRound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO rounddetail
        ///           (round_id,
        ///            delta1,
        ///            delta2,
        ///            delta3,
        ///            delta4,
        ///            winner_id,
        ///            hua_count,
        ///            hand_id,
        ///            lezi,
        ///            menqing,
        ///            zimo,
        ///            dianpao_id,
        ///            qianggang,
        ///            huangfan,
        ///            gangkai,
        ///            laoyue,
        ///            chengbao_id,
        ///            created) 
        ///VALUES     (@round_id,
        ///            @delta1,
        ///            @delta2,
        ///            @delta [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string insertRoundDetail {
            get {
                return ResourceManager.GetString("insertRoundDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE game SET huangfan = huangfan - 1 WHERE id = @gameId.
        /// </summary>
        internal static string updateGameDecrementHuangfan {
            get {
                return ResourceManager.GetString("updateGameDecrementHuangfan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE game SET finished = 1 WHERE id = @gameId.
        /// </summary>
        internal static string updateGameFinishGame {
            get {
                return ResourceManager.GetString("updateGameFinishGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE game SET huangfan = huangfan + 1 WHERE id = @gameId.
        /// </summary>
        internal static string updateGameIncrementHuangfan {
            get {
                return ResourceManager.GetString("updateGameIncrementHuangfan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE game SET huangfan_executed = huangfan_executed + 1 WHERE id = @gameId.
        /// </summary>
        internal static string updateGameIncrementHuangfanExecuted {
            get {
                return ResourceManager.GetString("updateGameIncrementHuangfanExecuted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE round 
        ///SET    delta1 = rounddetail.delta1, 
        ///       delta2 = rounddetail.delta2, 
        ///       delta3 = rounddetail.delta3, 
        ///       delta4 = rounddetail.delta4
        ///FROM (
        ///       SELECT round_id,
        ///              SUM(delta1) as delta1, 
        ///              SUM(delta2) as delta2, 
        ///              SUM(delta3) as delta3, 
        ///              SUM(delta4) as delta4
        ///       FROM rounddetail
        ///       GROUP BY round_id) AS rounddetail
        ///WHERE  round.id = @id 
        ///       AND rounddetail.round_id = round.id.
        /// </summary>
        internal static string updateRound {
            get {
                return ResourceManager.GetString("updateRound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE rounddetail
        ///SET    delta1 = @delta1, 
        ///       delta2 = @delta2, 
        ///       delta3 = @delta3, 
        ///       delta4 = @delta4, 
        ///       winner_id = @winner_id, 
        ///       hua_count = @hua_count, 
        ///       hand_id = @hand_id, 
        ///       lezi = @lezi, 
        ///       menqing = @menqing, 
        ///       zimo = @zimo, 
        ///       dianpao_id = @dianpao_id, 
        ///       qianggang = @qianggang, 
        ///       huangfan = @huangfan, 
        ///       gangkai = @gangkai, 
        ///       laoyue = @laoyue, 
        ///       chengbao_id = @chengbao_id 
        ///WHERE  id = @id .
        /// </summary>
        internal static string updateRoundDetail {
            get {
                return ResourceManager.GetString("updateRoundDetail", resourceCulture);
            }
        }
    }
}
