using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Drawing;

public class ME_Empty : MandalaElement
{
    public ME_Empty(SvgDocument svgDocument, int depth, int elementId) : base(svgDocument, depth, elementId)
    {
        Type = MandalaElementType.Empty;

        Initialize();
    }

    protected override float CalculateArea()
    {
        return SvgDocument.Width * SvgDocument.Height;
    }

    protected override SvgElement CreateSvgElement()
    {
        return new SvgRectangle()
        {
            X = 0,
            Y = 0,
            Width = SvgDocument.Width,
            Height = SvgDocument.Height,
            Fill = new SvgColourServer(Color.Transparent),
            Stroke = new SvgColourServer(Color.Transparent)
        };
    }
}