using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hex_Casting_Generator.Graphs
{
    public class Path
    {
        private List<Node> Nodes;
        private List<Edge> Edges;

        public Path()
        {
            Nodes = new();
            Edges = new();
        }

        private Path(Node start)
        {
            Edges = new();
            Nodes = new()
            {
                start
            };
        }

        private Path(List<Node> nodes, List<Edge> edges)
        {
            Nodes = nodes;
            Edges = edges;
        }

        public Path Copy()
        {
            List<Node> newNodes = new();
            List<Edge> newEdges = new();
            newNodes.AddRange(Nodes);
            newEdges.AddRange(Edges);
            Path p = new()
            {
                Edges = newEdges,
                Nodes = newNodes
            };
            return p;
        }

        public Path? CopyWithNode(Node n)
        {
            List<Node> newNodes = new();
            List<Edge> newEdges = new();
            newNodes.AddRange(Nodes);
            newEdges.AddRange(Edges);
            Edge? e = Nodes[^1].edges.Select(kv => kv.Value).FirstOrDefault(e => e?.IsIncident(n) ?? false, null);
            if (e == null)
            {
                Console.WriteLine("Error: copyWithNode called with non-adjacent node");
                return null;
            }
            newEdges.Add(e);
            newNodes.Add(n);
            Path p = new(newNodes, newEdges);
            return p;
        }

        public int PathLength()
        {
            return Nodes.Count;
        }

        public Turn GetLastTurn()
        {
            Direction dir1, dir2;
            dir1 = GetDirection(Nodes[^3], Nodes[^2]);
            dir2 = GetDirection(Nodes[^2], Nodes[^1]);
            return GetTurn(dir1, dir2);
        }

        public bool ContainsEdge(Edge e)
        {
            return Edges.Where(o => o.IsEqual(e)).Any();
        }

        public Node GetLastNode()
        {
            return Nodes.Last();
        }

        public List<Node> GetNodesCopy()
        {
            List<Node> nodes = new();
            nodes.AddRange(Nodes);
            return nodes;
        }

        public List<Edge> GetEdgesCopy()
        {
            List<Edge> edges = new();
            edges.AddRange(Edges);
            return edges;
        }

        public bool ContainsNode(Node n)
        {
            return Nodes.Where(o => o.IsEqual(n)).Any();
        }

        public static Path BuildPosZero(Node start)
        {
            List<Node> nodes = new();
            List<Edge> edges = new();
            nodes.Add(start);
            edges.Add(start.edges[5]!);
            Node node2 = start.edges[5]!.OppositeNode(start);
            nodes.Add(node2);
            edges.Add(node2.edges[1]!);
            Node node3 = node2.edges[1]!.OppositeNode(node2);
            nodes.Add(node3);
            edges.Add(node3.edges[0]!);
            Node node4 = node3.edges[0]!.OppositeNode(node3);
            nodes.Add(node4);
            edges.Add(node4.edges[4]!);
            nodes.Add(start);
            edges.Add(start.edges[3]!);
            nodes.Add(node3);
            return new()
            {
                Edges = edges,
                Nodes = nodes
            };
        }

        public static Path BuildNegZero(Node start)
        {
            List<Node> nodes = new();
            List<Edge> edges = new();
            nodes.Add(start);
            edges.Add(start.edges[1]!);
            Node node2 = start.edges[1]!.OppositeNode(start);
            nodes.Add(node2);
            edges.Add(node2.edges[5]!);
            Node node3 = node2.edges[5]!.OppositeNode(node2);
            nodes.Add(node3);
            edges.Add(node3.edges[4]!);
            Node node4 = node3.edges[4]!.OppositeNode(node3);
            nodes.Add(node4);
            edges.Add(node4.edges[0]!);
            nodes.Add(start);
            edges.Add(start.edges[3]!);
            nodes.Add(node3);
            return new()
            {
                Edges = edges,
                Nodes = nodes
            };
        }

        public static Direction GetDirection(Node from, Node to)
        {
            int dirInt = from.edges.Where(e => e.Value?.IsIncident(to) ?? false).Select(a => a.Key).FirstOrDefault(-1);
            return dirInt switch
            {
                0 => Direction.NORTH_WEST,
                1 => Direction.NORTH_EAST,
                2 => Direction.WEST,
                3 => Direction.EAST,
                4 => Direction.SOUTH_WEST,
                5 => Direction.SOUTH_EAST,
                _ => Direction.NONE,
            };
        }

        public static Turn GetTurn(Direction dirIn, Direction dirOut)
        {
            return dirIn switch
            {
                Direction.NORTH_WEST => dirOut switch
                {
                    Direction.NORTH_WEST => Turn.STRAIGHT,
                    Direction.NORTH_EAST => Turn.RIGHT,
                    Direction.WEST => Turn.LEFT,
                    Direction.EAST => Turn.SHARP_RIGHT,
                    Direction.SOUTH_WEST => Turn.SHARP_LEFT,
                    Direction.SOUTH_EAST => Turn.REVERSE,
                    _ => Turn.NONE,
                },
                Direction.NORTH_EAST => dirOut switch
                {
                    Direction.NORTH_WEST => Turn.LEFT,
                    Direction.NORTH_EAST => Turn.STRAIGHT,
                    Direction.WEST => Turn.SHARP_LEFT,
                    Direction.EAST => Turn.RIGHT,
                    Direction.SOUTH_WEST => Turn.REVERSE,
                    Direction.SOUTH_EAST => Turn.SHARP_RIGHT,
                    _ => Turn.NONE,
                },
                Direction.WEST => dirOut switch
                {
                    Direction.NORTH_WEST => Turn.RIGHT,
                    Direction.NORTH_EAST => Turn.SHARP_RIGHT,
                    Direction.WEST => Turn.STRAIGHT,
                    Direction.EAST => Turn.REVERSE,
                    Direction.SOUTH_WEST => Turn.LEFT,
                    Direction.SOUTH_EAST => Turn.SHARP_LEFT,
                    _ => Turn.NONE,
                },
                Direction.EAST => dirOut switch
                {
                    Direction.NORTH_WEST => Turn.SHARP_LEFT,
                    Direction.NORTH_EAST => Turn.LEFT,
                    Direction.WEST => Turn.REVERSE,
                    Direction.EAST => Turn.STRAIGHT,
                    Direction.SOUTH_WEST => Turn.SHARP_RIGHT,
                    Direction.SOUTH_EAST => Turn.RIGHT,
                    _ => Turn.NONE,
                },
                Direction.SOUTH_WEST => dirOut switch
                {
                    Direction.NORTH_WEST => Turn.SHARP_RIGHT,
                    Direction.NORTH_EAST => Turn.REVERSE,
                    Direction.WEST => Turn.RIGHT,
                    Direction.EAST => Turn.SHARP_LEFT,
                    Direction.SOUTH_WEST => Turn.STRAIGHT,
                    Direction.SOUTH_EAST => Turn.LEFT,
                    _ => Turn.NONE,
                },
                Direction.SOUTH_EAST => dirOut switch
                {
                    Direction.NORTH_WEST => Turn.REVERSE,
                    Direction.NORTH_EAST => Turn.SHARP_LEFT,
                    Direction.WEST => Turn.SHARP_RIGHT,
                    Direction.EAST => Turn.LEFT,
                    Direction.SOUTH_WEST => Turn.RIGHT,
                    Direction.SOUTH_EAST => Turn.STRAIGHT,
                    _ => Turn.NONE,
                },
                _ => Turn.NONE,
            };
        }

        public override string ToString()
        {
            String s = "";
            for (int i = 1; i < Nodes.Count - 1; i++)
            {
                Direction dirIn, dirOut;
                dirIn = GetDirection(Nodes[i-1], Nodes[i]);
                dirOut = GetDirection(Nodes[i], Nodes[i+1]);
                Turn turn = GetTurn(dirIn, dirOut);
                s += turn switch
                {
                    Turn.SHARP_LEFT => "a",
                    Turn.LEFT => "q",
                    Turn.STRAIGHT => "w",
                    Turn.RIGHT => "e",
                    Turn.SHARP_RIGHT => "d",
                    Turn.REVERSE => "s",
                    _ => "ERROR",
                };
            }
            Direction firstDir = GetDirection(Nodes[0], Nodes[1]);
            s = firstDir.ToString() + " " + s;
            return s;
        }

        static Regex withDir = new(@"((?:NORTH_|SOUTH_)?EAST|(?:NORTH_|SOUTH_)?WEST) ([asdqwe]*)");
        static Regex withoutDir = new(@"[asdqwe]*");

        public static bool IsValidPathString(string str)
        {
            return withDir.IsMatch(str) || withoutDir.IsMatch(str);
        }

        public static Path? FromString(string str, HexGraph graph)
        {
            if (!withDir.IsMatch(str) && withoutDir.IsMatch(str))
                str = "EAST " + withoutDir.Match(str).Value;
            //now we have a direction for sure
            Match m = withDir.Match(str);
            Direction start = DirFromString(m.Groups[1].Value);
            Turn[] turns = TurnsFromString(m.Groups[2].Value);
            Direction dir = start;
            Node node = graph.defaultStart;
            Path result = new(graph.defaultStart);
            if(turns.Length == 0)
            {
                Node? next = node.edges[(int)dir]?.OppositeNode(node);
                if (next == null)
                    return result;
                result = result.CopyWithNode(next!) ?? result;
            }
            else
            {
                foreach (Turn turn in turns)
                {
                    Node? next = node.edges[(int)dir]?.OppositeNode(node);
                    if (next == null)
                        break;
                    result = result.CopyWithNode(next) ?? result; //will not be null, just doing this because CopyWithNode returns a Path? in case the node isn't adjacent
                    dir = DirAfterTurn(dir, turn);
                    node = next;
                }
                Node? last = node.edges[(int)dir]?.OppositeNode(node);
                if(last != null)
                    result = result.CopyWithNode(last) ?? result;
            }
            return result;
        }

        private static Direction DirAfterTurn(Direction before, Turn turn)
        {
            return turn switch
            {
                Turn.STRAIGHT => before,
                Turn.REVERSE => DirAfterTurn(DirAfterTurn(before, Turn.SHARP_LEFT), Turn.LEFT),
                Turn.LEFT => before switch
                {
                    Direction.NORTH_WEST => Direction.WEST,
                    Direction.NORTH_EAST => Direction.NORTH_WEST,
                    Direction.WEST => Direction.SOUTH_WEST,
                    Direction.EAST => Direction.NORTH_EAST,
                    Direction.SOUTH_WEST => Direction.SOUTH_EAST,
                    Direction.SOUTH_EAST => Direction.EAST,
                    _ => Direction.NONE,
                },
                Turn.RIGHT => before switch
                {
                    Direction.NORTH_WEST => Direction.NORTH_EAST,
                    Direction.NORTH_EAST => Direction.EAST,
                    Direction.WEST => Direction.NORTH_WEST,
                    Direction.EAST => Direction.SOUTH_EAST,
                    Direction.SOUTH_WEST => Direction.WEST,
                    Direction.SOUTH_EAST => Direction.SOUTH_WEST,
                    _ => Direction.NONE,
                },
                Turn.SHARP_LEFT => DirAfterTurn(DirAfterTurn(before, Turn.LEFT), Turn.LEFT),
                Turn.SHARP_RIGHT => DirAfterTurn(DirAfterTurn(before, Turn.RIGHT), Turn.RIGHT),
                _ => Direction.NONE,
            };
        }

        private static Direction DirFromString(string str)
        {
            switch (str)
            {
                case "EAST":
                    return Direction.EAST;
                case "WEST":
                    return Direction.WEST;
                case "NORTH_EAST":
                    return Direction.NORTH_EAST;
                case "NORTH_WEST":
                    return Direction.NORTH_WEST;
                case "SOUTH_EAST":
                    return Direction.SOUTH_EAST;
                case "SOUTH_WEST":
                    return Direction.SOUTH_WEST;
                default:
                    return Direction.NONE;
            }
        }

        private static Turn[] TurnsFromString(string str)
        {
            Turn[] turns = new Turn[str.Length];
            for(int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case 'q':
                        turns[i] = Turn.LEFT;
                        break;
                    case 'w':
                        turns[i] = Turn.STRAIGHT;
                        break;
                    case 'e':
                        turns[i] = Turn.RIGHT;
                        break;
                    case 'a':
                        turns[i] = Turn.SHARP_LEFT;
                        break;
                    case 's':
                        turns[i] = Turn.REVERSE;
                        break;
                    case 'd':
                        turns[i] = Turn.SHARP_RIGHT;
                        break;
                    default:
                        break;
                }
            }
            return turns;
        }
    }

    public enum Direction
    {
        NORTH_WEST, NORTH_EAST, WEST, EAST, SOUTH_WEST, SOUTH_EAST, NONE
    }

    public enum Turn
    {
        SHARP_LEFT = -2, LEFT = -1, STRAIGHT = 0, RIGHT = 1, SHARP_RIGHT = 2, REVERSE = 3, NONE = -3
    }
}
