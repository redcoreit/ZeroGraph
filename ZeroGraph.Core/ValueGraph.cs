using System;
using System.Buffers;
using System.Collections.Generic;

namespace ZeroGraph.Core
{
    public readonly ref struct ValueGraph<TEdge>
        where TEdge : IEdge
    {
        private readonly TEdge[] _edges;
        private readonly int _length;
        private readonly bool _isRented;

        internal ValueGraph(in ReadOnlyMemory<TEdge> edges, bool isInverted)
        {
            _isRented = true;
            Comparer = isInverted ? InlineableComparer<TEdge>.Inverted : InlineableComparer<TEdge>.Normal;
            _edges = ArrayPool<TEdge>.Shared.Rent(edges.Length);
            _length = edges.Length;

            edges.CopyTo(_edges);

            var sortedEdges = _edges.AsSpan(0, _length);
            NoAllocSort.Sort(sortedEdges, Comparer);
        }

        internal ValueGraph(TEdge[] edges, InlineableComparer<TEdge> comparer)
        {
            _isRented = false;
            _edges = edges;
            _length = edges.Length;
            Comparer = comparer;
        }

        public bool IsInverted
            => Comparer.IsInverted;

        internal InlineableComparer<TEdge> Comparer { get; }

        public void Dispose()
        {
            if(_isRented)
            { 
                ArrayPool<TEdge>.Shared.Return(_edges); 
            }
        }

        internal ReadOnlySpan<TEdge> GetEdgesSpan()
            => _edges.AsSpan(0, _length);
    }
}
