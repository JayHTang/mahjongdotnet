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
--SELECT CAST(SCOPE_IDENTITY() AS INT);