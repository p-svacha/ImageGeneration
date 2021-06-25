using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Geometry;

public class MRule_Ring_Circles : MandalaLanguageRule
{
    // Rule attributes
    private const int MinCircles = 4;

    // Dynamic values set in Initialize
    private int NumCircles;
    private bool Alternating;
    private int RingId1;
    private int RingId2;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Ring source = (ME_Ring)sourceElement;

        // Get Circle radius
        float circleRadius = source.Width / 2;

        // Create SvgElement and add it to SvgDocument
        List<MandalaElement> elements = new List<MandalaElement>();

        float angleStep = 360f / NumCircles;
        for(int i = 0; i < NumCircles; i++)
        {
            float angle = i * angleStep;
            float centerX = source.Center.X + (float)((source.InnerRadius + source.Width / 2) * Math.Sin(DegreeToRadian(angle)));
            float centerY = source.Center.Y + (float)((source.InnerRadius + source.Width / 2) * Math.Cos(DegreeToRadian(angle)));
            PointF center = new PointF(centerX, centerY);

            DrawCircle(source.SvgDocument, center, circleRadius);

            elements.Add(new ME_Circle(source.SvgDocument, source.Depth + 1, (Alternating && i % 2 == 1) ? RingId2 : RingId1, center, circleRadius, angle, false));
        }

        return elements;
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Ring source = (ME_Ring)sourceElement;

        // # circles
        int maxCircles = GetMaxNumCircles(source);
        
        if (maxCircles < MinCircles) NumCircles = 0; // Don't draw any circles if minimum is not possible
        else
        {
            if (random.Next(2) == 1) NumCircles = maxCircles;
            else
            {
                NumCircles = random.Next(maxCircles - MinCircles) + MinCircles;
            }
        }

        // Creating element ids
        Alternating = random.Next(2) == 1;
        if (NumCircles % 2 == 1) Alternating = false;
        RingId1 = MandalaElement.ElementId++;
        RingId2 = MandalaElement.ElementId++; // only used for alternating
    }

    private int GetMaxNumCircles(ME_Ring source)
    {
        // Get center point of first circle
        float centerX1 = source.Center.X + (float)((source.InnerRadius + source.Width / 2) * Math.Sin(DegreeToRadian(0)));
        float centerY1 = source.Center.Y + (float)((source.InnerRadius + source.Width / 2) * Math.Cos(DegreeToRadian(0)));
        PointF center1 = new PointF(centerX1, centerY1);
        float radius = source.Width / 2;

        // Get intersections of "tangent circle of first circle" (circle that has all the points which would tangent the first circle as a center with the same radius)
        // with the "radius circle" (middle between inner and outer radius of ring)
        List<PointF> intersections = FindCircleCircleIntersections(center1, radius*2, source.Center, source.InnerRadius + source.Width / 2);

        // Return 1 if only 1 circle possible
        if (intersections.Count == 0) return 1;

        // Keep making circles in one directions until it touches first circle
        int count = 0;
        while(count == 0 || (intersections.Count == 2  && FindCircleCircleIntersections(intersections[0], radius, center1, radius).Count == 0))
        {
            intersections = FindCircleCircleIntersections(intersections[0], radius*2, source.Center, source.InnerRadius + source.Width / 2);
            count++;
        }

        return count + 1;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        if (sourceElement.Type != MandalaElementType.Ring) return false;
        else return true; // ?
        ME_Ring source = (ME_Ring)sourceElement;
        // Ring has to be thinner than innerradius
        return source.Width < source.InnerRadius;
    }
}
