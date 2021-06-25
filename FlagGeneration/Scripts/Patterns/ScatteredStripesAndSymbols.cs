using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebSockets;

namespace FlagGeneration
{
    class ScatteredStripesAndSymbols : FlagMainPattern
    {
        private const int STRIPE_OFFSET = 100; // how much outside the flag the stripes start (needed to avoid seeing the end of the stripe)

        private const int MIN_STRIPES = 0;
        private const int MAX_STRIPES = 3;

        private const float MIN_STRIPE_WIDTH = 0.03f * FlagWidth;
        private const float MAX_STRIPE_WIDTH = 0.13f * FlagWidth;

        private Dictionary<int, int> NumSymbols = new Dictionary<int, int>()
        {
            {0, 300 },
            {1, 100 },
            {2, 100 },
            {3, 90 },
            {4, 80 },
            {5, 70 },
            {6, 60 },
            {7, 50 },
            {8, 40 },
        };

        private const float MIN_SYMBOL_SIZE = 0.05f * FlagWidth;
        private const float MAX_SYMBOL_SIZE = 0.45f * FlagWidth;

        private const float FIXED_RANDOM_ANGLE_CHANCE = 0.4f; // chance that all symbols have the same random angle
        private const float INDIVIDUAL_RANDOM_ANGLE_CHANCE = 0.2f; // chance that all symbols are rotated individually random

        private const float MIN_COA_SIZE = FlagHeight * 0.3f;
        private const float MAX_COA_SIZE = FlagHeight * 0.75f;

        List<Polygon> Hitboxes = new List<Polygon>();

        enum StripeStyle
        {
            Random,
            ThroughCenter,
            Straight
        }

        private Dictionary<StripeStyle, int> StripeStyles = new Dictionary<StripeStyle, int>()
        {
            {StripeStyle.Random, 30 },
            {StripeStyle.Straight, 50 },
            {StripeStyle.ThroughCenter, 70}
        };

        public override void Apply(SvgDocument SvgDocument, Random r)
        {
            R = r;
            ColorManager = new ColorManager(R);

            Color backgroundColor = ColorManager.GetRandomColor();
            List<Color> usedColors = new List<Color>() { backgroundColor };

            int numSymbols = GetWeightedRandomInt(NumSymbols);
            bool hasSymbols = numSymbols > 0;

            DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, backgroundColor);

            // Stripes (when there are no symbols there are always stripes)
            int minStripes = hasSymbols ? MIN_STRIPES : 1;
            int numStripes = RandomRange(minStripes, MAX_STRIPES + 1);
            float thickness = RandomRange(MIN_STRIPE_WIDTH, MAX_STRIPE_WIDTH);
            StripeStyle stripeStyle = GetWeightedRandomEnum(StripeStyles);

            List<Color> stripeColors = new List<Color>();
            for (int i = 0; i < numStripes; i++)
            {
                Color stripeColor = ColorManager.GetRandomColor(usedColors);
                if (i == 2 && R.NextDouble() < 0.5f) stripeColor = stripeColors[0];
                stripeColors.Add(stripeColor);
                usedColors.Add(stripeColor);
            }
            List<Vector2> stripeLine = DrawStripes(SvgDocument, numStripes, thickness, stripeColors.ToArray(), stripeStyle);



            // Symbols
            Symbol symbol = GetRandomSymbol();
            List<Color> candidateColors = new List<Color>();
            foreach (Color c in stripeColors) candidateColors.Add(c);
            candidateColors.Add(ColorManager.GetRandomColor(usedColors));
            Color symbolColor = candidateColors[R.Next(0, candidateColors.Count)];

            float angle = 0;
            if (R.NextDouble() < FIXED_RANDOM_ANGLE_CHANCE) angle = RandomRange(0, 360);
            bool individualRandomAngles = R.NextDouble() < INDIVIDUAL_RANDOM_ANGLE_CHANCE;

            for (int i = 0; i < numSymbols; i++)
            {
                bool overlap;
                float size, xPos, yPos;
                Polygon hitbox;
                int counter = 0;
                bool stop = false;

                do
                {
                    if (counter >= 50) stop = true;
                    size = RandomRange(MIN_SYMBOL_SIZE, MAX_SYMBOL_SIZE);
                    xPos = RandomRange(size, FlagWidth - size);
                    yPos = RandomRange(size, FlagHeight - size);

                    float hitboxSize = size + FlagWidth * 0.04f;
                    float hitboxMargin = 15f;
                    hitbox = new Polygon(new List<Vector2>() {
                        new Vector2(xPos - size * 0.5f - hitboxMargin, yPos - size * 0.5f - hitboxMargin),
                        new Vector2(xPos + size * 0.5f + hitboxMargin, yPos - size * 0.5f - hitboxMargin),
                        new Vector2(xPos + size * 0.5f + hitboxMargin, yPos + size * 0.5f + hitboxMargin), 
                        new Vector2(xPos - size * 0.5f - hitboxMargin, yPos + size * 0.5f + hitboxMargin) });
                    overlap = Hitboxes.Any(x => x.IsPolygonsIntersecting(hitbox));
                    counter++;
                } while (overlap && !stop);

                if (!stop)
                {
                    Hitboxes.Add(hitbox);
                    if (individualRandomAngles) angle = RandomRange(0, 360);
                    symbol.Draw(SvgDocument, this, new Vector2(xPos, yPos), size, angle, symbolColor);
                }
            }

            // Coat of arms
            if(!hasSymbols)
            {
                CoatOfArmsChance = 0.5f;
                CoatOfArmsColor = ColorManager.GetRandomColor(usedColors);

                // Chose random point on line for coa position
                float factor = (float)(R.NextDouble() * 0.5f + 0.25f);
                if (factor > 0.4f && factor < 0.6f) factor = 0.5f;
                Vector2 stripeVector = stripeLine[1] - stripeLine[0];
                CoatOfArmsPosition = stripeLine[0] + factor * stripeVector;

                // Adjust size depending on position
                float maxCoaSize = Math.Min(MAX_COA_SIZE, ClosestDistanceToFlagEdge(CoatOfArmsPosition));
                float minCoaSize = Math.Min(MIN_COA_SIZE, maxCoaSize);
                CoatOfArmsSize = RandomRange(minCoaSize, maxCoaSize);

                ApplyCoatOfArms(SvgDocument);
            }
        }

        #region Stripe operations

        /// <summary>
        /// Draws x stripes on the flag with a specified thickess and colors. A start and end point will be chosen according to the speicifed style and returned.
        /// </summary>
        private List<Vector2> DrawStripes(SvgDocument SvgDocument, int numStripes, float thickness, Color[] colors, StripeStyle style)
        {
            FlagSide startSide = GetRandomFlagSide();

            Vector2 startPoint = GetRandomPointOnFlagSide(startSide);
            Vector2 endPoint = GetEndPoint(startSide, startPoint, style);
            Vector2 thicknessVector = new Vector2(endPoint.Y - startPoint.Y, -(endPoint.X - startPoint.X));
            thicknessVector = Vector2.Normalize(thicknessVector);

            float totalThickness = numStripes * thickness;

            for (int i = 0; i < numStripes; i++)
            {
                float lineOffset = (-totalThickness * 0.5f) + (i * thickness) + (thickness * 0.5f);
                Vector2 lineStartPoint = new Vector2(startPoint.X + thicknessVector.X * lineOffset, startPoint.Y + thicknessVector.Y * lineOffset);
                Vector2 lineEndPoint = new Vector2(endPoint.X + thicknessVector.X * lineOffset, endPoint.Y + thicknessVector.Y * lineOffset);

                Vector2[] vertices = new Vector2[]
                {
                new Vector2(lineStartPoint.X + thicknessVector.X * thickness * 0.5f, lineStartPoint.Y + thicknessVector.Y * thickness * 0.5f),
                new Vector2(lineEndPoint.X + thicknessVector.X * thickness * 0.5f, lineEndPoint.Y + thicknessVector.Y * thickness * 0.5f),
                new Vector2(lineEndPoint.X - thicknessVector.X * thickness * 0.5f, lineEndPoint.Y - thicknessVector.Y * thickness * 0.5f),
                new Vector2(lineStartPoint.X - thicknessVector.X * thickness * 0.5f, lineStartPoint.Y - thicknessVector.Y * thickness * 0.5f),
                };
                DrawPolygon(SvgDocument, vertices, colors[i]);
                Hitboxes.Add(new Polygon(vertices.ToList()));
            }

            return new List<Vector2>() { startPoint, endPoint };
        }

        private FlagSide GetRandomFlagSide()
        {
            double rng = R.NextDouble();
            if (rng < 0.25) return FlagSide.Left;
            if (rng < 0.5) return FlagSide.Top;
            if (rng < 0.75) return FlagSide.Right;
            return FlagSide.Bottom;
        }

        private FlagSide GetOppositeFlagSide(FlagSide side)
        {
            if (side == FlagSide.Bottom) return FlagSide.Top;
            if (side == FlagSide.Top) return FlagSide.Bottom;
            if (side == FlagSide.Left) return FlagSide.Right;
            if (side == FlagSide.Right) return FlagSide.Left;
            return FlagSide.Left;
        }

        private Vector2 GetEndPoint(FlagSide startSide, Vector2 startPoint, StripeStyle style)
        {
            switch(style)
            {
                case StripeStyle.Random:
                    return GetRandomPointOnFlagSide(GetOppositeFlagSide(startSide));

                case StripeStyle.Straight:
                    if (startSide == FlagSide.Bottom) return new Vector2(startPoint.X, -STRIPE_OFFSET);
                    if (startSide == FlagSide.Top) return new Vector2(startPoint.X, FlagHeight + STRIPE_OFFSET);
                    if (startSide == FlagSide.Left) return new Vector2(FlagWidth + STRIPE_OFFSET, startPoint.Y);
                    if (startSide == FlagSide.Right) return new Vector2(-STRIPE_OFFSET, startPoint.Y);
                    throw new Exception();

                case StripeStyle.ThroughCenter:
                    if (startSide == FlagSide.Bottom) return new Vector2(FlagWidth - startPoint.X, -STRIPE_OFFSET);
                    if (startSide == FlagSide.Top) return new Vector2(FlagWidth - startPoint.X, FlagHeight + STRIPE_OFFSET);
                    if (startSide == FlagSide.Left) return new Vector2(FlagWidth + STRIPE_OFFSET, FlagHeight - startPoint.Y);
                    if (startSide == FlagSide.Right) return new Vector2(-STRIPE_OFFSET, FlagHeight - startPoint.Y);
                    throw new Exception();

                default:
                    throw new Exception();
            }
        }

        private Vector2 GetRandomPointOnFlagSide(FlagSide side)
        {
            
            if (side == FlagSide.Left) return new Vector2(-STRIPE_OFFSET, RandomRange(0, FlagHeight));
            if (side == FlagSide.Top) return new Vector2(RandomRange(0, FlagWidth), -STRIPE_OFFSET);
            if (side == FlagSide.Right) return new Vector2(FlagWidth + STRIPE_OFFSET, RandomRange(0, FlagHeight));
            if (side == FlagSide.Bottom) return new Vector2(RandomRange(0, FlagWidth), FlagHeight + STRIPE_OFFSET);
            throw new Exception();
        }

        enum FlagSide
        {
            Left,
            Top,
            Right,
            Bottom
        }

        #endregion

        class Polygon
        {
            List<Vector2> Points;

            public Polygon(List<Vector2> points)
            {
                Points = points;
            }

            public bool IsPolygonsIntersecting(Polygon other)
            {
                foreach (var polygon in new[] { this, other })
                {
                    for (int i1 = 0; i1 < polygon.Points.Count; i1++)
                    {
                        int i2 = (i1 + 1) % polygon.Points.Count;
                        var p1 = polygon.Points[i1];
                        var p2 = polygon.Points[i2];

                        var normal = new Vector2(p2.Y - p1.Y, p1.X - p2.X);

                        double? minA = null, maxA = null;
                        foreach (var p in Points)
                        {
                            var projected = normal.X * p.X + normal.Y * p.Y;
                            if (minA == null || projected < minA)
                                minA = projected;
                            if (maxA == null || projected > maxA)
                                maxA = projected;
                        }

                        double? minB = null, maxB = null;
                        foreach (var p in other.Points)
                        {
                            var projected = normal.X * p.X + normal.Y * p.Y;
                            if (minB == null || projected < minB)
                                minB = projected;
                            if (maxB == null || projected > maxB)
                                maxB = projected;
                        }

                        if (maxA < minB || maxB < minA)
                            return false;
                    }
                }
                return true;
            }
        }
    }

}
