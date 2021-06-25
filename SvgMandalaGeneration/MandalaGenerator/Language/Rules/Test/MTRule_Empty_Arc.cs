using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

public class MTRule_Empty_Arc : MandalaLanguageRule
{

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Empty source = (ME_Empty)sourceElement;

        // Take Center
        float centerX  = source.SvgDocument.Width / 2;
        float centerY = source.SvgDocument.Height / 2;
        PointF center = new PointF(centerX, centerY);

        // Create SvgElement and add it to SvgDocument
        Random random = new Random();
        float startAngle = random.Next(360);
        float endAngle = (startAngle + random.Next(360 - (int)startAngle));
        DrawArc(source.SvgDocument, center, 0.4f * source.SvgDocument.Width, startAngle, endAngle);
        Console.WriteLine(startAngle + ", " + endAngle);

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
