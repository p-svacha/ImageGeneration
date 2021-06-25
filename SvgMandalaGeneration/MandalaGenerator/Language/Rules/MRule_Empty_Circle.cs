using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

public class MRule_Empty_Circle : MandalaLanguageRule
{
    // Rule attributes
    private const float MinRadius = 0.4f;
    private const float MaxRadius = 0.49f;

    // Dynamic values set in Initialize
    private float Radius;
    private int CircleId;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Empty source = (ME_Empty)sourceElement;

        // Take Center
        float centerX  = source.SvgDocument.Width / 2;
        float centerY = source.SvgDocument.Height / 2;
        PointF center = new PointF(centerX, centerY);

        // Create SvgElement and add it to SvgDocument
        DrawCircle(source.SvgDocument, center, Radius);

        // Create Mandala Elements and return them
        ME_Circle circle = new ME_Circle(source.SvgDocument, source.Depth + 1, CircleId, center, Radius, 0, true);

        return new List<MandalaElement>() { circle };
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Empty source = (ME_Empty)sourceElement;

        // Get Radius
        float minRadius = source.SvgDocument.Width * MinRadius;
        float maxRadius = source.SvgDocument.Width * MaxRadius;
        Radius = (float)random.NextDouble() * (maxRadius - minRadius) + minRadius;

        // Element ids
        CircleId = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Empty;
    }
}
