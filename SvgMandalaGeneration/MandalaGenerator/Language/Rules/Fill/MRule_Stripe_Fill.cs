using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;

public class MRule_Stripe_Fill : MandalaLanguageRule
{
    // Rule attributes

    // Dynamic values set in Initialize
    SvgPaintServer Colour;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Stripe source = (ME_Stripe)sourceElement;

        SvgPath svgPath = new SvgPath()
        {
            Fill = Colour,
            StrokeWidth = 0,
        };
        svgPath.PathData = new SvgPathSegmentList();

        SvgMoveToSegment svgStartMove = new SvgMoveToSegment(source.InnerVertex1);
        svgPath.PathData.Add(svgStartMove);

        SvgArcSegment arc1 = new SvgArcSegment(source.InnerVertex1, source.InnerRadius, source.InnerRadius, 0, SvgArcSize.Small, SvgArcSweep.Negative, source.InnerVertex2);
        svgPath.PathData.Add(arc1);

        SvgLineSegment line1 = new SvgLineSegment(source.InnerVertex2, source.OuterVertex2);
        svgPath.PathData.Add(line1);

        SvgArcSegment arc2 = new SvgArcSegment(source.OuterVertex2, source.OuterRadius, source.OuterRadius, 0, SvgArcSize.Small, SvgArcSweep.Positive, source.OuterVertex1);
        svgPath.PathData.Add(arc2);

        SvgLineSegment line2 = new SvgLineSegment(source.OuterVertex1, source.InnerVertex1);
        svgPath.PathData.Add(line2);

        source.SvgDocument.Children.Add(svgPath);

        return new List<MandalaElement>() { };
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Stripe source = (ME_Stripe)sourceElement;
        Colour = new SvgColourServer(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Stripe;
    }
}
