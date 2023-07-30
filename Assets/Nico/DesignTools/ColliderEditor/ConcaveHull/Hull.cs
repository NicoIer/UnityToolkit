using System.Collections.Generic;
using System.Linq;

namespace ConcaveHull
{
    public class Hull
    {
        public readonly HashSet<Node> unUsed;
        public readonly List<Line> convexLines;
        public List<Line> concaveLines;


        public Hull(List<Node> nodes, double concavity, int scaleFactor)
        {
            unUsed = new HashSet<Node>(nodes);
            convexLines = new List<Line>();
            concaveLines = new List<Line>();


            convexLines.AddRange(MakeConvexHull(nodes));
            foreach (Line line in convexLines)
            {
                unUsed.Remove(line.start);
                unUsed.Remove(line.end);
            }

            MakeConvexHull(nodes);
            MakeConcaveHull(concavity, scaleFactor);
        }

        public List<Line> MakeConvexHull(List<Node> nodes)
        {
            List<Node> convexH = GrahamScan.ConvexHull(nodes);
            List<Line> exitLines = new List<Line>();

            for (int i = 0; i < convexH.Count - 1; i++)
            {
                exitLines.Add(new Line(convexH[i], convexH[i + 1]));
            }

            exitLines.Add(new Line(convexH[0], convexH[convexH.Count - 1]));
            return exitLines;
        }

        public void MakeConcaveHull(double concavity, int scaleFactor)
        {
            /* Run setConvHull before! 
             * Concavity is a value used to restrict the concave angles 
             * It can go from -1 (no concavity) to 1 (extreme concavity) 
             * Avoid concavity == 1 if you don't want 0º angles
             * */
            bool aLineWasDividedInTheIteration;
            concaveLines.AddRange(convexLines);
            do
            {
                aLineWasDividedInTheIteration = false;
                for (int linePositionInHull = 0;
                     linePositionInHull < concaveLines.Count && !aLineWasDividedInTheIteration;
                     linePositionInHull++)
                {
                    Line line = concaveLines[linePositionInHull];
                    List<Node> nearbyPoints = HullFunctions.GetNearbyPoints(line, unUsed, scaleFactor);
                    List<Line> dividedLine =
                        HullFunctions.GetDividedLine(line, nearbyPoints, concaveLines, concavity);
                    if (dividedLine.Count <= 0) continue;
                    
                    // Line divided!
                    aLineWasDividedInTheIteration = true;

                    unUsed.Remove(dividedLine[0].end);

                    concaveLines.AddRange(dividedLine);
                    concaveLines.RemoveAt(linePositionInHull); // Divided line no longer exists
                }

                concaveLines = concaveLines.OrderByDescending(a => a.length).ToList();
            } while (aLineWasDividedInTheIteration);
        }
    }
}