using System;
using System.Buffers;
using System.Collections.Generic;

namespace ZeroGraph.Core
{
    public readonly ref struct ValueGraph<TEdge>
        where TEdge : IEdge
    {
        private readonly TEdge[] _rentedSource;
        private readonly int _length;

        internal ValueGraph(in ReadOnlyMemory<TEdge> edges, bool isInverted)
        {
            Comparer = isInverted ? InlineableComparer<TEdge>.Inverted : InlineableComparer<TEdge>.Normal;
            _rentedSource = ArrayPool<TEdge>.Shared.Rent(edges.Length);
            _length = edges.Length;

            edges.CopyTo(_rentedSource);

            var sortedEdges = _rentedSource.AsSpan(0, _length);
            NoAllocSort.Sort(sortedEdges, Comparer);
        }

        public bool IsInverted
            => Comparer.IsInverted;

        internal InlineableComparer<TEdge> Comparer { get; }

        public void Dispose()
            => ArrayPool<TEdge>.Shared.Return(_rentedSource);

        internal ReadOnlySpan<TEdge> GetEdgesSpan()
            => _rentedSource.AsSpan(0, _length);
    }
}
