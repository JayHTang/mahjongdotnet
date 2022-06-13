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