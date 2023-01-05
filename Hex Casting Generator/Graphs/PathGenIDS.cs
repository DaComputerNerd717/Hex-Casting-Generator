using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    internal class PathGenIDS : PathGenBase
    {
        int target;
        HexGraph graph;
        public PathGenIDS(int target, HexGraph graph)
        {
            this.graph = graph;
            this.target = target;
        }

        public override Path? FindPath()
        {
            Path initial = target > 0 ? Path.BuildPosZero(graph.defaultStart) : Path.BuildNegZero(graph.defaultStart);
            if (target == 0)
                return Path.BuildPosZero(graph.defaultStart); //don't want to risk any edge cases
            for(int depth = 0; ; depth++)
            {
                Path? p = DLS(initial, depth);
                if (p != null)
                    return p;
            }
        }

        public Path? DLS(Path p, int depth)
        {
            Console.WriteLine(p.ToString());
            if (depth <= 0)
                return PathValue(p) == target ? p : null;
            Node lastNode = p.GetLastNode();
            foreach (var t in HexGraph.NeighborCoords(lastNode.u, lastNode.v))
            {
                if (!graph.NodeDict.ContainsKey(t) || graph.NodeDict[t] == null)
                {
                    continue;
                }
                Edge? ne = lastNode.GetEdgeBetween(graph.NodeDict[t]);
                if (p != null && !p.ContainsEdge(ne)) //no overlap
                {
                    Node nn = ne.OppositeNode(lastNode);
                    Path? newPath = p.CopyWithNode(nn);
                    if (newPath != null && (newPath.GetLastTurn() != Turn.SHARP_RIGHT || PathValue(p) % 2 == 0)) //no fractions
                    {
                        if (PathValue(newPath) > target && Interlocked.Equals(MainWindow.limitVals, 1))
                            continue;
                        Path? result = DLS(newPath, depth - 1);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        public static int PathValue(Path p)
        {
            int val = 0;
            List<Node> nodes = p.GetNodesCopy();
            Turn firstTurn = Path.GetTurn(Path.GetDirection(nodes[0], nodes[1]), Path.GetDirection(nodes[1], nodes[2]));
            for (int i = 5; i < nodes.Count - 1; i++)
            {
                Direction dirIn, dirOut;
                dirIn = Path.GetDirection(nodes[i - 1], nodes[i]);
                dirOut = Path.GetDirection(nodes[i], nodes[i + 1]);
                Turn turn = Path.GetTurn(dirIn, dirOut);
                switch (turn)
                {
                    case Turn.SHARP_LEFT:
                        val *= 2;
                        break;
                    case Turn.LEFT:
                        val += 5;
                        break;
                    case Turn.STRAIGHT:
                        val += 1;
                        break;
                    case Turn.RIGHT:
                        val += 10;
                        break;
                    case Turn.SHARP_RIGHT:
                        val /= 2;
                        break;
                    case Turn.REVERSE:
                    default:
                        break;
                };
            }
            if (firstTurn == Turn.SHARP_LEFT) //a of aqaa
                return val;
            else
                return -val;
        }
    }
}
