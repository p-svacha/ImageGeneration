using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FlagGeneration.Geometry;

namespace FlagGeneration
{
    class Symbol_Default_Star : Symbol
    {
        public Symbol_Default_Star(FlagMainPattern flag, Random R) : base(flag, R) { }

        public override void Draw(SvgDocument Svg, Vector2 center, float size, float angle, Color primaryColor, Color secondaryColor)
        {
            int numCorners = 5;
            float startAngle = angle + 180;
            float outerRadius = size * 0.5f;
            float innerRadius = outerRadius * 0.4f;

            int numVertices = numCorners * 2;
            Vector2[] vertices = new Vector2[numVertices];

            // Create vertices
            float angleStep = 360f / numVertices;
            for (int i = 0; i < numVertices; i++)
            {
                float curAngle = startAngle + (i * angleStep);
                bool outerCorner = i % 2 == 0;
                float radius = outerCorner ? outerRadius : innerRadius;
                float x = center.X + (float)(radius * Math.Sin(DegreeToRadian(curAngle)));
                float y = center.Y + (float)(radius * Math.Cos(DegreeToRadian(curAngle)));
                vertices[i] = new Vector2(x, y);
            }

            Flag.DrawPolygon(Svg, vertices, primaryColor);

            if(HasFrame)
            {
                float framedOuterRadius = outerRadius * FrameWidth;
                float framedInnerRadius = innerRadius * FrameWidth;
                Vector2[] framedVertices = new Vector2[numVertices];
                for (int i = 0; i < numVertices; i++)
                {
                    float curAngle = startAngle + (i * angleStep);
                    bool outerCorner = i % 2 == 0;
                    float radius = outerCorner ? framedOuterRadius : framedInnerRadius;
                    float x = center.X + (float)(radius * Math.Sin(DegreeToRadian(curAngle)));
                    float y = center.Y + (float)(radius * Math.Cos(DegreeToRadian(curAngle)));
                    framedVertices[i] = new Vector2(x, y);
                }
                Flag.DrawPolygon(Svg, framedVertices, secondaryColor);
            }
        }
    }
}
