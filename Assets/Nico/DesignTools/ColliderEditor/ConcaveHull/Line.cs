using System;

namespace ConcaveHull
{
    [Serializable]
    public struct Line
    {
        public Node start;
        public Node end;
        public double length => Math.Pow(start.y - end.y, 2) + Math.Pow(start.x - end.x, 2);

        public Line(Node n1, Node n2)
        {
            start = n1;
            end = n2;
        }

        public bool Contains(Node node, double tolerance = 0.0000000001)
        {
            return Math.Abs(start.x - node.x) < tolerance && Math.Abs(start.y - node.y) < tolerance ||
                   Math.Abs(end.x - node.x) < tolerance && Math.Abs(end.y - node.y) < tolerance;
        }

        public bool Conflict(Line line)
        {
            return line.start == start ||
                   line.start == end ||
                   line.end == start ||
                   line.end == end;
        }
    }
}