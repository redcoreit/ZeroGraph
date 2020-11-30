using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroGraph.Core
{
    public static partial class GraphSourceExtensions
    {
        public static GraphSource<TEdge> ToGraphSource<TEdge>(this ReadOnlyMemory<TEdge> edges, bool invert = false)
            where TEdge : IEdge
            => new GraphSource<TEdge>(in edges, invert);

        public static void DepthFirstTraversal<TEdge>(this in GraphSource<TEdge> graph, int startNode, ICollection<int> visited, bool reportStartNode = true)
            where TEdge : IEdge
            => DepthFirstTraversal(in graph, startNode, visited, static (arg, m) => arg.Add(m), reportStartNode);

        public static void DepthFirstTraversal<TEdge, TArg>(this in GraphSource<TEdge> graph, int startNode, TArg arg, Action<TArg, int> visit, bool reportStartNode = true)
            where TEdge : IEdge
            where TArg : class
        {
            var nodes = Locate(in graph, startNode);

            if (nodes.Length == 0)
            {
                return;
            }

            if (reportStartNode)
            {
                visit(arg, startNode);
            }

            var idx = 0;
            while (idx < nodes.Length)
            {
                var node = graph.IsInverted
                    ? nodes[idx++].Referencer
                    : nodes[idx++].Referenced;

                visit(arg, node);
                DepthFirstTraversal(in graph, node, arg, visit, false);
            }
        }

        private static ReadOnlySpan<TEdge> Locate<TEdge>(in GraphSource<TEdge> graph, int node)
            where TEdge : IEdge
        {
            var edges = graph.GetEdgesSpan();
            var comparable = graph.Comparer.GetComparable(node);
            var idx = edges.BinarySearch(comparable);

            if (idx == edges.Length || idx < 0)
            {
                return Array.Empty<TEdge>();
            }

            var start = idx; // Inclusive
            var end = idx; // Exclusive

            while (start > 0)
            {
                if (graph.Comparer.Compare(edges[start - 1], edges[idx]) == 0)
                {
                    start--;
                    continue;
                }

                break;
            }

            while (end < edges.Length)
            {
                if (graph.Comparer.Compare(edges[end], edges[idx]) != 0)
                {
                    break;
                }

                end++;
            }

            return edges.Slice(start, end - start);
        }
    }
}
