using System;
using System.Collections.Generic;
using System.Numerics;

namespace ZeroGraph.Core
{
    // https://source.dot.net/#System.Private.CoreLib/Array.cs,a26f9995088379ee

    // Private value used by the Sort methods for instances of Array.
    // This is slower than the one for Object[], since we can't use the JIT helpers
    // to access the elements.  We must use GetValue & SetValue.
    internal readonly ref struct SorterGenericArray<TItem>
    {
        // This is the threshold where Introspective sort switches to Insertion sort.
        // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        // Large value types may benefit from a smaller number.
        internal const int IntrosortSizeThreshold = 16;

        private readonly Span<TItem> _span;
        private readonly IComparer<TItem> _comparer;

        internal SorterGenericArray(Span<TItem> span, IComparer<TItem> comparer)
        {
            _span = span;
            _comparer = comparer;
        }

        internal void SwapIfGreater(int a, int b)
        {
            if (a != b)
            {
                if (_comparer.Compare(_span[a], _span[b]) > 0)
                {
                    var tmp = _span[a];
                    _span[a] = _span[b];
                    _span[b] = tmp;
                }
            }
        }

        private void Swap(int i, int j)
        {
            var t1 = _span[i];
            _span[i] = _span[j];
            _span[j] = t1;
        }

        internal void Sort(int left, int length)
        {
            IntrospectiveSort(left, length);
        }

        private void IntrospectiveSort(int left, int length)
        {
            if (length < 2)
                return;

            IntroSort(left, length + left - 1, 2 * (BitOperations.Log2((uint)length) + 1));
        }

        private void IntroSort(int lo, int hi, int depthLimit)
        {
            while (hi > lo)
            {
                int partitionSize = hi - lo + 1;
                if (partitionSize <= IntrosortSizeThreshold)
                {
                    if (partitionSize == 2)
                    {
                        SwapIfGreater(lo, hi);
                        return;
                    }

                    if (partitionSize == 3)
                    {
                        SwapIfGreater(lo, hi - 1);
                        SwapIfGreater(lo, hi);
                        SwapIfGreater(hi - 1, hi);
                        return;
                    }

                    InsertionSort(lo, hi);
                    return;
                }

                if (depthLimit == 0)
                {
                    Heapsort(lo, hi);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(lo, hi);
                IntroSort(p + 1, hi, depthLimit);
                hi = p - 1;
            }
        }

        private int PickPivotAndPartition(int lo, int hi)
        {
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int mid = lo + (hi - lo) / 2;

            SwapIfGreater(lo, mid);
            SwapIfGreater(lo, hi);
            SwapIfGreater(mid, hi);

            var pivot = _span[mid];
            Swap(mid, hi - 1);
            int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (_comparer.Compare(_span[++left], pivot) < 0) ;
                while (_comparer.Compare(pivot, _span[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1)
            {
                Swap(left, hi - 1);
            }
            return left;
        }

        private void Heapsort(int lo, int hi)
        {
            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; i--)
            {
                DownHeap(i, n, lo);
            }
            for (int i = n; i > 1; i--)
            {
                Swap(lo, lo + i - 1);

                DownHeap(1, i - 1, lo);
            }
        }

        private void DownHeap(int i, int n, int lo)
        {
            var d = _span[lo + i - 1];
            int child;

            while (i <= n / 2)
            {
                child = 2 * i;
                if (child < n && _comparer.Compare(_span[lo + child - 1], _span[lo + child]) < 0)
                {
                    child++;
                }

                if (!(_comparer.Compare(d, _span[lo + child - 1]) < 0))
                    break;

                _span[lo + i - 1] = _span[lo + child - 1];
                
                i = child;
            }
            _span[lo + i - 1] = d;
        }

        private void InsertionSort(int lo, int hi)
        {
            int i, j;
            TItem t;
            
            for (i = lo; i < hi; i++)
            {
                j = i;
                t = _span[i + 1];

                while (j >= lo && _comparer.Compare(t, _span[j]) < 0)
                {
                    _span[j + 1] = _span[j];
                    j--;
                }

                _span[j + 1] = t;
            }
        }
    }
}
