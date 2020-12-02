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
    public class ValueGraphHandling
    {
        private readonly ReadOnlyMemory<Edge> _source;
        private readonly int _startNode;

        HashSet<int> result;

        public ValueGraphHandling(bool writeToFile = true)
        {
            const int nodeCount = 400;
            const int minDepth = 5;
            const int maxOutgoingEdge = 3;

            var graph = DaGraphBuilder.Build(nodeCount, minDepth, maxOutgoingEdge);
            _source = graph.Graph.ToArray();

            result = new(minDepth * 10);

            if (writeToFile)
                graph.WriteDotFile("graph-debug.dot");

            _startNode = graph.Roots.First();

            Console.WriteLine($"StartNode: {_startNode}");
            Console.WriteLine();
        }

        [Benchmark]
        public void ZeroGraph_inverse_walk_hashset()
        {
            using var graph = _source.ToValueGraph(true);
            graph.DepthFirstTraversal(_startNode, result);
        }

        [Benchmark]
        public void ZeroGraph_inverse_walk_action()
        {
            var graph = _source.ToValueGraph(true);
            graph.DepthFirstTraversal(_startNode, Array.Empty<int>(), static (arg, m) => { });
            graph.Dispose();
        }
    }
}
