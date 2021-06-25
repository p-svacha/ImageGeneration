using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Geometry
{
    public static PointF FindPointOnCircle(PointF center, float radius, float angle)
    {
        return new PointF(
            center.X + (float)(radius * Math.Sin(DegreeToRadian(angle))),
            center.Y + (float)(radius * Math.Cos(DegreeToRadian(angle)))
            );
    }

    public static float FindAngleOfPointOnCircle(PointF center, float radius, PointF point)
    {
        return (float)(Math.Asin((point.X - center.X) / radius));
    }

    public static PointF GetLineVector(PointF p1, PointF p2)
    {
        return new PointF(p2.X - p1.X, p2.Y - p1.Y);
    }

    public static float FindDistance(PointF p1, PointF p2)
    {
        PointF vector = GetLineVector(p1, p2);
        return (float)(Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y));
    }

    public static float DegreeToRadian(float angle)
    {
        return (float)(Math.PI * angle / 180.0);
    }

    public static float RadianToDegree(float angle)
    {
        return (float)(angle * (180.0 / Math.PI));
    }

    // Find the points where the two circles intersect.
    public static List<PointF> FindCircleCircleIntersections(
        PointF center0, float radius0, // center and radius of point 1
        PointF center1, float radius1) // center and radius of point 2
    {
        // Find the distance between the centers.
        float dx = center0.X - center1.X;
        float dy = center0.Y - center1.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            return new List<PointF>();
        }
        else if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            return new List<PointF>();
        }
        else if ((dist == 0) && (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            return new List<PointF>();
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
            PointF intersection1 = new PointF(
                (float)(cx2 + h * (center1.Y - center0.Y) / dist),
                (float)(cy2 - h * (center1.X - center0.X) / dist));
            PointF intersection2 = new PointF(
                (float)(cx2 - h * (center1.Y - center0.Y) / dist),
                (float)(cy2 + h * (center1.X - center0.X) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1)
            {
                return new List<PointF>() { intersection1 };
            }
            return new List<PointF>() { intersection1, intersection2 };
        }
    }

    // Find the points of intersection.
    public static List<PointF> FindLineCircleIntersections(
        PointF center, float radius,
        PointF point1, PointF point2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.X - point1.X;
        dy = point2.Y - point1.Y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.X - center.X) + dy * (point1.Y - center.Y));
        C = (point1.X - center.X) * (point1.X - center.X) + (point1.Y - center.Y) * (point1.Y - center.Y) - radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            return new List<PointF>();
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            PointF intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
            return new List<PointF>() { intersection1 };
        }
        else
        {
            // Two solutions.
            t = (float)((-B + Math.Sqrt(det)) / (2 * A));
            PointF intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
            t = (float)((-B - Math.Sqrt(det)) / (2 * A));
            PointF intersection2 = new PointF(point1.X + t * dx, point1.Y + t * dy);
            return new List<PointF>() { intersection1, intersection2 };
        }
    }

    public static PointF? FindLineLineIntersection(
    PointF p1, PointF p2, PointF p3, PointF p4)
    {
        bool lines_intersect; // true if not parallel
        bool segments_intersect; // true if segments intersect

        // Get the segments' parameters.
        float dx12 = p2.X - p1.X;
        float dy12 = p2.Y - p1.Y;
        float dx34 = p4.X - p3.X;
        float dy34 = p4.Y - p3.Y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                / denominator;
        if (float.IsInfinity(t1))
        {
            // The lines are parallel (or close enough to it).
            lines_intersect = false;
            segments_intersect = false;
            return null;
        }
        lines_intersect = true;

        float t2 =
            ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                / -denominator;

        // Find the point of intersection.
        PointF intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect =
            ((t1 >= 0) && (t1 <= 1) &&
             (t2 >= 0) && (t2 <= 1));

        if (!segments_intersect) return null;
        else return intersection;
    }

    public static float Dot(PointF a, PointF b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public static float Cross(PointF a, PointF b)
    {
        return a.X * b.Y - b.X * a.Y;
    }

    public static float FindAngleBetweenTwoLineSegments(PointF startPoint, PointF endPoint1, PointF endPoint2)
    {
        PointF line1 = new PointF(startPoint.X - endPoint1.X, startPoint.Y - endPoint1.Y);
        PointF line2 = new PointF(startPoint.X - endPoint2.X, startPoint.Y - endPoint2.Y);
        float dot = Dot(line1, line2);
        float mag1 = (float)(Math.Sqrt(Dot(line1, line1)));
        float mag2 = (float)(Math.Sqrt(Dot(line2, line2)));
        float cos_ = dot / mag1 / mag2;
        float angle = (float)(Math.Acos(dot / mag2 / mag1));
        float crossProduct = Cross(line1, line2); // if positive, angle > 180
        float angleDeg = RadianToDegree(angle);
        if (crossProduct > 0) return 180 + (180 - angleDeg);
        else return angleDeg;
    }

    public static float GetTriangeArea(PointF a, PointF b, PointF c)
    {
        return ((a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y))) / 2;
    }

    public static float GetCircleSegmentArea(PointF center, float radius, PointF startPoint, PointF endPoint, float angle)
    {
        PointF tangent = FindPointOnCircle(center, radius, angle);
        PointF shIntersectionPoint = (PointF)(FindLineLineIntersection(center, tangent, startPoint, endPoint));
        float h = FindDistance(shIntersectionPoint, tangent);
        float s = FindDistance(startPoint, endPoint);
        return (float)((radius * radius) * (Math.Asin(s / (2 * radius))) - ((s * (radius - h)) / 2));
    }
}
