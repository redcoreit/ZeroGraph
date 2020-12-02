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
    public class GraphSourceHandling
    {
        private readonly GraphSource<Edge> _source;
        private readonly int _startNode;

        private readonly HashSet<int> _result;

        public GraphSourceHandling(bool writeToFile = true)
        {
            const int nodeCount = 400;
            const int minDepth = 5;
            const int maxOutgoingEdge = 3;

            var graph = DaGraphBuilder.Build(nodeCount, minDepth, maxOutgoingEdge);
            _source = new GraphSource<Edge>(graph.Graph, true);

            _result = new(minDepth * 10);

            if (writeToFile)
                graph.WriteDotFile("gsh-graph-debug.dot");

            _startNode = graph.Roots.First();

            Console.WriteLine($"StartNode: {_startNode.ToString()}");
            Console.WriteLine();
        }

        [Benchmark]
        public void GraphSource_cached_direction_walk()
        {
            using var graph = _source.Value;
            graph.DepthFirstTraversal(_startNode, Array.Empty<int>(), static (arg, m) => { });
        }

        [Benchmark]
        public void GraphSource_inverted_direction_walk()
        {
            using var graph = _source.GetInverted();
            graph.DepthFirstTraversal(_startNode, Array.Empty<int>(), static (arg, m) => { });
        }
    }
}
