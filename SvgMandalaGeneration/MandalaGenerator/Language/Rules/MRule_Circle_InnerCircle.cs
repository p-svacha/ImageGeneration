using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

public class MRule_Circle_InnerCircle : MandalaLanguageRule
{
    // Rule attributes
    private const float MinRadius = 0.2f;
    private const float MaxRadius = 0.95f;

    // Dynamic values set in Initialize
    private float Radius;
    private int CircleId;
    private int RingId;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Circle source = (ME_Circle)sourceElement;

        // Create SvgElement and add it to SvgDocument
        DrawCircle(source.SvgDocument, source.Center, Radius);

        // Create Mandala Elements and return them
        ME_Circle circle = new ME_Circle(source.SvgDocument, source.Depth + 1, CircleId, source.Center, Radius, source.AngleFromCenter, source.Centered); ;
        ME_Ring ring = new ME_Ring(source.SvgDocument, source.Depth + 1, RingId, source.Center, Radius, source.Radius);

        return new List<MandalaElement>() { circle, ring };
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Circle source = (ME_Circle)sourceElement;

        // Get Radius
        float minRadius = source.Radius * MinRadius;
        float maxRadius = source.Radius * MaxRadius;
        Radius = (float)random.NextDouble() * (maxRadius - minRadius) + minRadius;

        // Element ids
        CircleId = MandalaElement.ElementId++;
        RingId = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Circle;
    }
}
