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