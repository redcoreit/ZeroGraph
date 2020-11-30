using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroGraph.Core
{
    internal static class NoAllocSort
    {
        public static void Sort<TItem>(Span<TItem> span, IComparer<TItem> comparer)
            => new SorterGenericArray<TItem>(span, comparer).Sort(0, span.Length);
    }
}
