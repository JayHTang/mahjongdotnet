UPDATE round 
SET    delta1 = rounddetail.delta1, 
       delta2 = rounddetail.delta2, 
       delta3 = rounddetail.delta3, 
       delta4 = rounddetail.delta4
FROM (
       SELECT round_id,
              SUM(delta1) as delta1, 
              SUM(delta2) as delta2, 
              SUM(delta3) as delta3, 
              SUM(delta4) as delta4
       FROM rounddetail
       GROUP BY round_id) AS rounddetail
WHERE  round.id = @id 
       AND rounddetail.round_id = round.id