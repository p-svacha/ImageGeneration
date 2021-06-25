using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using static Geometry;

public class MRule_Ring_Stripes : MandalaLanguageRule
{
    // Rule attributes
    private const int MinFactor = 2;
    private const int MaxFactor = 7;

    // Dynamic values set in Initialize
    private int NumStripes;
    private bool Alternating;
    private int StripeId1;
    private int StripeId2;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Ring source = (ME_Ring)sourceElement;

        List<MandalaElement> elements = new List<MandalaElement>();

        // Create SvgElement and add it to SvgDocument
        float angleStep = 360f / NumStripes;
        PointF lastStartPoint = new PointF(0,0), lastEndPoint = new PointF(0,0);
        for(int i = 0; i < NumStripes; i++)
        {
            float angle = i * angleStep;

            float startX = source.Center.X + (float)(source.InnerRadius * Math.Sin(DegreeToRadian(angle)));
            float startY = source.Center.Y + (float)(source.InnerRadius * Math.Cos(DegreeToRadian(angle)));
            PointF startPoint = new PointF(startX, startY);
            float endX = source.Center.X + (float)(source.OuterRadius * Math.Sin(DegreeToRadian(angle)));
            float endY = source.Center.Y + (float)(source.OuterRadius * Math.Cos(DegreeToRadian(angle)));
            PointF endPoint = new PointF(endX, endY);

            DrawLine(source.SvgDocument, startPoint, endPoint);

            int id = (Alternating && i % 2 == 1) ? StripeId2 : StripeId1;
            if (i > 0)
            {
                ME_Stripe stripe = new ME_Stripe(source.SvgDocument, source.Depth + 1, id, source.Center, source.InnerRadius, source.OuterRadius, lastStartPoint, startPoint, lastEndPoint, endPoint);
                elements.Add(stripe);
            }
            else
            {
                float lastAngle = 360f - angleStep;
                lastStartPoint = new PointF(source.Center.X + (float)(source.InnerRadius * Math.Sin(DegreeToRadian(lastAngle))), 
                    source.Center.Y + (float)(source.InnerRadius * Math.Cos(DegreeToRadian(lastAngle))));
                lastEndPoint = new PointF(source.Center.X + (float)(source.OuterRadius * Math.Sin(DegreeToRadian(lastAngle))),
                    source.Center.Y + (float)(source.OuterRadius * Math.Cos(DegreeToRadian(lastAngle))));
                ME_Stripe stripe = new ME_Stripe(source.SvgDocument, source.Depth + 1, id, source.Center, source.InnerRadius, source.OuterRadius, lastStartPoint, startPoint, lastEndPoint, endPoint);
                elements.Add(stripe);
            }

            lastStartPoint = startPoint;
            lastEndPoint = endPoint;
        }

        return elements;
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Ring source = (ME_Ring)sourceElement;

        // Get # of Stripes
        int factor = random.Next(MaxFactor - MinFactor) + MinFactor;
        NumStripes = 1;
        for (int i = 0; i < factor; i++)
        {
            NumStripes *= 2;
        }

        Alternating = random.Next(2) == 1;

        // Element ids
        StripeId1 = MandalaElement.ElementId++;
        StripeId2 = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Ring;
    }
}
