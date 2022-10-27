using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    public class PathGenerator : PathGenBase
    {
        HexGraph graph;
        int target;
        int carryOver = 25;

        List<Path> beam = new();

        public PathGenerator(int target, HexGraph graph, int carryOver)
        {
            this.graph = graph;
            this.target = target;
            this.carryOver = carryOver;
        }

        public override Path? FindPath()
        {
            Node middle = graph.defaultStart;
            beam.Add(target > 0 ? Path.BuildPosZero(middle) : Path.BuildNegZero(middle));
            Path? currentSmallest = null;
            do
            {
                ExpandBeam();
                if(currentSmallest != null)
                {
                    GridBounds b = GetPathBounds(currentSmallest);
                    beam.RemoveAll(p => SmallerBounds(b, GetPathBounds(p))); //too long to lead to a better path
                }
                TrimToBest();
                List<Path> finished = new();
                foreach (Path p in beam)
                {
                    if(PathValue(p) == target)
                    {
                        finished.Add(p);
                    }
                }
                foreach(Path p in finished)
                {
                    beam.Remove(p); //no more expanding to do on this path, we already reached the value
                    /*int currentLength = currentSmallest?.GetNodesCopy()?.Count ?? int.MaxValue;
                    if (PathLength(p) < currentLength)
                    {
                        currentSmallest = p;
                    }
                    */
                    if (currentSmallest == null || SmallerBounds(GetPathBounds(currentSmallest), GetPathBounds(p)))
                    {
                        currentSmallest = p;
                    }
                }
                
            }
            while (beam.Count != 0);
            return currentSmallest;
        }

        public bool SmallerBounds(GridBounds initial, GridBounds test)
        {
            double b1, b2;
            b1 = (initial.maxRow - initial.minRow + 1) * (initial.maxCol - initial.minCol + 1);
            b2 = (test.maxRow - test.minRow + 1) * (test.maxCol - test.minCol + 1);
            return b2 < b1;
        }

        public void ExpandBeam()
        {
            List<Path> newBeam = new();
            foreach(Path p in beam)
            {
                Node lastNode = p.GetLastNode();
                foreach(var t in HexGraph.NeighborCoords(lastNode.u, lastNode.v))
                {
                    if (!graph.NodeDict.ContainsKey(t) || graph.NodeDict[t] == null)
                    {
                        continue;
                    }
                    Edge? ne = lastNode.GetEdgeBetween(graph.NodeDict[t]);
                    if (p != null && !p.ContainsEdge(ne))
                    {
                        Node nn = ne.OppositeNode(lastNode);
                        Path? newPath = p.CopyWithNode(nn);
                        if (newPath != null && (newPath.GetLastTurn() != Turn.SHARP_RIGHT || PathValue(p) % 2 == 0))
                            newBeam.Add(newPath);
                    }
                }
            }
            beam.Clear();
            beam.AddRange(newBeam);
        }

        public void TrimToBest()
        {
            if(!Interlocked.Equals(MainWindow.limitVals, 0))
                beam.RemoveAll(p => PathValue(p) > target); //if a value goes over, it's mostly going to only go even further off track
            List<Path> beamClone1 = new(), beamClone2 = new(), beamClone3 = new(), beamClone4 = new();
            beamClone1.AddRange(beam);
            beamClone2.AddRange(beam);
            beamClone3.AddRange(beam);
            beamClone4.AddRange(beam);
            var shortQuery = beamClone1.OrderBy(PathLength).Where((a, b) => b < carryOver).ToList();
            shortQuery = shortQuery.Count > carryOver ? shortQuery.GetRange(0, carryOver) : shortQuery;
            //var boundQuery = beamClone2.Where(a => !shortQuery.Contains(a)).OrderBy(BoundedNodeCount).Where((a, b) => b < carryOver).ToList();
            //boundQuery = boundQuery.Count > carryOver ? boundQuery.GetRange(0, carryOver) : boundQuery;
            var valQuery = beamClone3.Where(a => !shortQuery.Contains(a) /* && !boundQuery.Contains(a) */).OrderBy(p => Math.Abs(PathValue(p) - target)).Where((a, b) => b < carryOver).ToList();
            valQuery = valQuery.Count > carryOver ? valQuery.GetRange(0, carryOver) : valQuery;
            var countQuery = beamClone4.Where(a => !shortQuery.Contains(a) /* && !boundQuery.Contains(a) */ && !valQuery.Contains(a)).OrderBy(UniqueNodeCount).Where((a, b) => b < carryOver).ToList();
            beam.Clear();
            beam.AddRange(shortQuery);
            //beam.AddRange(boundQuery);
            beam.AddRange(valQuery);
            beam.AddRange(countQuery);
            //all values that aren't picked by any category are removed
        }

        public static int UniqueNodeCount(Path p)
        {
            if (p.GetNodesCopy().Count == 0)
                return 0;
            List<Node> uniqueNodes = new()
            {
                p.GetLastNode() //make sure it's not empty
            };
            foreach (Node node in p.GetNodesCopy())
            {
                if(!uniqueNodes.Where(n => n.IsEqual(node)).Any()){
                    uniqueNodes.Add(node);
                }
            }
            return uniqueNodes.Count;
        }

        public int PathValue(Path p)
        {
            int val = 0;
            List<Node> nodes = p.GetNodesCopy();
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
            return val * Math.Sign(target);
        }

        public static int PathLength(Path p)
        {
            return p.GetNodesCopy().Count;
        }

        public int BoundedNodeCount(Path p)
        {
 
            List<Node> nodes = p.GetNodesCopy();//copy so our edits in this function won't affect the path
            int count = nodes.Count;
            int minRow = nodes.Select(n => n.u).Min();
            int maxRow = nodes.Select(n => n.u).Max();
            int minDiag = nodes.Select(n => n.v).Min();
            int maxDiag = nodes.Select(n => n.v).Max();
            GridBounds b = GetPathBounds(p);
            //*
            List<Node> bounds = graph.Nodes.Where(n => !nodes.Contains(n)).Where(n => OutsideBounds(n, b)).ToList();
            for(int i = minRow; i <= maxRow; i++)
            {
                for(int j = minDiag + maxRow/2 - minRow/2 - (i-minRow)/2; j < maxDiag - (i - minRow) / 2; j++)
                {
                    bool result = PathToNodes(graph.NodeDict[new(i,j)], nodes, bounds);
                    if (result)
                    {
                        bounds.Add(graph.NodeDict[new(i, j)]);
                        //There's a path out from here, no point in rechecking that later as any node that can path to here has a path out
                    }
                    else
                    {
                        count++; //node is surrounded so it counts
                        nodes.Add(graph.NodeDict[new(i, j)]);
                        //no path out from here, no point in rechecking that later as any that can path to here also can't path out
                    }
                }
            }
            //*/
            /*
            count = (maxRow - minRow + 1) * (maxDiag - minDiag + 1);
            //*/
            return count;
        }

        public static bool OutsideBounds(Node node, GridBounds bounds)
        {
            return node.u < bounds.minRow || node.u > bounds.maxRow || node.v > bounds.minCol || node.v < bounds.maxCol;
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

        /// <summary>
        /// Gets whether a path exists from the given node, around the barrier, to a target
        /// </summary>
        /// <param name="start">The node to start at</param>
        /// <param name="barrier">The nodes to go around</param>
        /// <param name="targets">The nodes to go to</param>
        public bool PathToNodes(Node start, List<Node> barrier, List<Node> targets)
        {
            bool pathFound = false;
            if(barrier.Contains(start)){
                return false;
            }
            if (targets.Contains(start))
            {
                return true;
            }
            var query = start.edges.Select(kv => kv.Value).Select(e => e?.OppositeNode(start)).Where(n => n != null).Where(n => barrier.Contains(n!));
            foreach (Node? n in query) //adjacent, not in barrier
            {
                if (n != null && n.edges.ContainsValue(null))
                    return true; //reached edge of grid on a side the pattern goes to the edge on
                if (targets.Where(node => node.IsEqualNullable(n)).Any()) //reached the targets
                    return true;
                List<Node> newBarrier = new(); //to make the path not loop back and forth
                newBarrier.AddRange(barrier);
                newBarrier.Add(n!);
                pathFound = PathToNodes(start, newBarrier, targets);
                if (pathFound)
                    return true;
            }
            return pathFound;
        }
    }

    public class GridBounds
    {
        public int minRow, maxRow;
        public double minCol, maxCol;

        public double GetArea()
        {
            return (maxCol - minCol + 1) * (maxRow - minRow + 1);
        }
    }
}
