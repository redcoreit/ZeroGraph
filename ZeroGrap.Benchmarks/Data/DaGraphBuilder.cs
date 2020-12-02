using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using ZeroGraph.Core;

namespace ZeroGrap.Benchmarks.Data
{
    public class DaGraphBuilder
    {
        private readonly int _nodeCount;
        private readonly int _minDepth;
        private readonly int _maxOutgoingEdge;
        private readonly Random _rnd;

        private int _nextNodeId;

        private DaGraphBuilder(int nodeCount, int minDepth, int maxOutgoingEdge)
        {
            _nodeCount = nodeCount;
            _minDepth = minDepth;
            _maxOutgoingEdge = maxOutgoingEdge;
            _rnd = new Random();
        }

        public static DaGraph Build(int nodeCount, int minDepth, int maxOutgoingEdge)
        {
            if (nodeCount < 2)
            {
                throw new ArgumentException("Node count shall be greater than 2.");
            }

            if ((uint)minDepth >= (uint)nodeCount)
            {
                throw new ArgumentException("Node count shall be greater than min depth.");
            }

            var instance = new DaGraphBuilder(nodeCount, minDepth, maxOutgoingEdge);

            return instance.BuildGraph();
        }

        private DaGraph BuildGraph()
        {
            var bootstrapperEdges = new List<Edge>();
            var rootNodeCount = _rnd.Next(1, _nodeCount / _minDepth);

            while(rootNodeCount-- > 0)
            {
                var referenced = GetNodeId();
                bootstrapperEdges.Add(new Edge(GetNodeId(), referenced));
            }

            var graphs = new List<Edge>(bootstrapperEdges);
            var budget = _nodeCount;

            int depth = 0;

            BuildRecursive(graphs, ref budget, ref depth);

            if (graphs.Count == 1)
            {
                throw new InvalidOperationException($"Graph is empty.");
            }

            if (depth < _minDepth)
            {
                throw new InvalidOperationException($"Min depth not reached. Depth: {depth.ToString()} Budget: {budget.ToString()}");
            }

            graphs.Reverse();
            var graphSet = graphs.ToHashSet();

            return new DaGraph(bootstrapperEdges.Select(m => m.Referenced).ToList(), graphSet);
        }

        private void BuildRecursive(List<Edge> parentEdges, ref int budget, ref int depth)
        {
            if (depth < _minDepth && budget == 0)
            {
                throw new InvalidOperationException($"Min depth not reached but budget is exceeded. Depth: {depth.ToString()}");
            }

            if (budget == 0)
            {
                return;
            }

            var depthDiff = _minDepth - depth;
            var localBudget = depthDiff > 0 ? budget / depthDiff : budget;
            var usage = _rnd.Next(1, localBudget + 1);

            if (localBudget == 0)
            {
                throw new InvalidOperationException($"Min depth not reached but local budget is 0. Depth: {depth.ToString()}");
            }

            budget -= usage;
            depth++;

            var result = new List<Edge>();

            while (usage > 0)
            {
                var nodeId = GetNodeId();
                var outgoingEdgeCount = _rnd.Next(1, _maxOutgoingEdge + 1);

                while (outgoingEdgeCount > 0)
                {
                    var edge = CreateRandomEdge(parentEdges, nodeId);
                    result.Add(edge);

                    outgoingEdgeCount--;
                }
                usage--;
            }

            parentEdges.AddRange(result);

            BuildRecursive(parentEdges, ref budget, ref depth);
        }

        private Edge CreateRandomEdge(IReadOnlyList<Edge> parentEdges, int referencerNodeId)
        {
            var referencedEdge = _rnd.Next(0, parentEdges.Count);
            var useReferencer = referencedEdge != 0 ? _rnd.Next(0, 2) == 1 : true;
            var referencedNodeId = useReferencer
                ? parentEdges[referencedEdge].Referencer
                : parentEdges[referencedEdge].Referenced;

            return new Edge(referencerNodeId, referencedNodeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNodeId() => _nextNodeId++;
    }
}
