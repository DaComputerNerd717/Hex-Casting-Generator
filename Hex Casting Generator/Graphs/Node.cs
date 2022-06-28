using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    public class Node
    {
        public Dictionary<int, Edge?> edges;
        ///Diagonal coordinates
        public int u, v;

        public Node()
        {
            edges = new();
        }

        public bool IsEqual(Node n)
        {
            return n != null && u == n.u && v == n.v;
        }

        public bool IsEqualNullable(Node? n)
        {
            return n != null && IsEqual(n);
        }

        public Edge GetEdgeBetween(Node node)
        {
            return edges.Select(kv=>kv.Value).Where(e=>e?.IsIncident(node) ?? false).FirstOrDefault()!;
        }

        public override bool Equals(object? obj)
        {
            return (obj as Node)?.IsEqual(this) ?? false;
        }

        public override int GetHashCode()
        {
            return (u << 8) + v;
        }
    }
}
