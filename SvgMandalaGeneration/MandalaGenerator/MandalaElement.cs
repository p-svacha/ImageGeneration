using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using System.Windows.Input;

public abstract class MandalaElement
{
    public static int ElementId = 0;

    public int Id;
    public SvgDocument SvgDocument;
    public MandalaElementType Type;
    public int Depth;
    public float Area;
    public SvgElement SvgElement;

    /// <summary>
    /// A MandalaElement is a shape inside the mandala that can include other shapes. Elements with the same id will be handled the same.
    /// </summary>
    public MandalaElement(SvgDocument svgDocument, int depth, int elementId)
    {
        Id = elementId;
        SvgDocument = svgDocument;
        Depth = depth;
    }

    protected abstract float CalculateArea();
    protected abstract SvgElement CreateSvgElement();

    protected void Initialize()
    {
        Area = CalculateArea();
        SvgElement = CreateSvgElement();
        SvgElement.MouseMove += UpdateUIInfo;
        SvgDocument.Children.Add(SvgElement);

        MandalaGenerator.AddElement(this);
    }

    protected void UpdateUIInfo(object sender, MouseArg m)
    {
        Console.WriteLine("Update");
    }
}
