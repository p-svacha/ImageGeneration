            using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FlagGeneration.Geometry;

namespace FlagGeneration
{
    class FramedCoa : CoatOfArms
    {
        private enum FrameType
        {
            Circle,
            Star
        }

        private readonly Dictionary<FrameType, int> FrameTypes = new Dictionary<FrameType, int>()
        {
            { FrameType.Circle, 100 },
            { FrameType.Star, 50 }
        };

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float size, Color primaryColor, List<Color> flagColors)
        {
            float coaSize = 0f;

            // Draw Frame
            switch(GetRandomFrameType(R))
            {
                case FrameType.Circle:
                    flag.DrawCircle(Svg, pos, size/2, primaryColor);
                    coaSize = size * 0.8f;
                    break;

                case FrameType.Star:
                    int numCorners = R.Next(17) + 8;
                    float startAngle = 180;
                    float outerRadius = size * 0.5f;
                    float innerRadius = outerRadius * flag.RandomRange(0.6f, 0.95f);

                    int numVertices = numCorners * 2;
                    Vector2[] vertices = new Vector2[numVertices];

                    // Create vertices
                    float angleStep = 360f / numVertices;
                    for (int i = 0; i < numVertices; i++)
                    {
                        float curAngle = startAngle + (i * angleStep);
                        bool outerCorner = i % 2 == 0;
                        float radius = outerCorner ? outerRadius : innerRadius;
                        float x = pos.X + (float)(radius * Math.Sin(DegreeToRadian(curAngle)));
                        float y = pos.Y + (float)(radius * Math.Cos(DegreeToRadian(curAngle)));
                        vertices[i] = new Vector2(x, y);
                    }

                    flag.DrawPolygon(Svg, vertices, primaryColor);

                    coaSize = innerRadius * 2 * 0.8f;
                    break;
            }

            // Draw COA inside frame
            if (flagColors == null) flagColors = new List<Color>();
            if (flagColors.Contains(primaryColor)) flagColors.Remove(primaryColor);
            flagColors.Add(flag.ColorManager.GetRandomColor(flagColors));

            Color coaColor = flagColors[R.Next(0, flagColors.Count)];
            List<Color> coaSecondaryColors = flagColors.Except(new List<Color>() { coaColor }).ToList();
            CoatOfArms coa = flag.GetRandomCoa();
            coa.Draw(Svg, flag, R, pos, coaSize, coaColor, coaSecondaryColors);

        }

        private FrameType GetRandomFrameType(Random R)
        {
            int probabilitySum = FrameTypes.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<FrameType, int> kvp in FrameTypes)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return FrameType.Circle;
        }
    }
}
