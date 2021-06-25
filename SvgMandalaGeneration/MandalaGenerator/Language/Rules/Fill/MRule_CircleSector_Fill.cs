using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;

public class MRule_CircleSector_Fill : MandalaLanguageRule
{
    // Rule attributes
    private const float MinRadius = 0.4f;
    private const float MaxRadius = 0.49f;

    // Dynamic values set in Initialize
    SvgPaintServer Colour;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_CircleSector source = (ME_CircleSector)sourceElement;

        SvgPath svgPath = new SvgPath()
        {
            Fill = Colour,
            StrokeWidth = 0
        };
        svgPath.PathData = new SvgPathSegmentList();

        SvgMoveToSegment svgStartMove = new SvgMoveToSegment(source.InnerVertex);
        svgPath.PathData.Add(svgStartMove);

        SvgLineSegment line = new SvgLineSegment(source.InnerVertex, source.OuterVertex1);
        svgPath.PathData.Add(line);

        SvgArcSegment arc = new SvgArcSegment(source.OuterVertex1, source.OuterRadius, source.OuterRadius, 0, SvgArcSize.Small, SvgArcSweep.Negative, source.OuterVertex2);
        svgPath.PathData.Add(arc);

        source.SvgDocument.Children.Add(svgPath);

        return new List<MandalaElement>() { };
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_CircleSector source = (ME_CircleSector)sourceElement;
        Colour = new SvgColourServer(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.CircleSector;
    }
}
