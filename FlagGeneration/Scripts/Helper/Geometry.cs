using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    static class Geometry
    {
        public static float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float RadianToDegree(float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            if (angleInDegrees == 0) return pointToRotate;

            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (float)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (float)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        /// <summary>
        /// Returns the point on a circle given the center, radius and angle
        /// </summary>
        public static Vector2 GetPointOnCircle(Vector2 center, float radius, float angle)
        {
            float x = (float)(center.X + Math.Sin(DegreeToRadian(angle)) * radius);
            float y = (float)(center.Y + Math.Cos(DegreeToRadian(angle)) * radius);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns the height of a right angled triangle, where c is the point with the right angle
        /// </summary>
        public static float GetTriangleHeight(Vector2 pointA, Vector2 pointB, Vector2 pointC)
        {
            float AC = (pointC - pointA).Length();
            float AB = (pointB - pointA).Length();
            float q = (float)(Math.Pow(AC, 2) / AB);
            float h = (float) Math.Sqrt(Math.Pow(AC, 2) - Math.Pow(q, 2));
            return h;
        }

        /// <summary>
        ///  Finds and returns all intersections between two circles, given their center and radius.
        ///  Can either be 0, 1 or 2 points.
        /// </summary>
        public static List<Vector2> FindCircleCircleIntersections(Vector2 center0, float radius0, Vector2 center1, float radius1)
        {
            // Find the distance between the centers.
            float dx = center0.X - center1.X;
            float dy = center0.Y - center1.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                return new List<Vector2>();
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                return new List<Vector2>();
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                return new List<Vector2>();
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                    radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = center0.X + a * (center1.X - center0.X) / dist;
                double cy2 = center0.Y + a * (center1.Y - center0.Y) / dist;

                // Get the points P3.
                Vector2 i1 = new Vector2(
                    (float)(cx2 + h * (center1.Y - center0.Y) / dist),
                    (float)(cy2 - h * (center1.X - center0.X) / dist));
                Vector2 i2 = new Vector2(
                    (float)(cx2 - h * (center1.Y - center0.Y) / dist),
                    (float)(cy2 + h * (center1.X - center0.X) / dist));

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return new List<Vector2>() { i1 };
                return new List<Vector2>() { i1, i2 };
            }
        }

        /// <summary>
        /// Returns the vertices that form a default 5-spiked star
        /// </summary>
        public static List<Vector2> GetStarVertices(Vector2 position, int numCorners, float outerRadius, float innerRadius, float angle)
        {
            float startAngle = angle + 180;

            int numVertices = numCorners * 2;
            Vector2[] vertices = new Vector2[numVertices];

            // Create vertices
            float angleStep = 360f / numVertices;
            for (int i = 0; i < numVertices; i++)
            {
                float curAngle = startAngle + (i * angleStep);
                bool outerCorner = i % 2 == 0;
                float radius = outerCorner ? outerRadius : innerRadius;
                float x = position.X + (float)(radius * Math.Sin(Geometry.DegreeToRadian(curAngle)));
                float y = position.Y + (float)(radius * Math.Cos(Geometry.DegreeToRadian(curAngle)));
                vertices[i] = new Vector2(x, y);
            }

            return vertices.ToList();
        }
    }
}
