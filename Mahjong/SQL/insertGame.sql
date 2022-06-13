INSERT INTO game
           (player1_id,
            player2_id,
            player3_id,
            player4_id,
            hua,
            lezi,
            huangfan,
			huangfan_executed,
			finished,
            created,
			prev_game_id) 
VALUES     (@player1_id,
            @player2_id,
            @player3_id,
            @player4_id,
            @hua,
            @lezi,
            0,
			0,
			0,
            datetime('now','localtime'),
			NULL);
--SELECT CAST(SCOPE_IDENTITY() AS INT);