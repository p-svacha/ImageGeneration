using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

public class MRule_Star_InnerStar : MandalaLanguageRule
{
    // Rule attributes
    private const float MinWidth = 0.05f; // % of outer radius of star
    private const float MaxWidth = 0.6f; // % of outer radius of star

    // Dynamic values set in Initialize
    private float Width;
    private int StarId;
    
    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Star source = (ME_Star)sourceElement;

        // Draw Star 
        float angle = source.AngleFromCenter;
        float widthFactor = (source.OuterRadius - Width) / source.OuterRadius;
        float innerRadius = source.InnerRadius * widthFactor;
        float outerRadius = source.OuterRadius - Width;
        PointF[] vertices = DrawStar(source.SvgDocument, source.Center, innerRadius, outerRadius, source.Corners, angle);

        List<MandalaElement> elements = new List<MandalaElement>();
        // Create Star Element
        ME_Star star = new ME_Star(source.SvgDocument, source.Depth + 1, StarId, source.Center, innerRadius, outerRadius, source.Corners, angle, source.Centered, vertices);
        elements.Add(star);

        return elements;
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Star source = (ME_Star)sourceElement;

        // Get Radius
        float minWidth = source.OuterRadius * MinWidth;
        float maxWidth = source.OuterRadius * MaxWidth;
        Width = (float)random.NextDouble() * (maxWidth - minWidth) + minWidth;

        // Element ids
        StarId = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Star;
    }
}
