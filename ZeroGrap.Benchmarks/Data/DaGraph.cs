using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZeroGraph.Core;

namespace ZeroGrap.Benchmarks.Data
{
    public sealed class DaGraph
    {
        public DaGraph(IEnumerable<int> roots, IEnumerable<Edge> graph)
        {
            Roots = roots;
            Graph = graph;
        }

        public IEnumerable<int> Roots { get; }

        public IEnumerable<Edge> Graph { get; }
    }
}
