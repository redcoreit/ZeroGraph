using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroGraph.Core
{
    public class GraphSource<TEdge>
        where TEdge : IEdge
    {
        private readonly TEdge[] _edges;
        private readonly InlineableComparer<TEdge> _comparer;

        public GraphSource(IEnumerable<TEdge> edges, bool invertCachedGraph)
        {
            _edges = edges.ToArray();
            IsInverted = invertCachedGraph;

            _comparer = invertCachedGraph ? InlineableComparer<TEdge>.Inverted : InlineableComparer<TEdge>.Normal;
            NoAllocSort.Sort(_edges, _comparer);
        }

        public ValueGraph<TEdge> Value => new ValueGraph<TEdge>(_edges, _comparer);

        public bool IsInverted { get; }

        public ValueGraph<TEdge> GetInverted()
            => GraphExtensions.ToValueGraph<TEdge>(_edges, !IsInverted);
    }
}
