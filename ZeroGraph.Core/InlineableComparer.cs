#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZeroGraph.Core
{
    internal sealed class InlineableComparer<TEdge> : IComparer<TEdge>
        where TEdge : IEdge
    {
        private InlineableComparer(bool isInverted)
        {
            IsInverted = isInverted;
        }

        public static InlineableComparer<TEdge> Normal { get; } = new InlineableComparer<TEdge>(false);

        public static InlineableComparer<TEdge> Inverted { get; } = new InlineableComparer<TEdge>(true);

        public bool IsInverted { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(TEdge a, TEdge b)
            => IsInverted
            ? Compare(a.Referenced, b.Referenced)
            : Compare(a.Referencer, b.Referencer);

        public InlineableComparable GetComparable(in int value)
            => new InlineableComparable(IsInverted, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Compare(int ref1, int ref2)
        {
            if (ref1 == ref2)
            {
                return 0;
            }

            if (ref1 < ref2)
            {
                return -1;
            }

            return 1;
        }

        public readonly struct InlineableComparable : IComparable<TEdge>
        {
            private readonly bool _isInverted;
            private readonly int _value;

            internal InlineableComparable(bool isInverted, int value)
            {
                _isInverted = isInverted;
                _value = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]

            public int CompareTo(TEdge other)
                => InlineableComparer<TEdge>.Compare(_value, _isInverted ? other.Referenced : other.Referencer);
        }
    }
}
