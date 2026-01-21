using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Algorithms.WeightedAStar
{
    internal sealed class MinPriorityQueue<TKey,TValue>
        where TKey : IComparable<TKey>
    {
        private readonly List<(TKey key, TValue value)> _heap = new();
        public int Count => _heap.Count;

        public void Enqueue(TKey key, TValue value)
        {
            _heap.Add((key, value));
            SiftUp(Count - 1);
        }

        public bool TryDequeue(out TKey key, out TValue value)
        {
            if (_heap.Count == 0)
            {
                key = default!;
                value = default!;
                return false;
            }

            (key, value) = _heap[0];

            var last = _heap[^1];
            _heap[0] = last;
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
            {
                SiftDown(0);
            }

            return true;
        }

        public (TKey key, TValue value) Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty.");

            return _heap[0];
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) >> 1;
                if (_heap[index].key.CompareTo(_heap[parent].key) >= 0)
                    break;

                (_heap[index], _heap[parent]) = (_heap[parent], _heap[index]);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int n = _heap.Count;
            while (true)
            {
                int left = (index << 1) + 1;
                int right = left + 1;
                int smallest = index;

                if (left < n && _heap[left].key.CompareTo(_heap[smallest].key) < 0)
                    smallest = left;

                if (right < n && _heap[right].key.CompareTo(_heap[smallest].key) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
                index = smallest;
            }
        }
    }
}
