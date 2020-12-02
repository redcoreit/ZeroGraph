#pragma warning disable CA1707 

using BenchmarkDotNet.Attributes;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ZeroGrap.Benchmarks.Data;

using ZeroGraph.Core;

namespace ZeroGraph.Benchmarks.PerfTests
{
    [RyuJitX64Job]
    [MemoryDiagnoser]
    public class Sorting
    {
        private readonly ReadOnlyMemory<Edge> _source;

        public Sorting()
        {
            const int nodeCount = 400;
            const int minDepth = 5;
            const int maxOutgoingEdge = 3;

            var graph = DaGraphBuilder.Build(nodeCount, minDepth, maxOutgoingEdge);
            _source = graph.Graph.ToArray();
        }

        [Benchmark]
        public void Array_sort_over_edegs()
        {
            var array = ArrayPool<Edge>.Shared.Rent(_source.Length);
            _source.CopyTo(array);
            var span = array.AsSpan(0, _source.Length);

            span.Sort(InlineableComparer<Edge>.Normal);

            ArrayPool<Edge>.Shared.Return(array);
        }

        [Benchmark]
        public void NoAlloc_sort_over_edegs()
        {
            var array = ArrayPool<Edge>.Shared.Rent(_source.Length);
            _source.CopyTo(array);
            var span = array.AsSpan(0, _source.Length);

            NoAllocSort.Sort(span, InlineableComparer<Edge>.Normal);

            ArrayPool<Edge>.Shared.Return(array);
        }
    }
}
