using System.Collections.Generic;

namespace GamesToGo.API.Models
{
    public class CircularList<T>
    {
        private readonly List<T> list;

        public CircularList()
        {
            list = new List<T>();
        }

        public CircularList(IEnumerable<T> items)
        {
            list = new List<T>(items);
        }

        private T this[int index] => list[index];

        public int Count => list.Count;

        private int currentIndex = -1;

        /// <summary>
        /// Gets the item currently set as pivot, default if <see cref="MoveNext"/> hasn't been called yet
        /// </summary>
        public T Current => currentIndex < 0 ? default : this[currentIndex];

        public bool MoveNext()
        {
            if (Count == 0)
                return false;
            currentIndex = ++currentIndex % list.Count;
            return true;
        }
    }
}