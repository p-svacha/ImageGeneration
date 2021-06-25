using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;
using static Geometry;

public class ME_StarSpike : MandalaElement
{
    public PointF CircleCenter; // The center of the circle that defines the short side of the spike
    public float CircleRadius; // The radius of the circle that defines the short side of the spike
    public float Angle; // The direction that the spike points to
    public PointF InnerVertex1, InnerVertex2, OuterVertex, Tangent;

    public ME_StarSpike(SvgDocument svgDocument, int depth, int elementId, PointF center, float radius, float angle, PointF inner1, PointF inner2, PointF outer) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.StarSpike;
        CircleCenter = center;
        CircleRadius = radius;
        Angle = angle;
        InnerVertex1 = inner1;
        InnerVertex2 = inner2;
        OuterVertex = outer;
        Tangent = FindPointOnCircle(center, radius, angle);

        Initialize();
    }

    protected override float CalculateArea()
    {
        // Triangle Area
        float triangleArea = GetTriangeArea(InnerVertex1, InnerVertex2, OuterVertex);

        // Segment area
        float segmentArea = GetCircleSegmentArea(CircleCenter, CircleRadius, InnerVertex1, InnerVertex2, Angle);

        return triangleArea - segmentArea;
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