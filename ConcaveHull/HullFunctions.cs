using ConcaveHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConcaveHull
{
    public static class HullFunctions
    {
        /// <summary>
        /// 返回两条连接到中点的线
        /// </summary>
        /// <param name="line">线</param>
        /// <param name="nearbyPoints">线段附近的点集</param>
        /// <param name="concaveHull">凸包线集合</param>
        /// <param name="concavity">凹度</param>
        /// <returns></returns>
        public static List<Line> GetDividedLine(Line line, List<Node> nearbyPoints, List<Line> concaveHull,
            double concavity)
        {
            List<Line> dividedLine = new List<Line>();
            List<Tuple<Node, double>> pairs = new List<Tuple<Node, double>>();

            foreach (Node middlePoint in nearbyPoints)
            {
                double cosValue = GetCos(line.start, line.end, middlePoint);

                //跳过cos值大于凹度的点
                if (!(cosValue < concavity)) continue;


                Line newLineA = new Line(line.start, middlePoint);
                Line newLineB = new Line(middlePoint, line.end);
                if (!LineCollidesWithHull(newLineA, concaveHull) && !LineCollidesWithHull(newLineB, concaveHull))
                {
                    pairs.Add(new Tuple<Node, double>(middlePoint, cosValue));
                }
            }


            if (pairs.Count <= 0) return dividedLine;


            //希望中点的 cos 值最小
            pairs = pairs.OrderBy(p => p.Item2).ToList();
            dividedLine.Add(new Line(line.start, pairs[0].Item1));
            dividedLine.Add(new Line(pairs[0].Item1, line.end));

            return dividedLine;
        }

        /// <summary>
        /// 判断线是否和凸包线相交
        /// </summary>
        /// <param name="line"></param>
        /// <param name="concaveHull"></param>
        /// <returns></returns>
        private static bool LineCollidesWithHull(Line line, List<Line> concaveHull)
        {
            foreach (Line hullLine in concaveHull)
            {
                //不能和凸包线的端点相交
                if (line.Conflict(hullLine))
                {
                    continue;
                }
                //如果相交，返回true
                if (LineIntersectionFunctions.DoIntersect(line.start, line.end, hullLine.start, hullLine.end))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// cos AOB
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        private static double GetCos(Node a, Node b, Node o)
        {
            /* Law of cosines */
            double aPow2 = Math.Pow(a.x - o.x, 2) + Math.Pow(a.y - o.y, 2);
            double bPow2 = Math.Pow(b.x - o.x, 2) + Math.Pow(b.y - o.y, 2);
            double cPow2 = Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2);
            double cos = (aPow2 + bPow2 - cPow2) / (2 * Math.Sqrt(aPow2 * bPow2));
            return Math.Round(cos, 4);
        }

        /// <summary>
        /// 找到线段的在指定缩放下的 包围盒
        /// </summary>
        /// <param name="line"></param>
        /// <param name="nodeList"></param>
        /// <param name="scaleFactor"></param>
        /// <returns></returns>
        public static List<Node> GetNearbyPoints(Line line, IEnumerable<Node> nodeList, int scaleFactor)
        {
            List<Node> nearbyPoints = new List<Node>();
            int tries = 0;

            Node[] enumerable = nodeList as Node[] ?? nodeList.ToArray();
            while (tries < 2 && nearbyPoints.Count == 0)
            {
                double[] boundary = GetBoundary(line, scaleFactor);
                foreach (Node node in enumerable)
                {
                    //Not part of the line
                    if (line.Contains(node)) continue;

                    double nodeXRelPos = Math.Floor(node.x / scaleFactor);
                    double nodeYRelPos = Math.Floor(node.y / scaleFactor);

                    //Inside the boundary
                    if (nodeXRelPos >= boundary[0] && nodeXRelPos <= boundary[2] &&
                        nodeYRelPos >= boundary[1] && nodeYRelPos <= boundary[3])
                    {
                        nearbyPoints.Add(node);
                    }
                }

                //if no points are found we increase the area
                scaleFactor = scaleFactor * 4 / 3;
                tries++;
            }

            return nearbyPoints;
        }

        private static double[] GetBoundary(Line line, int scaleFactor)
        {
            double[] boundary = new double[4];
            Node aNode = line.start;
            Node bNode = line.end;
            double minXPosition = Math.Floor(Math.Min(aNode.x, bNode.x) / scaleFactor);
            double minYPosition = Math.Floor(Math.Min(aNode.y, bNode.y) / scaleFactor);
            double maxXPosition = Math.Floor(Math.Max(aNode.x, bNode.x) / scaleFactor);
            double maxYPosition = Math.Floor(Math.Max(aNode.y, bNode.y) / scaleFactor);

            boundary[0] = minXPosition;
            boundary[1] = minYPosition;
            boundary[2] = maxXPosition;
            boundary[3] = maxYPosition;

            return boundary;
        }
    }
}