using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace ConvexHullPlugin
{
    public class ConvexHullPluginCommand : Command
    {
        public ConvexHullPluginCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static ConvexHullPluginCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "ConvexHull"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Select points
            if(!SelectPoints(out List<Point3d> inputPoints))
            {
                RhinoApp.WriteLine("Error in point selection");
                return Result.Failure;
            }

            // Check for errors
            if(!CheckErrors(inputPoints))
            {
                RhinoApp.WriteLine("Error in point cloud");
                return Result.Failure;
            }

            // Calculate the convex hull
            Polyline convexHull = ConvexHull.CalculateCH(inputPoints);
            // Draw convex hull line
            doc.Objects.AddPolyline(convexHull);
            doc.Views.Redraw();
            return Result.Success;
        }

        private bool CheckErrors(List<Point3d> points)
        {
            if (points.Count < 3)
            {
                RhinoApp.WriteLine("Convex hull needs at least 3 points");
                return false;
            }
            Point3d[] noDuplicates = Point3d.CullDuplicates(points, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            points = new List<Point3d>(noDuplicates);
            return true;

        }

        private bool SelectPoints(out List<Point3d> inputPoints)
        {
            var go = new GetObject();

            go.GeometryFilter = Rhino.DocObjects.ObjectType.Point;
            go.GetMultiple(1, 0);

            if (go.CommandResult() != Result.Success)
            {
                inputPoints = null;
                return false;
            }

            inputPoints = new List<Point3d>(go.ObjectCount);

            for (int i = 0; i < go.ObjectCount; i++)
            {
                inputPoints.Add(go.Object(i).Point().Location);
            }
            return true;
        }
    }
}
