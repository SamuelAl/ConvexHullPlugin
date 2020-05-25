using Rhino;
using Rhino.Geometry;
using Rhino.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvexHullPlugin
{
    class ConvexHull
    {
        internal static Polyline CalculateCH(List<Point3d> points)
        {
            FindLowest(points);
            AngleSort(points);

            return new Polyline(GrahamSort(points));
        }

        private static List<Point3d> GrahamSort(List<Point3d> points)
        {
            List<Point3d> polyLinePoints = new List<Point3d>();
            polyLinePoints.Add(points[0]);
            polyLinePoints.Add(points[1]);
            int i = 2;
            while (i < points.Count)
            {
               double det = Determinant(polyLinePoints[polyLinePoints.Count - 2],
                            polyLinePoints[polyLinePoints.Count - 1],
                            points[i]);
                if (det > -(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance))
                {
                    polyLinePoints.Add(points[i]);
                    i++;
                }
                else if (det < 0 && polyLinePoints.Count > 2)
                {
                    polyLinePoints.RemoveAt(polyLinePoints.Count - 1);
                }
            }

            polyLinePoints.Add(points[0]);

            return polyLinePoints;
            
        }

        private static double Determinant(Point3d point1, Point3d point2, Point3d point3)
        {
            double[] r1 = new double[3];
            double[] r2 = new double[3];
            double[] r3 = new double[3];

            r1[0] = r2[0] = r3[0] = 1;

            r1[1] = point1.X;
            r1[2] = point1.Y;

            r2[1] = point2.X;
            r2[2] = point2.Y;

            r3[1] = point3.X;
            r3[2] = point3.Y;

            double det = ((r2[1] * r3[2]) + (r1[1] * r2[2]) + (r3[1] * r1[2])) -
                         ((r2[1] * r1[2]) + (r3[1] * r2[2]) + (r1[1] * r3[2]));

            return det;
        }

        private static void AngleSort(List<Point3d> points)
        {
            Point3d referencePoint = points[0];
            // Sort points according to angle with point0 -> x-axis line
            for (int i = 1; i < points.Count; i++)
            {
                
                for (int j = i + 1; j < points.Count; j++ )
                {
                    double angleI = Vector3d.VectorAngle(Vector3d.XAxis, new Vector3d(points[i] - referencePoint));
                    double angleJ = Vector3d.VectorAngle(Vector3d.XAxis, new Vector3d(points[j] - referencePoint));
                    if (angleJ < angleI)
                    {
                        //Swap
                        Point3d tmp = points[i];
                        points[i] = points[j];
                        points[j] = tmp;
                    }
                    else if (Math.Abs(angleJ - angleI) < RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        double lengthI = referencePoint.DistanceTo(points[i]);
                        double lengthJ = referencePoint.DistanceTo(points[j]);

                        if (lengthJ < lengthI)
                        {
                            // Swap
                            Point3d tmp = points[i];
                            points[i] = points[j];
                            points[j] = tmp;
                        }
                    }
                }
            }
        }

        private static void FindLowest(List<Point3d> points)
        {
            int lowestPoint = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].Y < points[lowestPoint].Y ||
                    (points[i].Y == points[lowestPoint].Y && points[i].X > points[lowestPoint].X))
                {
                    lowestPoint = i;
                }
            }
            
            //swap first position
            if (lowestPoint != 0)
            {
                Point3d tmp = points[0];
                points[0] = points[lowestPoint];
                points[lowestPoint] = tmp;
            }
        }
    }
}
