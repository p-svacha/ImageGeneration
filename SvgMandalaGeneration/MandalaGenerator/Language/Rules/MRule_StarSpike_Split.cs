using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;
using static Geometry;

public class MRule_StarSpike_Split : MandalaLanguageRule
{
    // Rule attributes
    private const float MinDistanceInside = 0.1f;
    private const float MinDistanceOutside = 0.1f;

    // Dynamic values set in Initialize
    private float SplitRadius;
    private int SpikeId;
    private int StripeId;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_StarSpike source = (ME_StarSpike)sourceElement;

        // Find intersection points
        PointF intersection1 = FindLineCircleIntersections(source.CircleCenter, SplitRadius, source.InnerVertex1, source.OuterVertex)[0];
        PointF intersection2 = FindLineCircleIntersections(source.CircleCenter, SplitRadius, source.InnerVertex2, source.OuterVertex)[0];
        float angle = FindAngleBetweenTwoLineSegments(source.CircleCenter, intersection1, intersection2);
        float startAngle = source.Angle - angle / 2;
        float endAngle = source.Angle + angle / 2;
        DrawArc(source.SvgDocument, source.CircleCenter, SplitRadius, startAngle, endAngle);

        // Create elements
        ME_StarSpike spike = new ME_StarSpike(source.SvgDocument, source.Depth + 1, SpikeId, source.CircleCenter, SplitRadius, source.Angle, intersection1, intersection2, source.OuterVertex);
        ME_Stripe stripe = new ME_Stripe(source.SvgDocument, source.Depth + 1, StripeId, source.CircleCenter, source.CircleRadius, SplitRadius, source.InnerVertex1, source.InnerVertex2, intersection1, intersection2);

        return new List<MandalaElement>() { spike, stripe};
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_StarSpike source = (ME_StarSpike)sourceElement;

        float spikeLength = FindDistance(source.Tangent, source.OuterVertex);
        float splitRangeRel = 1 - MinDistanceInside - MinDistanceOutside;
        float splitRadiusRel = (float)random.NextDouble() * splitRangeRel + MinDistanceInside;
        SplitRadius = source.CircleRadius + spikeLength * splitRadiusRel;


        // Element ids
        SpikeId = MandalaElement.ElementId++;
        StripeId = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.StarSpike;
    }
}
