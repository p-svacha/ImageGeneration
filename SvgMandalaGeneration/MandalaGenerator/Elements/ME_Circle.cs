using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;

public class ME_Circle : MandalaElement
{
    public PointF Center;
    public float Radius;
    public float AngleFromCenter; // Angle from center point to canvas center
    public bool Centered; // Is the center of the circle the center of the canvas

    public ME_Circle(SvgDocument svgDocument, int depth, int elementId, PointF center, float radius, float angle, bool centered) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.Circle;
        Center = center;
        Radius = radius;
        AngleFromCenter = angle;
        Centered = centered;

        Initialize();
    }

    protected override float CalculateArea()
    {
        return (float)(Radius * Radius * Math.PI);
    }

    protected override SvgElement CreateSvgElement()
    {
        return new SvgCircle()
        {
            CenterX = Center.X,
            CenterY = Center.Y,
            Radius = Radius,
            Fill = new SvgColourServer(Color.Transparent),
            Stroke = new SvgColourServer(Color.Transparent)
        };
    }
}