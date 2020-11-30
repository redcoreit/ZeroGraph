using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroGraph.Core
{
    public record Edge(int Referencer, int Referenced) : IEdge;

    public interface IEdge
    {
        int Referencer { get; }
        
        int Referenced { get; }
    }
}
