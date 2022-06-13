SELECT player_id, 
       Count(*) as count
FROM   dice 
WHERE  dice1 = @dice1
       AND dice2 = @dice2
       AND ( @game_id = 0 
              OR game_id = @game_id ) 
GROUP  BY player_id 
ORDER  BY player_id