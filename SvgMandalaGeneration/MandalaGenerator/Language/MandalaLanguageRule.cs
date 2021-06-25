using Svg;
using Svg.Pathing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Geometry;

public abstract class MandalaLanguageRule
{
    /// <summary>
    /// Applies the rule on the given source element. Returns the outcome elements.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public abstract List<MandalaElement> Apply(MandalaElement sourceElement);
    public abstract void Initialize(MandalaElement sourceElement, Random random);
    public abstract bool CanApply(MandalaElement sourceElement);


    protected void DrawLine(SvgDocument source, PointF p1, PointF p2)
    {
        SvgLine svgLine = new SvgLine()
        {
            StartX = p1.X,
            StartY = p1.Y,
            EndX = p2.X,
            EndY = p2.Y,
            StrokeWidth = MandalaGenerator.StrokeWidth,
            Stroke = new SvgColourServer(Color.Black)
        };
        source.Children.Add(svgLine);
    }

    protected void DrawCircle(SvgDocument source, PointF center, float radius)
    {
        SvgCircle svgCircle = new SvgCircle()
        {
            CenterX = center.X,
            CenterY = center.Y,
            Radius = radius,
            Fill = new SvgColourServer(Color.Transparent),
            StrokeWidth = MandalaGenerator.StrokeWidth,
            Stroke = new SvgColourServer(Color.Black)
        };
        source.Children.Add(svgCircle);
    }

    protected void DrawCircle(SvgDocument source, PointF center, float radius, Color color)
    {
        SvgCircle svgCircle = new SvgCircle()
        {
            CenterX = center.X,
            CenterY = center.Y,
            Radius = radius,
            Fill = new SvgColourServer(Color.Transparent),
            StrokeWidth = MandalaGenerator.StrokeWidth,
            Stroke = new SvgColourServer(color)
        };
        source.Children.Add(svgCircle);
    }

    protected void DrawArc(SvgDocument source, PointF center, float radius, float startAngle, float endAngle)
    {
        float startX = center.X + (float)(radius * Math.Sin(DegreeToRadian(startAngle)));
        float startY = center.Y + (float)(radius * Math.Cos(DegreeToRadian(startAngle)));
        PointF startPoint = new PointF(startX, startY);

        float endX = center.X + (float)(radius * Math.Sin(DegreeToRadian(endAngle)));
        float endY = center.Y + (float)(radius * Math.Cos(DegreeToRadian(endAngle)));
        PointF endPoint = new PointF(endX, endY);

        DrawArc(source, center, radius, startPoint, endPoint, endAngle - startAngle > 180);
    }

    protected void DrawArc(SvgDocument source, PointF center, float radius, PointF startPoint, PointF endPoint, bool largeArc = false)
    {
        SvgPath svgPath = new SvgPath()
        {
            Fill = new SvgColourServer(Color.Transparent),
            StrokeWidth = MandalaGenerator.StrokeWidth,
            Stroke = new SvgColourServer(Color.Black)
        };
        svgPath.PathData = new SvgPathSegmentList();

        SvgMoveToSegment svgStartMove = new SvgMoveToSegment(startPoint);
        svgPath.PathData.Add(svgStartMove);

        SvgArcSize size = largeArc ? SvgArcSize.Large : SvgArcSize.Small;

        SvgArcSegment arc = new SvgArcSegment(startPoint, radius, radius, 0, size, SvgArcSweep.Negative, endPoint);
        svgPath.PathData.Add(arc);

        source.Children.Add(svgPath);
    }

    protected PointF[] DrawStar(SvgDocument source, PointF center, float innerRadius, float outerRadius, int numCorners, float startAngle)
    {
        int numVertices = numCorners * 2;
        PointF[] vertices = new PointF[numVertices];

        // Create vertices
        float angleStep = 360f / numVertices;
        for(int i = 0; i < numVertices; i++)
        {
            float angle = startAngle + (i * angleStep);
            bool outerCorner = i % 2 == 0;
            float radius = outerCorner ? outerRadius : innerRadius;
            float x = center.X + (float)(radius * Math.Sin(DegreeToRadian(angle)));
            float y = center.Y + (float)(radius * Math.Cos(DegreeToRadian(angle)));
            vertices[i] = new PointF(x, y);

            // Connect last vertex to this one
            if (i > 0) DrawLine(source, vertices[i - 1], vertices[i]);
        }

        // Connect last vertex to first one
        DrawLine(source, vertices[vertices.Length - 1], vertices[0]);

        return vertices;
    }
    
}