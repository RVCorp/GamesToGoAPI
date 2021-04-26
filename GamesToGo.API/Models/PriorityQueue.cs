using System.Collections.Generic;

namespace GamesToGo.API.Models
{
    public class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> queueSorter;

        public PriorityQueue()
        {
            queueSorter = new SortedDictionary<int, Queue<T>>();
        }

        public int Count
        {
            get
            {
                if (queueSorter.Count == 0)
                    return 0;
                
                var count = 0;

                foreach (var queue in queueSorter.Values)
                    count += queue.Count;

                return count;
            }
        }

        public void Enqueue(T item, int priority = 0)
        {
            if (queueSorter.ContainsKey(priority))
                queueSorter[priority].Enqueue(item);
            else
                queueSorter.Add(priority, new Queue<T>(new [] { item }));
        }

        public void EnqueueRange(IEnumerable<T> items, int priority = 0)
        {
            foreach(var item in items)
                Enqueue(item, priority);
        }

        public bool TryDequeue(out T item)
        {
            if (Count == 0)
            {
                item = default;
                return false;
            }

            foreach (var queue in queueSorter.Values)
            {
                if (!queue.TryDequeue(out var result)) 
                    continue;
                
                item = result;
                return true;
            }

            item = default;
            return false;
        }
    }
}