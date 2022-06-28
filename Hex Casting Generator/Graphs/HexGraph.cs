using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    public class HexGraph
    {
        public List<Edge> Edges;
        public List<Node> Nodes;
        public Dictionary<Tuple<int, int>, Node> NodeDict;
        public Node defaultStart;
        public int rows, cols;

        /*
        (0,0)   (1,0)   (2,0)   (3,0)
            (0,1)   (1,1)   (2,1)   (3,1)
        (-1,2)  (0,2)   (1,2)   (2,2)
            (-1,3)  (0,3)   (1,3)   (2,3)
        (-2,4), (-1,4)  (0,4)   (1,4)
        Neighboring (1,1) is (1,0), (2,0), (0,1), (2,1), (0,2), (1,2)
        Left end of each row is (-int(r/2), r), right end is (w-int(r/2), r)
        */

        public HexGraph(int rows, int cols)
        {
            Edges = new();
            Nodes = new();
            NodeDict = new();
            this.rows = rows;
            this.cols = cols;
            for(int i = 0; i < rows; i++)
            {
                for(int j = -i / 2; j < cols - i / 2; j++)
                {
                    Node node = new()
                    {
                        u = i,
                        v = j
                    };
                    NodeDict.Add(new Tuple<int, int>(i,j), node);
                }
            }
            for (int i = 0; i < rows; i++)
            {
                for (int j = -i / 2; j < cols - i / 2; j++)
                {
                    Node node = NodeDict[new Tuple<int, int>(i, j)];
                    Tuple<int, int>[] neighbors = NeighborCoords(i, j);
                    for(int k = 0; k < 6; k++)
                    {
                        if (NodeDict.ContainsKey(neighbors[k]))
                        {
                            Edge e = new(node, NodeDict[neighbors[k]]);
                            NodeDict[neighbors[k]].edges[5 - k] = e;
                            node.edges[k] = e;
                            Edges.Add(e);
                            Nodes.Add(node);
                        }
                        else
                        {
                            node.edges[k] = null; //no neighbor in that direction
                        }
                    }
                }
            }
            defaultStart = NodeDict[new(rows / 2, 0)];
        }

        public static Tuple<int, int>[] NeighborCoords(int u, int v)
        {
            Tuple<int, int>[] results = new Tuple<int, int>[6];
            results[0] = new(u - 1, v);
            results[1] = new(u - 1, v + 1);
            results[2] = new(u, v - 1);
            results[3] = new(u, v + 1);
            results[4] = new(u + 1, v - 1);
            results[5] = new(u + 1, v);
            return results;
        }
    }
}
