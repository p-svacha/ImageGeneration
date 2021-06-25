using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;

public class MTRule_Empty_ComplexShape : MandalaLanguageRule
{

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Empty source = (ME_Empty)sourceElement;

        SvgPath svgPath = new SvgPath()
        {
            Fill = new SvgColourServer(Color.Black),
            StrokeWidth = 0
        };
        svgPath.PathData = new SvgPathSegmentList();

        
        float startX = 420;
        float startY = 420;
        PointF startPoint = new PointF(startX, startY);
        SvgMoveToSegment svgStartMove = new SvgMoveToSegment(startPoint);
        svgPath.PathData.Add(svgStartMove);

        float x2 = 500;
        float y2 = 500;
        PointF p2 = new PointF(x2, y2);
        SvgLineSegment line = new SvgLineSegment(startPoint, p2);
        svgPath.PathData.Add(line);

        float x3 = 500;
        float y3 = 300;
        PointF p3 = new PointF(x3, y3);
        SvgArcSegment arc = new SvgArcSegment(p2, 50, 50, 0, SvgArcSize.Small, SvgArcSweep.Negative, p3);
        svgPath.PathData.Add(arc);

        source.SvgDocument.Children.Add(svgPath);

        return new List<MandalaElement>() { };
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Empty;
    }
}
