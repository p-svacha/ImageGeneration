using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;
using static Geometry;

public class ME_Star : MandalaElement
{
    public PointF Center;
    public float InnerRadius;
    public float OuterRadius;
    public float RadiusDiff;
    public int Corners;
    public float AngleFromCenter; // Angle from center point to canvas center
    public bool Centered; // Is the center of the star the center of the canvas
    public PointF[] Vertices; // even is outer, odd is inner

    public ME_Star(SvgDocument svgDocument, int depth, int elementId, PointF center, float innerRadius, float outerRadius, int corners, float angle, bool centered, PointF[] vertices) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.Star;
        Center = center;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        RadiusDiff = outerRadius - innerRadius;
        Corners = corners;
        AngleFromCenter = angle;
        Centered = centered;
        Vertices = vertices;

        Initialize();
    }

    protected override float CalculateArea()
    {
        float sideLength = FindDistance(Vertices[1], Vertices[3]);

        // Triangle area
        PointF sideCenterPoint = (PointF)(FindLineLineIntersection(Vertices[1], Vertices[3], Center, Vertices[2]));
        float triH = FindDistance(sideCenterPoint, Vertices[2]);
        float triangleArea = 0.5f * sideLength * triH;

        // Inner polygon area
        float polygonArea = (float)((Corners * sideLength * sideLength) / (4 * Math.Tan(180f / Corners)));

        return Corners * triangleArea + polygonArea;
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