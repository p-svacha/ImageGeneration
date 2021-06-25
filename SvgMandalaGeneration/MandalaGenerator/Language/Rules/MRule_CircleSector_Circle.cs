using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;
using static Geometry;

public class MRule_CircleSector_Circle : MandalaLanguageRule
{
    // Rule attributes

    // Dynamic values set in Initialize
    private int CircleId;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_CircleSector source = (ME_CircleSector)sourceElement;

        
        float sin = (float)(Math.Sin(DegreeToRadian(source.InnerAngle / 2)));
        float radius = ((-source.InnerRadius * sin) + (source.OuterRadius * sin)) / (sin + 1);

        float circleAngle = (source.StartAngle + source.EndAngle) / 2;
        PointF tangent = FindPointOnCircle(source.Center, source.OuterRadius, circleAngle);

        List<PointF> intersect = FindLineCircleIntersections(tangent, radius, source.InnerVertex, tangent);
        PointF center = intersect[1];

        DrawCircle(source.SvgDocument, center, radius);

        ME_Circle circle = new ME_Circle(source.SvgDocument, source.Depth + 1, CircleId, center, radius, circleAngle, false);

        return new List<MandalaElement>() { circle };
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_CircleSector source = (ME_CircleSector)sourceElement;

        // Element ids
        CircleId = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.CircleSector;
    }
}
