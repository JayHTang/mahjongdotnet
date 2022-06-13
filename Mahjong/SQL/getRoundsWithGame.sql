SELECT * 
FROM   round r 
       JOIN game g 
         ON r.game_id = g.id 
WHERE  @game_id = 0 OR r.game_id = @game_id
ORDER BY r.created