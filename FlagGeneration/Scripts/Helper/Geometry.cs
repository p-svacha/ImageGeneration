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
    }
}
