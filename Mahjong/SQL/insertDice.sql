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
