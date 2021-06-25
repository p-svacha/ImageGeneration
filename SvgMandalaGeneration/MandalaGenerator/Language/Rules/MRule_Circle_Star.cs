using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using static Geometry;

public class MRule_Circle_Star : MandalaLanguageRule
{
    // Rule attributes
    private const int MinCorners = 3;
    private const int MaxCorners = 16;
    private const float MinInnerRadius = 0.2f;
    // Max inner radius is set that the star spikes must go outside (the shape has to be concave)

    // Dynamic values set in Initialize
    private int NumCorners;
    private float InnerRadius;
    private int StarId;
    private int SectorId;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Circle source = (ME_Circle)sourceElement;

        // Draw Star 
        float angle;

        if (source.Centered) angle = 180;
        else angle = source.AngleFromCenter;
        PointF[] vertices = DrawStar(source.SvgDocument, source.Center, InnerRadius, source.Radius, NumCorners, angle);

        List<MandalaElement> elements = new List<MandalaElement>();
        // Create Star Element
        ME_Star star = new ME_Star(source.SvgDocument, source.Depth + 1, StarId, source.Center, InnerRadius, source.Radius, NumCorners, angle, source.Centered, vertices);
        elements.Add(star);

        // Create Sectors
        float angleStep = 360f / NumCorners;
        for(int i = 0; i < NumCorners; i++)
        {
            float startAngle = angle + i * angleStep;
            float endAngle = startAngle + angleStep;
            ME_CircleSector sector = new ME_CircleSector(source.SvgDocument, source.Depth + 1, SectorId, source.Center, InnerRadius, source.Radius, startAngle, endAngle);
            elements.Add(sector);
        }

        return elements;
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Circle source = (ME_Circle)sourceElement;

        // Get # corners
        NumCorners = random.Next(MaxCorners - MinCorners + 1) + MinCorners;

        // Get Radius (keep doing until shape is concave)
        float innerAngle = 361;
        while (innerAngle > 180)
        {
            float minInnerRadius = source.Radius * MinInnerRadius;
            float maxInnerRadius = source.Radius;
            InnerRadius = (float)random.NextDouble() * (maxInnerRadius - minInnerRadius) + minInnerRadius;

            float angleStep = 360f / (NumCorners * 2);
            PointF outer1 = FindPointOnCircle(source.Center, source.Radius, 0);
            PointF inner = FindPointOnCircle(source.Center, InnerRadius, angleStep);
            PointF outer2 = FindPointOnCircle(source.Center, source.Radius, angleStep * 2);

            innerAngle = FindAngleBetweenTwoLineSegments(inner, outer1, outer2);
        }

        // Element ids
        StarId = MandalaElement.ElementId++;
        SectorId = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Circle;
    }
}
