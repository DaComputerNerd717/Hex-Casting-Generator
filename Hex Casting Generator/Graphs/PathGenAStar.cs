using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    public class PathGenAStar : PathGenBase
    {
        int target;
        HexGraph graph;
        PriorityQueue<Path, int> frontier;

        Path? smallest = null;
        //No need to track past paths; the paths are always expanding, so past paths are just shorter versions of frontier paths

        public PathGenAStar(int target, HexGraph graph)
        {
            this.target = target;
            this.graph = graph;
            this.frontier = new();
            Node middle = graph.defaultStart;
            Path p = target > 0 ? Path.BuildPosZero(middle) : Path.BuildNegZero(middle);
            frontier.Enqueue(p, p.PathLength() /**/+ Heuristic(p)/**/);
        }

        public override Path? FindPath()
        {
            if (target == 0) //no expanding needed
                return frontier.Peek();
            while(frontier.Count > 0)
            {
                bool found = UpdateFrontier();
                if (found)
                {
                    //clear all paths which are larger or longer
                    foreach (Path toCompare in frontier.UnorderedItems.Select(p => p.Element).Where(p => (PathValue(p) == target)).ToList())
                    {
                        if (smallest == null || GetPathBounds(smallest).GetArea() > GetPathBounds(toCompare).GetArea())
                        {
                            smallest = toCompare;
                            List<(Path, int)> toKeep = frontier.UnorderedItems.Where(t => GetPathBounds(t.Element).GetArea() < GetPathBounds(smallest).GetArea()).ToList();
                            frontier.Clear();
                            frontier.EnqueueRange(toKeep);
                        }
                    }
                    //if(target < 10) //the heuristic is only consistent if the target's greater than 10, I think.  
                      //  return smallest; //one of our first finds will be smallest, I believe. 
                }
            }
            return smallest;
        }

        public bool UpdateFrontier()
        {
            bool flag = false;
            Path nextPath = frontier.Dequeue();
            foreach(var p in Expand(nextPath))
            {
                if (PathValue(p) == target)
                    flag = true;
                int g = p.PathLength();
                int h = Heuristic(p);
                Console.WriteLine(p.ToString() + " " + h);
                frontier.Enqueue(p, g/**/ + h/**/);
            }
            return flag;
        }

        public List<Path> Expand(Path p)
        {
            List<Path> paths = new();
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
                        if (smallest != null && GetPathBounds(newPath).GetArea() > GetPathBounds(smallest).GetArea())
                        { //we have a better path already
                            Debug.WriteLine("Path " + p.ToString() + " worse than " + smallest.ToString());
                            continue;
                        }
                        paths.Add(newPath);
                    } 
                        
                }
            }
            if(smallest != null)
                paths.RemoveAll(p => GetPathBounds(p).GetArea() > GetPathBounds(smallest).GetArea());
            return paths;
        }

        /// <summary>
        /// Gives an estimate of the length remaining on the optimal path. Used for the A* algorithm
        /// </summary>
        /// <param name="p">The path to work with</param>
        /// <returns>The result of the heuristic function</returns>
        public int Heuristic(Path p)
        {
            int val = Math.Abs(PathValue(p));
            int atarget = Math.Abs(target);
            int lenEst = 0;
            if (val == 0)
            {
                lenEst++;
                if (atarget > 10)
                    val += 10;
                else if (atarget > 5)
                    val += 5;
                else
                    val++;
            }
            while (val > atarget)
            {
                val /= 2;
                lenEst++; //adding d
            }
            
            while(atarget / 2 > val)
            {
                atarget /= 2;
                lenEst++;
            }


            return lenEst;
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

        public static GridBounds GetPathBounds(Path p)
        {
            double minCol = double.MaxValue, maxCol = double.MinValue;
            int minRow = int.MaxValue, maxRow = int.MinValue;
            foreach (Node n in p.GetNodesCopy())
            {
                int row = n.u;
                double col = n.v + 0.5 * n.u;
                minCol = Math.Min(minCol, col);
                minRow = Math.Min(minRow, row);
                maxCol = Math.Max(maxCol, col);
                maxRow = Math.Max(maxRow, row);
            }
            return new GridBounds
            {
                minRow = minRow,
                minCol = minCol,
                maxRow = maxRow,
                maxCol = maxCol
            };
        }
    }
}
