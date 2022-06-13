using System.Collections.Generic;

namespace Mahjong.Models.Mahjong
{
    public class Dice
    {
        public readonly int dice1;
        public readonly int dice2;

        public Dice(int dice1, int dice2)
        {
            this.dice1 = dice1;
            this.dice2 = dice2;
        }
    }

    public class DiceRoll
    {
        public readonly Dice dice;
        private readonly Dictionary<int, int> book; // key player_id, value count

        public DiceRoll(Dice dice)
        {
            this.dice = dice;
            book = new Dictionary<int, int>();
        }

        public void AddRollCount(int player_id, int count)
        {
            if (book.ContainsKey(player_id))
            {
                book[player_id] += count;
            }
            else
            {
                book.Add(player_id, count);
            }
        }

        public int GetPlayerRollCount(int player_id)
        {
            if (book.ContainsKey(player_id))
            {
                return book[player_id];
            }
            else
            {
                return 0;
            }
        }
    }
}