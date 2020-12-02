using System.Collections.Generic;
using System.IO;
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

        internal void WriteDotFile(string path)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"//{string.Join(", ", Roots)}");

            sb.AppendLine("digraph G");
            sb.AppendLine("{");

            AppendLineAndIndent("rankdir=\"BT\"");
            AppendLineAndIndent("node [shape = plaintext, fontname=\"Consolas\"];");

            foreach (var edge in Graph)
            {
                AppendLineAndIndent($"{edge.Referencer} -> {edge.Referenced}");
            }

            sb.AppendLine("}");

            void AppendLineAndIndent(string text)
            {
                sb.Append(' ', 4);
                sb.AppendLine(text);
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}
