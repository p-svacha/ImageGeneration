using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;
using static Geometry;

public class ME_CircleSector : MandalaElement
{
    public PointF Center; // The center of the circle, where the sector is cut from
    public float InnerRadius; // If 0, the sector starts at the center of the circle
    public float OuterRadius;
    public float Width; // Outer radius - inner radius
    public float StartAngle; // From center to OuterVertex1
    public float EndAngle; // From center to OuterVertex2
    public PointF InnerVertex, OuterVertex1, OuterVertex2, Tangent;
    public float InnerAngle;

    public ME_CircleSector(SvgDocument svgDocument, int depth, int elementId, PointF center, float innerRadius, float outerRadius, float startAngle, float endAngle) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.CircleSector;
        Center = center;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        Width = outerRadius - innerRadius;
        StartAngle = startAngle;
        EndAngle = endAngle;
        float innerVertexAngle = (endAngle + startAngle) / 2;
        InnerVertex = FindPointOnCircle(center, InnerRadius, innerVertexAngle);
        OuterVertex1 = FindPointOnCircle(center, outerRadius, startAngle);
        OuterVertex2 = FindPointOnCircle(center, outerRadius, endAngle);
        InnerAngle = FindAngleBetweenTwoLineSegments(InnerVertex, OuterVertex1, OuterVertex2);
        Tangent = FindPointOnCircle(center, OuterRadius, (startAngle + endAngle) / 2);

        Initialize();
    }

    protected override float CalculateArea()
    {
        // Area of segment
        PointF shIntersectionPoint = (PointF)(FindLineLineIntersection(Center, Tangent, OuterVertex1, OuterVertex2));
        float h = FindDistance(shIntersectionPoint, Tangent);
        float s = FindDistance(OuterVertex1, OuterVertex2);
        float segmentArea = (float)((OuterRadius * OuterRadius) * (Math.Asin(s / (2 * OuterRadius))) - ((s * (OuterRadius - h)) / 2));

        // Area of triangle
        float triH = FindDistance(InnerVertex, shIntersectionPoint);
        float triangleArea = 0.5f * s * triH;

        return segmentArea + triangleArea;
    }

    protected override SvgElement CreateSvgElement()
    {
        return new SvgCircle()
        {
            CenterX = 0,
            CenterY = 0,
            Radius = 0,
            Fill = new SvgColourServer(Color.Transparent),
            Stroke = new SvgColourServer(Color.Transparent)
        };
    }
}