SELECT Sum(t.numOfRounds) AS roundCount, 
       Max(t.lastPlayed)     AS lastPlayed
FROM   (SELECT g.id         AS game_id, 
               g.hua        AS hua, 
               Count(r.id)  AS numOfRounds, 
               Max(r.created)     AS lastPlayed,
               g.player1_id AS player1_id, 
               g.player2_id AS player2_id, 
               g.player3_id AS player3_id, 
               g.player4_id AS player4_id
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