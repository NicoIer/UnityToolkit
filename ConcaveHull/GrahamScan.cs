using System;
using System.Collections.Generic;

namespace ConcaveHull
{
    public static class GrahamScan
    {
        const int TURN_LEFT = 1;
        const int TURN_RIGHT = -1;
        const int TURN_NONE = 0;

        public static int Turn(Node p, Node q, Node r)
        {
            return ((q.x - p.x) * (r.y - p.y) - (r.x - p.x) * (q.y - p.y)).CompareTo(0);
        }

        public static void KeepLeft(List<Node> hull, Node r)
        {
            while (hull.Count > 1 && Turn(hull[hull.Count - 2], hull[hull.Count - 1], r) != TURN_LEFT)
            {
                hull.RemoveAt(hull.Count - 1);
            }

            if (hull.Count == 0 || hull[hull.Count - 1] != r)
            {
                hull.Add(r);
            }
        }

        public static double GetAngle(Node p1, Node p2)
        {
            double xDiff = p2.x - p1.x;
            double yDiff = p2.y - p1.y;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }

        public static List<Node> MergeSort(Node p0, List<Node> arrPoint)
        {
            if (arrPoint.Count == 1)
            {
                return arrPoint;
            }

            List<Node> arrSortedInt = new List<Node>();
            int middle = arrPoint.Count / 2;
            List<Node> leftArray = arrPoint.GetRange(0, middle);
            List<Node> rightArray = arrPoint.GetRange(middle, arrPoint.Count - middle);
            leftArray = MergeSort(p0, leftArray);
            rightArray = MergeSort(p0, rightArray);
            int leftptr = 0;
            int rightptr = 0;
            for (int i = 0; i < leftArray.Count + rightArray.Count; i++)
            {
                if (leftptr == leftArray.Count)
                {
                    arrSortedInt.Add(rightArray[rightptr]);
                    rightptr++;
                }
                else if (rightptr == rightArray.Count)
                {
                    arrSortedInt.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else if (GetAngle(p0, leftArray[leftptr]) < GetAngle(p0, rightArray[rightptr]))
                {
                    arrSortedInt.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else
                {
                    arrSortedInt.Add(rightArray[rightptr]);
                    rightptr++;
                }
            }

            return arrSortedInt;
        }

        public static List<Node> ConvexHull(List<Node> points)
        {
            Node p0 = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                if (p0.y > points[i].y)
                {
                    p0 = points[i];
                }
            }

            List<Node> order = new List<Node>();
            foreach (Node value in points)
            {
                if (p0 != value)
                    order.Add(value);
            }

            order = MergeSort(p0, order);
            List<Node> result = new List<Node>();
            result.Add(p0);
            result.Add(order[0]);
            result.Add(order[1]);
            order.RemoveAt(0);
            order.RemoveAt(0);
            foreach (Node value in order)
            {
                KeepLeft(result, value);
            }

            return result;
        }
    }
}