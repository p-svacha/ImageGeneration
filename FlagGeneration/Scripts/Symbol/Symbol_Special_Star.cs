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
    class Symbol_Special_Star : Symbol
    {
        private readonly Dictionary<int, int> NumSpikesDictionary = new Dictionary<int, int>()
        {
            {3, 100 },
            {4, 100 },
            {5, 100 },
            {6, 100},
            {7, 100 },
            {8, 100 },
            {9, 100 },
            {10, 100 },
            {12, 100 },
            {16, 100 },
            {24, 100 },
            {32, 100 },
        };

        private const float MIN_RADIUS_RATIO = 0.05f;
        private const float MAX_RADIUS_RATIO = 0.8f;

        private const float HALF_STAR_CHANCE = 0.1f;

        // Instance values
        private int NumSpikes;
        private float RadiusRatio;
        private bool HalfStar;
        private bool HalfStarMoved;

        public Symbol_Special_Star(FlagMainPattern flag, Random R) : base(flag, R)
        {
            NumSpikes = GetNumSpikes(R);
            RadiusRatio = RandomRange(MIN_RADIUS_RATIO, MAX_RADIUS_RATIO);
            HalfStar = R.NextDouble() < HALF_STAR_CHANCE;
            HalfStarMoved = R.NextDouble() < 0.5f;
        }

        public override void Draw(SvgDocument Svg, Vector2 center, float size, float angle, Color primaryColor, Color secondaryColor)
        {
            float startAngle = angle + 180;
            float outerRadius = size * 0.5f;
            float innerRadius = outerRadius * RadiusRatio;

            int numVertices = NumSpikes * 2;
            Vector2[] vertices = new Vector2[numVertices];

            // Create vertices
            if (!HalfStar) // Full star
            {
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
            }
            else // Half star
            {
                startAngle -= 90;
                if(HalfStarMoved) // 50% chance to move center of half star so its the actual center
                    center = new Vector2(center.X + (float)(size / 4 * Math.Sin(DegreeToRadian(startAngle-90))), center.Y + (float)(size / 4 * Math.Cos(DegreeToRadian(startAngle-90))));
                float angleStep = 180f / (numVertices - 2);
                for (int i = 0; i < numVertices; i++)
                {
                    float curAngle = startAngle + ((i-1) * angleStep);
                    bool outerCorner = i % 2 == 1;
                    float radius = i == 0 ? 0 : outerCorner ? outerRadius : innerRadius;
                    float x = center.X + (float)(radius * Math.Sin(DegreeToRadian(curAngle)));
                    float y = center.Y + (float)(radius * Math.Cos(DegreeToRadian(curAngle)));
                    vertices[i] = new Vector2(x, y);
                }
            }
            Flag.DrawPolygon(Svg, vertices, primaryColor);

            if (HasFrame && !HalfStar && RadiusRatio > 0.3f)
            {
                float angleStep = 360f / numVertices;
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

        public int GetNumSpikes(Random R)
        {
            int probabilitySum = NumSpikesDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<int, int> kvp in NumSpikesDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return 0;
        }
    }
}
