SELECT * 
FROM   round 
       LEFT OUTER JOIN dice 
                    ON dice.game_id = round.game_id 
                       AND dice.round_id = round.id 
WHERE  round.game_id = @gameId 
ORDER  BY round.id 