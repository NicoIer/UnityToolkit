using System;

namespace ConcaveHull
{
    [Serializable]
    public struct Node : IEquatable<Node>
    {
        public override bool Equals(object obj)
        {
            return obj is Node other && Equals(other);
        }

        public double x;

        public double y;

        public Node(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Node n1, Node n2)
        {
            return n1.Equals(n2);
        }

        public static bool operator !=(Node n1, Node n2)
        {
            return !(n1 == n2);
        }

        public bool Equals(Node other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }
        

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}