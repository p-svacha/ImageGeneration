using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;
using static Geometry;

public class MRule_CircleSector_Stripes : MandalaLanguageRule
{
    // Rule attributes
    private const int MinStripes = 2;
    private const int MaxStripes = 10;

    // Dynamic values set in Initialize
    private int NumStripes;
    private bool Alternating;
    private int SectorId;
    private int StripeId1;
    private int StripeId2;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_CircleSector source = (ME_CircleSector)sourceElement;

        float radiusStep = source.Width / NumStripes;

        List<MandalaElement> elements = new List<MandalaElement>();

        for(int i = 0; i < NumStripes; i++)
        {
            float radius = source.InnerRadius + (i+1) * radiusStep;
            PointF startPoint = FindLineCircleIntersections(source.Center, radius, source.InnerVertex, source.OuterVertex1)[0];
            PointF endPoint = FindLineCircleIntersections(source.Center, radius, source.InnerVertex, source.OuterVertex2)[0];
            if (i < NumStripes - 1) DrawArc(source.SvgDocument, source.Center, radius, startPoint, endPoint);

            float lastRadius = radius - radiusStep;
            PointF lastStartPoint = FindLineCircleIntersections(source.Center, lastRadius, source.InnerVertex, source.OuterVertex1)[0];
            PointF lastEndPoint = FindLineCircleIntersections(source.Center, lastRadius, source.InnerVertex, source.OuterVertex2)[0];

            // Add Stripe Elements
            if (i > 0)
            {
                int id = (Alternating && i % 2 == 1) ? StripeId2 : StripeId1;
                ME_Stripe stripe = new ME_Stripe(source.SvgDocument, source.Depth + 1, id, source.Center, lastRadius, radius, lastStartPoint, lastEndPoint, startPoint, endPoint);
                elements.Add(stripe);
            }
        }

        // Add Sector Element when only 2 stripes
        if (NumStripes == MinStripes)
        {
            float sectorRadius = source.InnerRadius + radiusStep;
            PointF outer1 = FindLineCircleIntersections(source.Center, sectorRadius, source.InnerVertex, source.OuterVertex1)[0];
            PointF outer2 = FindLineCircleIntersections(source.Center, sectorRadius, source.InnerVertex, source.OuterVertex2)[0];
            float angle = FindAngleBetweenTwoLineSegments(source.Center, outer1, outer2);
            float angleDiff = (source.EndAngle - source.StartAngle) - angle;

            float startPointAngle = source.StartAngle + angleDiff / 2;
            float endPointAngle = startPointAngle + angle;

            ME_CircleSector sector = new ME_CircleSector(source.SvgDocument, source.Depth + 1, SectorId, source.Center, source.InnerRadius, sectorRadius, startPointAngle, endPointAngle);
            elements.Add(sector);
        }

        return elements;
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_CircleSector source = (ME_CircleSector)sourceElement;

        // # Stripes
        if (random.Next(2) == 1) NumStripes = MinStripes;
        else NumStripes = random.Next(MaxStripes - MinStripes + 2) + MinStripes;

        Alternating = random.Next(2) == 1;

        // Element ids
        SectorId = MandalaElement.ElementId++;
        StripeId1 = MandalaElement.ElementId++;
        StripeId2 = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.CircleSector;
    }
}
