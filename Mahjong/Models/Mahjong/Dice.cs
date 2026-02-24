using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class Dice(int dice1, int dice2)
    {
        public readonly int dice1 = dice1;
        public readonly int dice2 = dice2;
    }

    public class DiceRoll(Dice dice)
    {
        public readonly Dice dice = dice;
        private readonly Dictionary<int, int> book = []; // key player_id, value count

        public void AddRollCount(int player_id, int count)
        {
            if (!book.TryAdd(player_id, count))
            {
                book[player_id] += count;
            }
        }

        public int GetPlayerRollCount(int player_id)
        {
            if (book.TryGetValue(player_id, out int value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }
    }
}