using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;
using static Geometry;

public class ME_Stripe : MandalaElement
{
    public PointF Center; // The center of the circle, where the stripe is cut from
    public float InnerRadius;
    public float OuterRadius;
    public float Width; // Outer radius - inner radius
    public float InnerStartAngle, InnerEndAngle, OuterStartAngle, OuterEndAngle;
    public PointF InnerVertex1, InnerVertex2, OuterVertex1, OuterVertex2;

    public ME_Stripe(SvgDocument svgDocument, int depth, int elementId, PointF center, 
        float innerRadius, float outerRadius, 
        PointF inner1, PointF inner2, PointF outer1, PointF outer2) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.Stripe;
        Center = center;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        Width = outerRadius - innerRadius;
        InnerStartAngle = FindAngleOfPointOnCircle(center, innerRadius, inner1);
        InnerEndAngle = FindAngleOfPointOnCircle(center, innerRadius, inner2);
        OuterStartAngle = FindAngleOfPointOnCircle(center, outerRadius, outer1);
        OuterEndAngle = FindAngleOfPointOnCircle(center, outerRadius, outer2);
        InnerVertex1 = inner1;
        InnerVertex2 = inner2;
        OuterVertex1 = outer1;
        OuterVertex2 = outer2;

        Initialize();
    }

    protected override float CalculateArea()
    {
        // Get approximation of angle by taking average of outer and inner
        float outerAngle = OuterEndAngle - OuterStartAngle;
        float innerAngle = InnerEndAngle - InnerStartAngle;
        float angle = (outerAngle + innerAngle) / 2;

        // Get ring area
        float outerArea = (float)(OuterRadius * OuterRadius * Math.PI);
        float innerArea = (float)(InnerRadius * InnerRadius * Math.PI);
        float ringArea = outerArea - innerArea;

        // Divide it according to angle
        float factor = angle / 360f;

        return ringArea * factor;
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