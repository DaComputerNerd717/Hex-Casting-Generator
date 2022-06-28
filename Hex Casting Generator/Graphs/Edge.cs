using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    public class Edge
    {
        public Node Node1, Node2;

        public Edge(Node n1, Node n2)
        {
            Node1 = n1;
            Node2 = n2;
        }

        public bool IsEqual(Edge other)
        {
            return other != null && ((Node1.IsEqual(other.Node1) && Node2.IsEqual(other.Node2)) || (Node1.IsEqual(other.Node2) && Node2.IsEqual(other.Node1)));
        }

        public bool IsIncident(Node n)
        {
            return n == Node1 || n == Node2;
        }

        public Node OppositeNode(Node n)
        {
            return n.IsEqual(Node1) ? Node2 : Node1;
        }

        public override bool Equals(object? obj)
        {
            return (obj as Edge)?.IsEqual(this) ?? false;
        }

        public override int GetHashCode()
        {
            return Node1.GetHashCode() < Node2.GetHashCode() ? (Node1.GetHashCode() << 16) + Node2.GetHashCode() : (Node2.GetHashCode() << 16) + Node1.GetHashCode();
        }
    }
}
