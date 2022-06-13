using System.Collections.Generic;

namespace Mahjong.Utility
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        //disable default constructor
        private FixedSizeQueue() { }
        public FixedSizeQueue(int size)
        {
            _size = size;
        }
        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            while (base.Count > _size)
            {
                base.Dequeue();
            }
        }

        private int _size;
    }
}