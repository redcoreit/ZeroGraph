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

namespace ZeroGraph.Benchmarks
{
    [RyuJitX64Job]
    [MemoryDiagnoser]
    public class GraphTestBed
    {
        private readonly ReadOnlyMemory<int> _roots;
        private readonly ReadOnlyMemory<Edge> _source;
        private readonly int _startNode;

        HashSet<int> result;

        public GraphTestBed(bool writeToFile = false)
        {
            const int nodeCount = 400;
            const int minDepth = 5;
            const int maxOutgoingEdge = 3;

            var graph = DaGraphBuilder.Build(nodeCount, minDepth, maxOutgoingEdge);
            _source = graph.Graph.ToArray();
            _roots = graph.Roots.ToArray();

            result = new(minDepth * 10);

            if (writeToFile)
                WriteDotFile();

            _startNode = graph.Roots.First();

            Console.WriteLine($"StartNode: {_startNode}");
            Console.WriteLine();

            GC.Collect(2, GCCollectionMode.Forced, true, true);
        }

        [Benchmark]
        public void ZeroGraph_inverse_walk_hashset()
        {
            using var graph = _source.ToGraphSource(true);
            graph.DepthFirstTraversal(_startNode, result);
        }

        [Benchmark]
        public void ZeroGraph_inverse_walk_action()
        {
            var graph = _source.ToGraphSource(true);
            graph.DepthFirstTraversal(_startNode, Array.Empty<int>(), static (arg, m) => { });
            graph.Dispose();
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

        private void WriteDotFile()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"//{string.Join(", ", _roots)}");

            sb.AppendLine("digraph G");
            sb.AppendLine("{");

            AppendLineAndIndent("rankdir=\"BT\"");
            AppendLineAndIndent("node [shape = plaintext, fontname=\"Consolas\"];");

            var span = _source.Span;

            for (var idx = 0; idx < _source.Length; idx++)
            {
                var edge = span[idx];
                AppendLineAndIndent($"{edge.Referencer} -> {edge.Referenced}");
            }

            sb.AppendLine("}");

            void AppendLineAndIndent(string text)
            {
                sb.Append(' ', 4);
                sb.AppendLine(text);
            }

            File.WriteAllText("graph-debug.dot", sb.ToString());
        }
    }
}
