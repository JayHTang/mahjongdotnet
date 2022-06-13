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