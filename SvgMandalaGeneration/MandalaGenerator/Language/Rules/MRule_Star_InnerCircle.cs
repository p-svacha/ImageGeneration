using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

public class MRule_Star_InnerCircle : MandalaLanguageRule
{
    // Rule attributes

    // Dynamic values set in Initialize
    private int CircleId;
    private bool Alternating;
    private int SpikeId1;
    private int SpikeId2;

    public override List<MandalaElement> Apply(MandalaElement sourceElement)
    {
        // Convert source to correct type
        ME_Star source = (ME_Star)sourceElement;

        // Take Center
        float radius = source.InnerRadius;

        DrawCircle(source.SvgDocument, source.Center, radius);

        List<MandalaElement> elements = new List<MandalaElement>();

        // Add circle element
        ME_Circle circle = new ME_Circle(source.SvgDocument, source.Depth + 1, CircleId, source.Center, radius, source.AngleFromCenter, source.Centered);
        elements.Add(circle);

        // Add star spike elements
        float angleStep = 360f / source.Corners;
        for(int i = 0; i < source.Corners; i++)
        {
            int cornerId = i * 2;
            float angle = source.AngleFromCenter + i * angleStep;
            int id = (Alternating && i % 2 == 1) ? SpikeId2 : SpikeId1;
            
            ME_StarSpike spike = new ME_StarSpike(source.SvgDocument, source.Depth + 1, id, source.Center, source.InnerRadius, angle,
                source.Vertices[cornerId == 0 ? (source.Vertices.Length - 1) : (cornerId - 1)],
                source.Vertices[(cornerId + 1) % source.Vertices.Length],
                source.Vertices[cornerId]
                );
            elements.Add(spike);
            
        }

        return elements;
    }

    public override void Initialize(MandalaElement sourceElement, Random random)
    {
        // Convert source to correct type
        ME_Star source = (ME_Star)sourceElement;

        Alternating = random.Next(2) == 1;
        if (source.Corners % 2 == 1) Alternating = false;

        // Element ids
        CircleId = MandalaElement.ElementId++;
        SpikeId1 = MandalaElement.ElementId++;
        SpikeId2 = MandalaElement.ElementId++;
    }

    public override bool CanApply(MandalaElement sourceElement)
    {
        return sourceElement.Type == MandalaElementType.Star;
    }
}
