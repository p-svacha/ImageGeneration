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
    class Symbol_Cross : Symbol
    {

        private const float MIN_CROSS_WIDTH = 0.2f; // relative to cross size
        private const float MAX_CROSS_WIDTH = 0.6f; // relative to cross size
        private float CrossWidthRel;

        private const float X_CHANCE = 0.2f; // chance that + is turned into x
        private bool IsX;

        public Symbol_Cross(FlagMainPattern flag, Random R) : base(flag, R)
        {
            CrossWidthRel = RandomRange(MIN_CROSS_WIDTH, MAX_CROSS_WIDTH);
            IsX = R.NextDouble() < X_CHANCE;
        }

        protected override void OverrideValues()
        {
            MIN_FRAME_WIDTH = 0.8f;
        }

        public override void Draw(SvgDocument Svg, Vector2 center, float size, float angle, Color primaryColor, Color secondaryColor)
        {
            float crossWidth = CrossWidthRel * size;

            List<Vector2> crossPoints = new List<Vector2>()
            {
                new Vector2(center.X - crossWidth * 0.5f, center.Y - size * 0.5f),
                new Vector2(center.X + crossWidth * 0.5f, center.Y - size * 0.5f),
                new Vector2(center.X + crossWidth * 0.5f, center.Y - crossWidth * 0.5f),
                new Vector2(center.X + size * 0.5f, center.Y - crossWidth * 0.5f),
                new Vector2(center.X + size * 0.5f, center.Y + crossWidth * 0.5f),
                new Vector2(center.X + crossWidth * 0.5f, center.Y + crossWidth * 0.5f),
                new Vector2(center.X + crossWidth * 0.5f, center.Y + size * 0.5f),
                new Vector2(center.X - crossWidth * 0.5f, center.Y + size * 0.5f),
                new Vector2(center.X - crossWidth * 0.5f, center.Y + crossWidth * 0.5f),
                new Vector2(center.X - size * 0.5f, center.Y + crossWidth * 0.5f),
                new Vector2(center.X - size * 0.5f, center.Y - crossWidth * 0.5f),
                new Vector2(center.X - crossWidth * 0.5f, center.Y - crossWidth * 0.5f),
            };

            if (IsX) angle += 45;

            if(angle != 0)
            {
                List<Vector2> rotatedPoints = new List<Vector2>();
                foreach(Vector2 v in crossPoints)
                {
                    rotatedPoints.Add(Geometry.RotatePoint(v, center, angle));
                }
                crossPoints = rotatedPoints;
            }

            Flag.DrawPolygon(Svg, crossPoints.ToArray(), primaryColor);

            if (HasFrame)
            {
                float frameWidthAbs = size * (1f - FrameWidth);
                float frameSize = size - frameWidthAbs;
                float framedCrossWidth = crossWidth - frameWidthAbs;

                List<Vector2> framedCrossPoints = new List<Vector2>()
                {
                    new Vector2(center.X - framedCrossWidth * 0.5f, center.Y - frameSize * 0.5f),
                    new Vector2(center.X + framedCrossWidth * 0.5f, center.Y - frameSize * 0.5f),
                    new Vector2(center.X + framedCrossWidth * 0.5f, center.Y - framedCrossWidth * 0.5f),
                    new Vector2(center.X + frameSize * 0.5f, center.Y - framedCrossWidth * 0.5f),
                    new Vector2(center.X + frameSize * 0.5f, center.Y + framedCrossWidth * 0.5f),
                    new Vector2(center.X + framedCrossWidth * 0.5f, center.Y + framedCrossWidth * 0.5f),
                    new Vector2(center.X + framedCrossWidth * 0.5f, center.Y + frameSize * 0.5f),
                    new Vector2(center.X - framedCrossWidth * 0.5f, center.Y + frameSize * 0.5f),
                    new Vector2(center.X - framedCrossWidth * 0.5f, center.Y + framedCrossWidth * 0.5f),
                    new Vector2(center.X - frameSize * 0.5f, center.Y + framedCrossWidth * 0.5f),
                    new Vector2(center.X - frameSize * 0.5f, center.Y - framedCrossWidth * 0.5f),
                    new Vector2(center.X - framedCrossWidth * 0.5f, center.Y - framedCrossWidth * 0.5f),
                };

                if (angle != 0)
                {
                    List<Vector2> rotatedPoints = new List<Vector2>();
                    foreach (Vector2 v in framedCrossPoints)
                    {
                        rotatedPoints.Add(Geometry.RotatePoint(v, center, angle));
                    }
                    framedCrossPoints = rotatedPoints;
                }

                Flag.DrawPolygon(Svg, framedCrossPoints.ToArray(), secondaryColor);
            }
        }
    }
}
