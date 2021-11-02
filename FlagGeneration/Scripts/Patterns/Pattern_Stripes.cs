using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class Pattern_Stripes : FlagMainPattern
    {
        private Dictionary<int, int> NumStripesDictionary = new Dictionary<int, int>()
        {
            {2, 60 },
            {3, 60 },
            {4, 20 },
            {5, 30 },
            {6, 5 },
            {7, 10 },
            {9, 5 },
            {11, 5},
            {13, 5}
        };

        public enum StripeDirectionType
        {
            Horizontal,
            Vertical
        }
        private Dictionary<StripeDirectionType, int> StripeDirections = new Dictionary<StripeDirectionType, int>()
        {
            {StripeDirectionType.Horizontal, 60 },
            {StripeDirectionType.Vertical, 40 }
        };

        public enum OverlayType
        {
            None,
            LeftTriangle,
            TopLeftRect,
            Antigua
        }
        private Dictionary<OverlayType, int> Overlays = new Dictionary<OverlayType, int>()
        {
            {OverlayType.None, 100 },
            {OverlayType.TopLeftRect, 50 },
            {OverlayType.LeftTriangle, 50 },
            {OverlayType.Antigua, 5 }
        };

        private const float ALTERNATING_COLORS_CHANCE = 0.6f; // Chance that two colors alternate
        private const float UNEVEN_SYMMETRY_CHANCE = 0.8f; // Chance that colors are symmetrical when there is an uneven amount of stripes

        private const float WIDE_MID_STRIPE_CHANCE = 0.3f;
        private const float MIN_MID_STRIPE_SIZE = 0.2f;
        private const float MAX_MID_STRIPE_SIZE = 0.8f;

        // Overlays
        private const float MID_STRIPE_SYMBOLS_CHANCE = 0.5f; // Chance that instead of a coat of arms, the mid stripe has multiple instances of symbols
        private const int MID_STRIPE_SYMBOLS_MIN_AMOUNT = 2;
        private const int MID_STRIPE_SYMBOLS_MAX_AMOUNT = 5;

        private const float LEFT_TRIANGLE_MIN_WIDTH = 0.2f; // Relative to flag width
        private const float LEFT_TRIANGLE_MAX_WIDTH = 0.6f; // Relative to flag width
        private const float LEFT_TRIANGLE_FULL_WIDTH_CHANCE = 0.15f;
        private const float LEFT_INNER_TRIANGLE_CHANCE = 0.14f; // Chance that an inner triangle appears (like East Timor)
        private const float LEFT_INNER_TRIANGLE_UNIQUE_COLOR_CHANCE = 0.7f; // Chance that an inner triangle has a unique color

        private const float BIG_COA_CHANCE = 0.5f;
        private const float COA_CHANCE = 0.5f;

        // Active values
        public int NumStripes;
        public bool EvenStripes;
        public bool HasWideMidStripe;
        public StripeDirectionType StripeDirection;

        public override void DoApply()
        {
            NumStripes = GetNumStripes();

            Color[] stripeColors = new Color[NumStripes];

            StripeDirection = GetStripeDirection();
            EvenStripes = NumStripes % 2 == 0;
            bool alternate = R.NextDouble() < ALTERNATING_COLORS_CHANCE;

            // Get stripe colors
            if (alternate || NumStripes >= 8 || (EvenStripes && NumStripes == 6)) // Alternating colored stripes
            {
                Color c1 = ColorManager.GetRandomColor();
                Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                for (int i = 0; i < NumStripes; i++)
                    stripeColors[i] = i % 2 == 0 ? c1 : c2;

            }
            else if (!EvenStripes && (R.NextDouble() < UNEVEN_SYMMETRY_CHANCE || NumStripes >= 7)) // Symmetrical colored stripes
            {
                for (int i = 0; i < NumStripes; i++)
                {
                    if (i < (NumStripes + 1) / 2) stripeColors[i] = ColorManager.GetRandomColor(stripeColors.Where(x => x != null).ToList());
                    else stripeColors[i] = stripeColors[NumStripes - 1 - i];
                }
            }
            else // All stripes different color
            {
                for (int i = 0; i < NumStripes; i++)
                {
                    stripeColors[i] = ColorManager.GetRandomColor(stripeColors.Where(x => x != null).ToList());
                }
            }

            float[] stripeSizes = new float[NumStripes]; // Stripe size (0-1)
            
            if (!EvenStripes && R.NextDouble() < WIDE_MID_STRIPE_CHANCE)
            {
                HasWideMidStripe = true;
                float midStripeSize = RandomRange(MIN_MID_STRIPE_SIZE, MAX_MID_STRIPE_SIZE);
                float otherStripesSize = (1f - midStripeSize) / (NumStripes - 1);
                for (int i = 0; i < NumStripes; i++)
                {
                    if (i == NumStripes / 2) stripeSizes[i] = midStripeSize;
                    else stripeSizes[i] = otherStripesSize;
                }
            }
            else
            {
                for (int i = 0; i < NumStripes; i++)
                    stripeSizes[i] = 1f / NumStripes;
            }

            // Draw stripes
            float curRel = 0;
            for(int i = 0; i < NumStripes; i++)
            {
                float stripeSize = stripeSizes[i];
                
                DrawRectangle(Svg,
                    StripeDirection == StripeDirectionType.Horizontal ? 0 : curRel * FlagWidth,
                    StripeDirection == StripeDirectionType.Horizontal ? curRel * FlagHeight : 0,
                    StripeDirection == StripeDirectionType.Horizontal ? FlagWidth : FlagWidth * stripeSize,
                    StripeDirection == StripeDirectionType.Horizontal ? FlagHeight * stripeSize : FlagHeight,
                    stripeColors[i]);
                curRel += stripeSize;
            }


            DrawOverlay(stripeColors, stripeSizes);
        }

        #region Overlay

        /// <summary>
        /// Choses and draws an overlay
        /// </summary>
        private void DrawOverlay(Color[] stripeColors, float[] stripeSizes)
        {
            switch (GetOverlayType())
            {
                case OverlayType.None:
                    DrawNoOverlay(stripeColors, stripeSizes);
                    break;

                case OverlayType.TopLeftRect:
                    DrawTopLeftRectOverlay(stripeColors, stripeSizes);
                    break;

                case OverlayType.LeftTriangle:
                    DrawLeftTriangleOverlay(stripeColors, stripeSizes);
                    break;

                case OverlayType.Antigua:
                    DrawAntiguaOverlay(stripeColors, stripeSizes);
                    break;
            }
        }

        /// <summary>
        /// Draws no overlay but can apply symbols and coat of arms onto the stripes
        /// </summary>
        private void DrawNoOverlay(Color[] stripeColors, float[] stripeSizes)
        {
            if (!EvenStripes && R.NextDouble() < MID_STRIPE_SYMBOLS_CHANCE)
            {
                int numSymbols = RandomRange(MID_STRIPE_SYMBOLS_MIN_AMOUNT, MID_STRIPE_SYMBOLS_MAX_AMOUNT);

                List<Color> candidateColors = new List<Color>();
                foreach (Color c in stripeColors) if (!candidateColors.Contains(c)) candidateColors.Add(c);
                candidateColors.Add(ColorManager.GetRandomColor(new List<Color>() { stripeColors[NumStripes / 2] }));
                if (candidateColors.Contains(stripeColors[NumStripes / 2])) candidateColors.Remove(stripeColors[NumStripes / 2]);
                Color symbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                Color symbolSecondaryColor = ColorManager.GetSecondaryColor(symbolColor, FlagColors);

                float midStripeWidth = stripeSizes[(NumStripes / 2)];
                float minSymbolRelSize = Math.Min(midStripeWidth, 0.1f);
                float maxSymbolRelSize = Math.Min(midStripeWidth, 1f / (numSymbols + 1));
                float symbolRelSize = RandomRange(minSymbolRelSize, maxSymbolRelSize);

                Symbol symbol = GetRandomSymbol();
                float relStepSize = 1f / (numSymbols + 1);
                for (int i = 0; i < numSymbols; i++)
                {
                    Vector2 position = new Vector2(StripeDirection == StripeDirectionType.Horizontal ? (i + 1) * relStepSize * FlagWidth : FlagCenter.X, StripeDirection == StripeDirectionType.Horizontal ? FlagCenter.Y : (i + 1) * relStepSize * FlagHeight);
                    symbol.Draw(Svg, position, StripeDirection == StripeDirectionType.Horizontal ? symbolRelSize * FlagHeight : symbolRelSize * FlagHeight, 0, symbolColor, symbolSecondaryColor);
                }

            }

            else if (R.NextDouble() < COA_CHANCE) // Coat of arms
            {
                CoatOfArmsPosition = FlagCenter;
                float coaSizeRel = EvenStripes ? stripeSizes[0] * 2 : stripeSizes[NumStripes / 2];
                CoatOfArmsSize = StripeDirection == StripeDirectionType.Horizontal ? coaSizeRel * FlagHeight : coaSizeRel * FlagWidth;
                CoatOfArmsSize = Math.Min(FlagHeight, CoatOfArmsSize);
                CoatOfArmsSize *= 0.9f;

                List<Color> forbiddenCoaColors = EvenStripes ? new List<Color>() { stripeColors[NumStripes / 2 - 1], stripeColors[NumStripes / 2] } : new List<Color>() { stripeColors[NumStripes / 2] };
                if (!EvenStripes && R.NextDouble() < BIG_COA_CHANCE)
                {
                    coaSizeRel = stripeSizes[(NumStripes / 2) - 1] + stripeSizes[(NumStripes / 2)] + stripeSizes[(NumStripes / 2) + 1];
                    forbiddenCoaColors.Add(stripeColors[(NumStripes / 2) - 1]);
                    forbiddenCoaColors.Add(stripeColors[(NumStripes / 2) + 1]);
                }

                List<Color> candidateColors = new List<Color>();
                foreach (Color c in stripeColors.Except(forbiddenCoaColors)) if (!candidateColors.Contains(c)) candidateColors.Add(c);
                candidateColors.Add(ColorManager.GetRandomColor(forbiddenCoaColors));
                CoatOfArmsPrimaryColor = candidateColors[R.Next(0, candidateColors.Count)];

                ApplyCoatOfArms(Svg);
            }
        }

        /// <summary>
        /// Draws an overlay with a rectangle in the top left corner (like USA, Liberia, Togo)
        /// </summary>
        private void DrawTopLeftRectOverlay(Color[] stripeColors, float[] stripeSizes)
        {
            List<Color> forbiddenColors = new List<Color>();
            for (int i = 0; i <= NumStripes / 2; i++) forbiddenColors.Add(stripeColors[i]);
            Color squareColor = ColorManager.GetRandomColor(forbiddenColors);

            float rectHeight = FlagHeight * 0.5f;
            float rectWidth = FlagWidth * 0.5f;
            if (!EvenStripes)
            {
                if (StripeDirection == StripeDirectionType.Horizontal)
                {
                    rectHeight = 0;
                    for (int i = 0; i < (stripeSizes.Length / 2) + 1; i++) rectHeight += stripeSizes[i] * FlagHeight;
                }
                else // Vertical
                {
                    rectWidth = 0;
                    for (int i = 0; i < (stripeSizes.Length / 2) + 1; i++) rectWidth += stripeSizes[i] * FlagWidth;
                }

            }
            DrawRectangle(Svg, 0, 0, rectWidth, rectHeight, squareColor);

            // Coa (33% symbols, 33% coa, 33% nothing)
            float coaRng = (float)R.NextDouble();

            if (coaRng < 0.33f)
            {
                List<Color> candidateColors = new List<Color>();
                foreach (Color c in stripeColors) if (!candidateColors.Contains(c)) candidateColors.Add(c);
                candidateColors.Add(ColorManager.GetRandomColor(new List<Color>() { squareColor }));
                Color symbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                FillRectangleWithSymbols(Svg, symbolColor, new Vector2(0, 0), rectWidth, rectHeight);
            }
            else if (coaRng < 0.66f)
            {
                CoatOfArmsPosition = new Vector2(rectWidth * 0.5f, rectHeight * 0.5f);
                float minCoaSize = rectHeight * 0.5f;
                float maxCoaSize = rectHeight;
                CoatOfArmsSize = RandomRange(minCoaSize, maxCoaSize);
                List<Color> candidateColors = new List<Color>();
                foreach (Color c in stripeColors) if (!candidateColors.Contains(c)) candidateColors.Add(c);
                candidateColors.Add(ColorManager.GetRandomColor(new List<Color>() { squareColor }));
                CoatOfArmsPrimaryColor = candidateColors[R.Next(0, candidateColors.Count)];

                ApplyCoatOfArms(Svg);
            }
        }

        /// <summary>
        /// Draws an overlay with a triangle on the left edge (like Cuba, Czechia, East Timor)
        /// </summary>
        private void DrawLeftTriangleOverlay(Color[] stripeColors, float[] stripeSizes)
        {
            Color triangleColor = ColorManager.GetRandomColor(stripeColors.ToList());

            float triangleWidth = RandomRange(LEFT_TRIANGLE_MIN_WIDTH, LEFT_TRIANGLE_MAX_WIDTH);
            if (R.NextDouble() < LEFT_TRIANGLE_FULL_WIDTH_CHANCE) triangleWidth = 1f;

            Vector2[] vertices = new Vector2[]
            {
                    new Vector2(0,0), new Vector2(triangleWidth * FlagWidth, FlagHeight/2), new Vector2(0, FlagHeight)
            };
            DrawPolygon(Svg, vertices, triangleColor);

            float coaXPos = (triangleWidth * 0.35f) * FlagWidth; // Set coa position

            // Inner triangle
            if (R.NextDouble() < LEFT_INNER_TRIANGLE_CHANCE)
            {
                float innerTriangleMinWidth = triangleWidth * 0.5f;
                float innerTriangleMaxWidth = triangleWidth * 0.9f;
                float innerTriangleWidth = RandomRange(innerTriangleMinWidth, innerTriangleMaxWidth);
                Vector2[] innerVertices = new Vector2[]
                {
                    new Vector2(0,0), new Vector2(innerTriangleWidth * FlagWidth, FlagHeight/2), new Vector2(0, FlagHeight)
                };

                Color innerColor = ColorManager.GetSecondaryColor(triangleColor, FlagColors);
                if (R.NextDouble() < LEFT_INNER_TRIANGLE_UNIQUE_COLOR_CHANCE) innerColor = ColorManager.GetRandomColor(FlagColors);

                DrawPolygon(Svg, innerVertices, innerColor);

                coaXPos = (innerTriangleWidth * 0.35f) * FlagWidth; // Adjust coa position
            }

            // Coa
            if (R.NextDouble() < COA_CHANCE)
            {
                CoatOfArmsPosition = new Vector2(coaXPos, FlagHeight / 2);
                float maxCoaSize = Math.Min(FlagHeight * 0.5f, coaXPos * 1.8f);
                float minCoaSize = maxCoaSize * 0.6f;
                CoatOfArmsSize = RandomRange(minCoaSize, maxCoaSize);

                CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(new List<Color>() { triangleColor });

                ApplyCoatOfArms(Svg);
            }
        }

        /// <summary>
        /// Draws an overlay consisting of two triangles in the lower corners (like Antigua and Barbuda)
        /// </summary>
        private void DrawAntiguaOverlay(Color[] stripeColors, float[] stripeSizes)
        {
            Color overlayColor = ColorManager.GetRandomColor(stripeColors.ToList());
            Vector2[] triangle1 = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth / 2, FlagHeight), new Vector2(0, FlagHeight) };
            Vector2[] triangle2 = new Vector2[] { new Vector2(FlagWidth, 0), new Vector2(FlagWidth / 2, FlagHeight), new Vector2(FlagWidth, FlagHeight) };
            DrawPolygon(Svg, triangle1, overlayColor);
            DrawPolygon(Svg, triangle2, overlayColor);

            // Coa
            if (R.NextDouble() < COA_CHANCE)
            {
                float height = RandomRange(0.2f, 0.4f);
                CoatOfArmsPosition = new Vector2(FlagWidth / 2, height * FlagHeight);
                float minCoaSize = 0.2f * FlagHeight;
                float maxCoaSize = Math.Min(0.5f, height * 2f) * FlagHeight;
                CoatOfArmsSize = RandomRange(minCoaSize, maxCoaSize);

                CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(stripeColors.ToList());

                ApplyCoatOfArms(Svg);
            }
        }

        #endregion

        #region Getters and Helpers

        private int GetNumStripes()
        {
            int probabilitySum = NumStripesDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<int, int> kvp in NumStripesDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return 0;
        }

        private StripeDirectionType GetStripeDirection()
        {
            int probabilitySum = StripeDirections.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<StripeDirectionType, int> kvp in StripeDirections)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return StripeDirectionType.Horizontal;
        }

        private OverlayType GetOverlayType()
        {
            Dictionary<OverlayType, int> overlayCandidates = Overlays.Where(x => CanApplyOverlayType(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            int probabilitySum = overlayCandidates.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<OverlayType, int> kvp in overlayCandidates)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return OverlayType.None;
        }
        private bool CanApplyOverlayType(OverlayType type)
        {
            switch(type)
            {
                case OverlayType.None:
                    return true;

                case OverlayType.LeftTriangle:
                    return StripeDirection == StripeDirectionType.Horizontal;

                case OverlayType.TopLeftRect:
                    return !HasWideMidStripe && !(StripeDirection == StripeDirectionType.Vertical && NumStripes == 3);

                case OverlayType.Antigua:
                    return true;

                default:
                    throw new Exception("Overlaytype not handled");
            }
        }

        #endregion
    }
}
