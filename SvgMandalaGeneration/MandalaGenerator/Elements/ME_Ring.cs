using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;

public class ME_Ring : MandalaElement
{
    public PointF Center;
    public float InnerRadius;
    public float OuterRadius;
    public float Width;

    public ME_Ring(SvgDocument svgDocument, int depth, int elementId, PointF center, float innerRadius, float outerRadius) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.Ring;
        Center = center;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        Width = outerRadius - innerRadius;

        Initialize();
    }

    protected override float CalculateArea()
    {
        float outerCircleArea = (float)(OuterRadius * OuterRadius * Math.PI);
        float innerCircleArea = (float)(InnerRadius * InnerRadius * Math.PI);
        return outerCircleArea - innerCircleArea;
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