UPDATE rounddetail
SET    delta1 = @delta1, 
       delta2 = @delta2, 
       delta3 = @delta3, 
       delta4 = @delta4, 
       winner_id = @winner_id, 
       hua_count = @hua_count, 
       hand_id = @hand_id, 
       lezi = @lezi, 
       menqing = @menqing, 
       zimo = @zimo, 
       dianpao_id = @dianpao_id, 
       qianggang = @qianggang, 
       huangfan = @huangfan, 
       gangkai = @gangkai, 
       laoyue = @laoyue, 
       chengbao_id = @chengbao_id 
WHERE  id = @id 