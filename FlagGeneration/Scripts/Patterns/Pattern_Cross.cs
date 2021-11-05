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
    class Pattern_Cross : FlagMainPattern
    {
        private const float MIN_CROSS_WIDTH = 0.07f; // relative to flag width
        private const float MAX_CROSS_WIDTH = 0.25f; // relative to flag width
        private const float DIAG_WIDTH_FACTOR = 1f; // diagonal width is smaller by this factor

        private const float INNER_CROSS_CHANCE = 0.4f;
        private const float TWO_BG_COLORS_CHANCE = 0.25f;

        private const float CENTER_CIRCLE_CHANCE = 0.35f;
        private const float CENTER_CIRCLE_MIN_RADIUS = FlagWidth * 0.1f;
        private const float CENTER_CIRCLE_MAX_RADIUS = FlagWidth * 0.3f;

        private const float CENTER_CIRCLE_COA_CHANCE = 0.5f;
        private float CenterCircleRadius;

        private const float NORDIC_SHIFT_FACTOR = 0.35f; // relative to width how much on the left it is (0 = full left)
        private const float NORDIC_CORNER_COA_CHANCE = 0.2f;

        private const float CORNER_SYMBOL_CHANCE = 0.2f;

        private const float STRAIGHT_MODIFIED_LINES_CHANCE = 0.12f;
        private const float DIAGONAL_MODIFIED_LINES_CHANCE = 0.12f;
        private const int MIN_MODIFY_REPEATS = 2;
        private const int MAX_MODIFY_REPEATS = 16;
        private const float MIN_MODIFIER_SIZE = 8;
        private const float MAX_MODIFIER_SIZE = 35;

        private Style ChosenStyle;

        private bool HasInnerCross;

        private bool HasTwoBackgroundColors;
        private bool HasCenterCircle;
        private bool HasCenterCircleCoa;

        private Vector2 CrossCenter;

        private float CrossWidthRel;
        private float CrossWidth;

        private float InnerCrossWidthRel;
        private float InnerCrossWidth;

        private float CrossWidthDiagX;
        private float CrossWidthDiagY;
        private float InnerCrossWidthDiagX;
        private float InnerCrossWidthDiagY;

        private LineModifierType LineModifier;
        private bool HasStraightModifiedLines;
        private bool HasDiagonalModifiedLines;
        private int NumModifyRepeats;
        private float ModifierSize;

        private Color BackgroundColor;
        private Color BackgroundColor2;
        private Color CrossColor;
        private Color InnerCrossColor;

        private enum Style
        {
            Nordic,
            England,
            Scotland,
            ScotlandPlusHorizontal,
            ScotlandPlusVertical,
            HorizontalStripe,
            VerticalStripe,
            DiagonalStripe1,
            DiagonalStripe2,
            UnionJack
        }
        private Dictionary<Style, int> Styles = new Dictionary<Style, int>()
        {
            {Style.Nordic, 200 },
            {Style.England, 100 },
            {Style.Scotland, 100 },
            {Style.ScotlandPlusHorizontal, 30 },
            {Style.ScotlandPlusVertical, 30 },
            {Style.HorizontalStripe, 50 },
            {Style.VerticalStripe, 20 },
            {Style.DiagonalStripe1, 30 },
            {Style.DiagonalStripe2, 30 },
            {Style.UnionJack, 100 },
        };

        public override void DoApply()
        {
            float minCoaSize = 0.5f;
            float maxCoaSize = 0.95f;
            CoatOfArmsSize = RandomRange(minCoaSize * FlagHeight, maxCoaSize * FlagHeight);
            CoatOfArmsPosition = FlagCenter;

            BackgroundColor = ColorManager.GetRandomColor();
            CrossColor = ColorManager.GetRandomColor(BackgroundColor);
            CrossWidthRel = RandomRange(MIN_CROSS_WIDTH, MAX_CROSS_WIDTH);
            CrossWidth = CrossWidthRel * FlagWidth;
            CrossWidthDiagX = FlagWidth * CrossWidthRel * DIAG_WIDTH_FACTOR;
            CrossWidthDiagY = FlagHeight * CrossWidthRel * DIAG_WIDTH_FACTOR;
            CrossCenter = FlagCenter;

            HasCenterCircle = R.NextDouble() < CENTER_CIRCLE_CHANCE;
            HasCenterCircleCoa = HasCenterCircle && R.NextDouble() < CENTER_CIRCLE_COA_CHANCE;
            HasInnerCross = R.NextDouble() < INNER_CROSS_CHANCE;
            HasTwoBackgroundColors = R.NextDouble() < TWO_BG_COLORS_CHANCE;

            HasStraightModifiedLines = R.NextDouble() < STRAIGHT_MODIFIED_LINES_CHANCE;
            HasDiagonalModifiedLines = R.NextDouble() < DIAGONAL_MODIFIED_LINES_CHANCE;
            LineModifier = GetWeightedRandomEnum<LineModifierType>(LineModifiers);
            NumModifyRepeats = RandomRange(MIN_MODIFY_REPEATS, MAX_MODIFY_REPEATS);
            ModifierSize = RandomRange(MIN_MODIFIER_SIZE, MAX_MODIFIER_SIZE);

            if (HasCenterCircle)
            {
                float minRadius = Math.Max(CENTER_CIRCLE_MIN_RADIUS, CrossWidth);
                CenterCircleRadius = RandomRange(CENTER_CIRCLE_MIN_RADIUS, CENTER_CIRCLE_MAX_RADIUS);
            }
            if(HasInnerCross)
            {
                float innerCrossMinWidth = CrossWidthRel * 0.3f;
                float innerCrossMaxWidth = CrossWidthRel * 0.8f;
                InnerCrossWidthRel = RandomRange(innerCrossMinWidth, innerCrossMaxWidth);
                InnerCrossWidth = InnerCrossWidthRel * FlagWidth;
                InnerCrossWidthDiagX = InnerCrossWidthRel * FlagWidth * DIAG_WIDTH_FACTOR;
                InnerCrossWidthDiagY = InnerCrossWidthRel * FlagHeight * DIAG_WIDTH_FACTOR;
                InnerCrossColor = ColorManager.GetRandomColor(CrossColor);
            }
            if(HasTwoBackgroundColors)
            {
                HasTwoBackgroundColors = true;
                List<Color> forbiddenColors = new List<Color>() { BackgroundColor, CrossColor };
                if (HasInnerCross) forbiddenColors.Add(InnerCrossColor);
                BackgroundColor2 = ColorManager.GetRandomColor(BackgroundColor, CrossColor);
            }

            DrawRectangle(Svg, 0f, 0f, FlagWidth, FlagHeight, BackgroundColor);

            ChosenStyle = GetWeightedRandomEnum(Styles);
            switch (ChosenStyle)
            {
                case Style.Nordic:
                    DrawNordic();
                    break;

                case Style.England:
                    DrawEngland();
                    break;

                case Style.Scotland:
                    DrawScotland();
                    break;

                case Style.ScotlandPlusHorizontal:
                    DrawScotlandPlusHorizontal();
                    break;

                case Style.ScotlandPlusVertical:
                    DrawScotlandPlusVertical();
                    break;

                case Style.HorizontalStripe:
                    DrawHorizontalStripe();
                    break;

                case Style.VerticalStripe:
                    DrawVerticalStripe();
                    break;

                case Style.DiagonalStripe1:
                    DrawDiagonalStripe1();
                    break;

                case Style.DiagonalStripe2:
                    DrawDiagonalStripe2();
                    break;

                case Style.UnionJack:
                    DrawUnionJack();
                    break;
            }

            DrawCoatOfArms();
        }

        private void DrawNordic()
        {
            CrossCenter = new Vector2(FlagWidth * NORDIC_SHIFT_FACTOR, FlagCenter.Y);

            if (HasTwoBackgroundColors)
            {
                DrawRectangle(Svg, 0, CrossCenter.Y + CrossWidth / 2, FlagWidth - (FlagWidth - CrossCenter.X) - CrossWidth / 2, FlagHeight / 2 - CrossWidth / 2, BackgroundColor2);
                DrawRectangle(Svg, FlagWidth - (FlagWidth - CrossCenter.X) + CrossWidth / 2, FlagHeight / 2 + CrossWidth / 2, FlagWidth - CrossCenter.X - CrossWidth / 2, FlagHeight / 2 - CrossWidth / 2, BackgroundColor2);
            }

            DrawOuterHorizontalLine();
            DrawOuterVerticalLine();
            DrawJaggedCenterSquare();
            DrawCenterCircle();
            DrawInnerHorizontalLine();
            DrawInnerVerticalLine();
        }
        private void DrawEngland()
        {
            bool topDownSplit = false;
            if (HasTwoBackgroundColors)
            {
                topDownSplit = R.NextDouble() < 0.5f;
                if (topDownSplit)
                {
                    DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight / 2, BackgroundColor2);
                }
                else
                {
                    DrawRectangle(Svg, 0, 0, FlagWidth / 2, FlagHeight, BackgroundColor2);
                }
            }

            DrawOuterHorizontalLine();
            DrawOuterVerticalLine();
            DrawJaggedCenterSquare();
            DrawCenterCircle();
            DrawInnerHorizontalLine();
            DrawInnerVerticalLine();

            if (R.NextDouble() < CORNER_SYMBOL_CHANCE)
            {
                Symbol symbol = GetRandomSymbol();

                Color topSymbolColor, botSymbolColor;
                Color topSymbolColorSecondary, botSymbolColorSecondary;

                if (!HasTwoBackgroundColors || R.NextDouble() < 0.5f)
                {
                    List<Color> candidateColors = new List<Color>() { CrossColor };
                    if (HasInnerCross) candidateColors.Add(InnerCrossColor);
                    topSymbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                    botSymbolColor = topSymbolColor;
                    topSymbolColorSecondary = ColorManager.GetSecondaryColor(topSymbolColor, FlagColors);
                    botSymbolColorSecondary = topSymbolColorSecondary;
                }
                else
                {
                    topSymbolColor = BackgroundColor2;
                    botSymbolColor = BackgroundColor;
                    topSymbolColorSecondary = BackgroundColor;
                    botSymbolColorSecondary = BackgroundColor2;
                }

                float symbolMaxSize = FlagHeight * 0.5f - CrossWidth * 0.5f;
                float symbolMinSize = symbolMaxSize * 0.5f;
                float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

                float left = (CrossCenter.X - CrossWidth / 2) * 0.5f;
                float right = (CrossCenter.X - CrossWidth / 2) + CrossWidth + ((FlagWidth - (CrossCenter.X - CrossWidth / 2) - CrossWidth) * 0.5f);
                float top = (CrossCenter.Y - CrossWidth / 2) * 0.5f;
                float bottom = (CrossCenter.Y - CrossWidth / 2) + CrossWidth + ((FlagHeight - (CrossCenter.Y - CrossWidth / 2) - CrossWidth) * 0.5f);

                if (topDownSplit)
                {
                    symbol.Draw(Svg, new Vector2(left, top), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                    symbol.Draw(Svg, new Vector2(right, top), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                    symbol.Draw(Svg, new Vector2(left, bottom), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
                    symbol.Draw(Svg, new Vector2(right, bottom), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
                }
                else
                {
                    symbol.Draw(Svg, new Vector2(left, top), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
                    symbol.Draw(Svg, new Vector2(right, top), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                    symbol.Draw(Svg, new Vector2(left, bottom), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
                    symbol.Draw(Svg, new Vector2(right, bottom), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                }
            }
        }
        private void DrawScotland()
        {
            if (HasTwoBackgroundColors)
            {
                Vector2[] leftTriangle = new Vector2[] { new Vector2(0, 0), FlagCenter, new Vector2(0, FlagHeight) };
                Vector2[] rightTriangle = new Vector2[] { new Vector2(FlagWidth, 0), FlagCenter, new Vector2(FlagWidth, FlagHeight) };
                DrawPolygon(Svg, leftTriangle, BackgroundColor2);
                DrawPolygon(Svg, rightTriangle, BackgroundColor2);
            }

            DrawOuterDiag1();
            DrawOuterDiag2();
            DrawCenterCircle();
            DrawInnerDiag1();
            DrawInnerDiag2();

            if (R.NextDouble() < CORNER_SYMBOL_CHANCE && !HasCenterCircle)
            {
                Symbol symbol = GetRandomSymbol();

                Color topSymbolColor, botSymbolColor;
                Color topSymbolColorSecondary, botSymbolColorSecondary;

                if (!HasTwoBackgroundColors || R.NextDouble() < 0.5f)
                {
                    List<Color> candidateColors = new List<Color>() { CrossColor };
                    if (HasInnerCross) candidateColors.Add(InnerCrossColor);
                    topSymbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                    botSymbolColor = topSymbolColor;
                    topSymbolColorSecondary = ColorManager.GetSecondaryColor(topSymbolColor, FlagColors);
                    botSymbolColorSecondary = topSymbolColorSecondary;
                }
                else
                {
                    topSymbolColor = BackgroundColor2;
                    botSymbolColor = BackgroundColor;
                    topSymbolColorSecondary = BackgroundColor;
                    botSymbolColorSecondary = BackgroundColor2;
                }

                float symbolMaxSize = FlagHeight * 0.5f - CrossWidth * 0.8f;
                float symbolMinSize = symbolMaxSize * 0.5f;
                float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

                float left = (FlagWidth / 2 - CrossWidth * 1.5f) / 2;
                float right = FlagWidth - ((FlagWidth / 2 - CrossWidth * 1.5f) / 2);
                float top = (FlagHeight / 2 - CrossWidth * 0.9f) / 2;
                float bottom = FlagHeight - (FlagHeight / 2 - CrossWidth * 0.9f) / 2;

                symbol.Draw(Svg, new Vector2(left, FlagCenter.Y), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
                symbol.Draw(Svg, new Vector2(FlagCenter.X, top), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                symbol.Draw(Svg, new Vector2(FlagCenter.X, bottom), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                symbol.Draw(Svg, new Vector2(right, FlagCenter.Y), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
            }
        }
        private void DrawScotlandPlusHorizontal()
        {
            ScaleCrossWidth(0.75f);

            if (HasTwoBackgroundColors)
            {
                Vector2[] leftTriangle = new Vector2[] { new Vector2(0, 0), FlagCenter, new Vector2(0, FlagHeight) };
                Vector2[] rightTriangle = new Vector2[] { new Vector2(FlagWidth, 0), FlagCenter, new Vector2(FlagWidth, FlagHeight) };
                DrawPolygon(Svg, leftTriangle, BackgroundColor2);
                DrawPolygon(Svg, rightTriangle, BackgroundColor2);
            }

            DrawOuterHorizontalLine();
            DrawOuterDiag1();
            DrawOuterDiag2();
            DrawCenterCircle();
            DrawInnerHorizontalLine();
            DrawInnerDiag1();
            DrawInnerDiag2();

            if (R.NextDouble() < CORNER_SYMBOL_CHANCE && !HasCenterCircle)
            {
                Symbol symbol = GetRandomSymbol();

                List<Color> candidateColors = new List<Color>() { CrossColor };
                if (HasInnerCross) candidateColors.Add(InnerCrossColor);
                Color symbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                Color symbolColorSecondary = ColorManager.GetSecondaryColor(symbolColor, FlagColors);

                float symbolMaxSize = FlagHeight * 0.5f - CrossWidth * 0.8f;
                float symbolMinSize = symbolMaxSize * 0.5f;
                float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

                float top = (FlagHeight / 2 - CrossWidth * 0.9f) / 2;
                float bottom = FlagHeight - (FlagHeight / 2 - CrossWidth * 0.9f) / 2;

                symbol.Draw(Svg, new Vector2(FlagCenter.X, top), symbolSize, 0f, symbolColor, symbolColorSecondary);
                symbol.Draw(Svg, new Vector2(FlagCenter.X, bottom), symbolSize, 0f, symbolColor, symbolColorSecondary);
            }
        }
        private void DrawScotlandPlusVertical()
        {
            ScaleCrossWidth(0.75f);

            if (HasTwoBackgroundColors)
            {
                Vector2[] topTriangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), FlagCenter };
                Vector2[] botTriangle = new Vector2[] { new Vector2(0, FlagHeight), FlagCenter, new Vector2(FlagWidth, FlagHeight) };
                DrawPolygon(Svg, topTriangle, BackgroundColor2);
                DrawPolygon(Svg, botTriangle, BackgroundColor2);
            }

            DrawOuterVerticalLine();
            DrawOuterDiag1();
            DrawOuterDiag2();
            DrawCenterCircle();
            DrawInnerVerticalLine();
            DrawInnerDiag1();
            DrawInnerDiag2();

            if (R.NextDouble() < CORNER_SYMBOL_CHANCE && !HasCenterCircle)
            {
                Symbol symbol = GetRandomSymbol();

                List<Color> candidateColors = new List<Color>() { CrossColor };
                if (HasInnerCross) candidateColors.Add(InnerCrossColor);
                Color symbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                Color symbolColorSecondary = ColorManager.GetSecondaryColor(symbolColor, FlagColors);

                float symbolMaxSize = FlagHeight * 0.5f - CrossWidth * 0.8f;
                float symbolMinSize = symbolMaxSize * 0.5f;
                float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

                float left = (FlagWidth / 2 - CrossWidth * 1.5f) / 2;
                float right = FlagWidth - ((FlagWidth / 2 - CrossWidth * 1.5f) / 2);

                symbol.Draw(Svg, new Vector2(left, FlagCenter.Y), symbolSize, 0f, symbolColor, symbolColorSecondary);
                symbol.Draw(Svg, new Vector2(right, FlagCenter.Y), symbolSize, 0f, symbolColor, symbolColorSecondary);
            }
        }
        private void DrawVerticalStripe()
        {
            if (HasTwoBackgroundColors)
            {
                if (R.NextDouble() < 0.3f) DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight / 2, BackgroundColor2);
                else DrawRectangle(Svg, 0, 0, FlagWidth / 2, FlagHeight, BackgroundColor2);
            }
            DrawOuterVerticalLine();
            DrawCenterCircle();
            DrawInnerVerticalLine();
        }
        private void DrawHorizontalStripe()
        {
            if (HasTwoBackgroundColors)
            {
                if (R.NextDouble() < 0.2f) DrawRectangle(Svg, 0, 0, FlagWidth / 2, FlagHeight, BackgroundColor2);
                else DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight / 2, BackgroundColor2);
            }
            DrawOuterHorizontalLine();
            DrawCenterCircle();
            DrawInnerHorizontalLine();
        }
        private void DrawDiagonalStripe1()
        {
            if (HasTwoBackgroundColors)
            {
                if (R.NextDouble() < 0.2f)
                {
                    Vector2[] triangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(0, FlagHeight) };
                    DrawPolygon(Svg, triangle, BackgroundColor2);
                }
                else
                {
                    Vector2[] triangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, FlagHeight) };
                    DrawPolygon(Svg, triangle, BackgroundColor2);
                }
            }
            DrawOuterDiag1();
            DrawCenterCircle();
            DrawInnerDiag1();
        }
        private void DrawDiagonalStripe2()
        {
            if (HasTwoBackgroundColors)
            {
                if (R.NextDouble() < 0.2f)
                {
                    Vector2[] triangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, FlagHeight) };
                    DrawPolygon(Svg, triangle, BackgroundColor2);
                }
                else
                {
                    Vector2[] triangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(0, FlagHeight) };
                    DrawPolygon(Svg, triangle, BackgroundColor2);
                }
            }
            DrawOuterDiag2();
            DrawCenterCircle();
            DrawInnerDiag2();
        }
        private void DrawUnionJack()
        {
            ScaleCrossWidth(0.7f);

            if (HasTwoBackgroundColors)
            {
                double ujColorRng = R.NextDouble();
                if (ujColorRng < 0.14f) // Left|Right
                {
                    DrawRectangle(Svg, 0, 0, FlagWidth / 2, FlagHeight, BackgroundColor2);
                }
                else if (ujColorRng < 0.28f) // Top|Bot
                {
                    DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight / 2, BackgroundColor2);
                }
                else if (ujColorRng < 0.42f) // Diag1
                {
                    Vector2[] triangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(0, FlagHeight) };
                    DrawPolygon(Svg, triangle, BackgroundColor2);
                }
                else if (ujColorRng < 0.57f) // Diag2
                {
                    Vector2[] triangle = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, FlagHeight) };
                    DrawPolygon(Svg, triangle, BackgroundColor2);
                }
                else if (ujColorRng < 0.71f) // Bot|Top Triangle
                {
                    Vector2[] leftTriangle = new Vector2[] { new Vector2(0, 0), FlagCenter, new Vector2(0, FlagHeight) };
                    Vector2[] rightTriangle = new Vector2[] { new Vector2(FlagWidth, 0), FlagCenter, new Vector2(FlagWidth, FlagHeight) };
                    DrawPolygon(Svg, leftTriangle, BackgroundColor2);
                    DrawPolygon(Svg, rightTriangle, BackgroundColor2);
                }
                else if (ujColorRng < 0.85f) // Corners
                {
                    DrawRectangle(Svg, 0, 0, FlagWidth / 2, FlagHeight / 2, BackgroundColor2);
                    DrawRectangle(Svg, FlagCenter.X, FlagCenter.Y, FlagWidth / 2, FlagHeight / 2, BackgroundColor2);
                }
                else // Alternating
                {
                    Vector2[] tri1 = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth / 2, 0), FlagCenter };
                    Vector2[] tri2 = new Vector2[] { new Vector2(FlagWidth, 0), new Vector2(FlagWidth, FlagHeight / 2), FlagCenter };
                    Vector2[] tri3 = new Vector2[] { new Vector2(FlagWidth, FlagHeight), new Vector2(FlagWidth / 2, FlagHeight), FlagCenter };
                    Vector2[] tri4 = new Vector2[] { new Vector2(0, FlagHeight / 2), new Vector2(0, FlagHeight), FlagCenter };
                    DrawPolygon(Svg, tri1, BackgroundColor2);
                    DrawPolygon(Svg, tri2, BackgroundColor2);
                    DrawPolygon(Svg, tri3, BackgroundColor2);
                    DrawPolygon(Svg, tri4, BackgroundColor2);
                }
            }

            double ujStyleRng = R.NextDouble();
            if (ujStyleRng < 0.6f) // all stripes have same layer
            {
                DrawOuterVerticalLine();
                DrawOuterHorizontalLine();
                DrawOuterDiag1();
                DrawOuterDiag2();
                DrawCenterCircle();
                DrawInnerVerticalLine();
                DrawInnerHorizontalLine();
                DrawInnerDiag1();
                DrawInnerDiag2();
            }
            else if (ujStyleRng < 0.8f) // vertical / horizontal in front
            {
                DrawOuterDiag1();
                DrawOuterDiag2();
                DrawInnerDiag1();
                DrawInnerDiag2();
                DrawOuterVerticalLine();
                DrawOuterHorizontalLine();
                DrawCenterCircle();
                DrawInnerVerticalLine();
                DrawInnerHorizontalLine();

            }
            else // diagonal in front
            {
                DrawOuterVerticalLine();
                DrawOuterHorizontalLine();
                DrawInnerVerticalLine();
                DrawInnerHorizontalLine();
                DrawOuterDiag1();
                DrawOuterDiag2();
                DrawCenterCircle();
                DrawInnerDiag1();
                DrawInnerDiag2();
            }
        }

        private void DrawCoatOfArms()
        {
            if (HasCenterCircleCoa)
            {
                CoatOfArmsPosition = CrossCenter;
                float coaMinSize = CenterCircleRadius * 1f;
                float coaMaxSize = CenterCircleRadius * 1.7f;
                if (HasInnerCross)
                {
                    coaMinSize = (CenterCircleRadius - (CrossWidth - InnerCrossWidth) / 2) * 1f;
                    coaMaxSize = (CenterCircleRadius - (CrossWidth - InnerCrossWidth) / 2) * 1.7f;
                }
                CoatOfArmsSize = RandomRange(coaMinSize, coaMaxSize);
                List<Color> candidateColors = new List<Color>() { BackgroundColor };
                if (HasTwoBackgroundColors) candidateColors.Add(BackgroundColor2);
                if (HasInnerCross) candidateColors.Add(CrossColor);
                if (HasInnerCross && candidateColors.Contains(InnerCrossColor)) candidateColors.Remove(InnerCrossColor);
                CoatOfArmsPrimaryColor = candidateColors[R.Next(0, candidateColors.Count)];
                ApplyCoatOfArms(Svg);
            }
        }

        private void ScaleCrossWidth(float factor)
        {
            CrossWidthRel *= factor;
            CrossWidth *= factor;
            InnerCrossWidthRel *= factor;
            InnerCrossWidth *= factor;

            CrossWidthDiagX = FlagWidth * CrossWidthRel * DIAG_WIDTH_FACTOR;
            CrossWidthDiagY = FlagHeight * CrossWidthRel * DIAG_WIDTH_FACTOR;
            InnerCrossWidthDiagX = InnerCrossWidthRel * FlagWidth * DIAG_WIDTH_FACTOR;
            InnerCrossWidthDiagY = InnerCrossWidthRel * FlagHeight * DIAG_WIDTH_FACTOR;
        }

        private void DrawOuterVerticalLine()
        {
            DrawRectangle(Svg, CrossCenter.X - CrossWidth / 2, 0, CrossWidth, FlagHeight, CrossColor);
            if (HasStraightModifiedLines)
            {
                EnhanceLine(Svg, new Vector2(CrossCenter.X, 0), CrossCenter, CrossWidth, LineModifier, (int)(Math.Round((FlagHeight / FlagWidth) * NumModifyRepeats / 2)), ModifierSize, CrossColor);
                EnhanceLine(Svg, new Vector2(CrossCenter.X, FlagHeight), CrossCenter, CrossWidth, LineModifier, (int)(Math.Round((FlagHeight / FlagWidth) * NumModifyRepeats / 2)), ModifierSize, CrossColor);
            }
        }
        private void DrawInnerVerticalLine()
        {
            if (HasInnerCross) DrawRectangle(Svg, CrossCenter.X - InnerCrossWidth / 2, 0, InnerCrossWidth, FlagHeight, InnerCrossColor);
        }

        private void DrawOuterHorizontalLine()
        {
            DrawRectangle(Svg, 0, CrossCenter.Y - CrossWidth / 2, FlagWidth, CrossWidth, CrossColor);
            if (HasStraightModifiedLines)
            {
                EnhanceLine(Svg, new Vector2(0, FlagCenter.Y), CrossCenter, CrossWidth, LineModifier, ChosenStyle == Style.Nordic ? (int)(NumModifyRepeats * NORDIC_SHIFT_FACTOR) : NumModifyRepeats / 2, ModifierSize, CrossColor);
                EnhanceLine(Svg, new Vector2(FlagWidth, FlagCenter.Y), CrossCenter, CrossWidth, LineModifier, ChosenStyle == Style.Nordic ? (int)(NumModifyRepeats * (1 - NORDIC_SHIFT_FACTOR)) : NumModifyRepeats / 2, ModifierSize, CrossColor);
            }
        }
        private void DrawInnerHorizontalLine()
        {
            if(HasInnerCross) DrawRectangle(Svg, 0, FlagHeight / 2 - InnerCrossWidth / 2, FlagWidth, InnerCrossWidth, InnerCrossColor);
        }

        private void DrawOuterDiag1()
        {
            Vector2[] cross1Vertices = new Vector2[] { new Vector2(0, 0), new Vector2(CrossWidthDiagX, 0), new Vector2(FlagWidth, FlagHeight - CrossWidthDiagY), new Vector2(FlagWidth, FlagHeight), new Vector2(FlagWidth - CrossWidthDiagX, FlagHeight), new Vector2(0, CrossWidthDiagY) };
            DrawPolygon(Svg, cross1Vertices, CrossColor);
            if (HasDiagonalModifiedLines)
            {
                EnhanceLine(Svg, Vector2.Zero, FlagCenter, CrossWidth, LineModifier, NumModifyRepeats / 2, ModifierSize, CrossColor);
                EnhanceLine(Svg, FlagDimensions, FlagCenter, CrossWidth, LineModifier, NumModifyRepeats / 2, ModifierSize, CrossColor);
            }
        }
        private void DrawInnerDiag1()
        {
            if(HasInnerCross)
            {
                Vector2[] innerCross1Vertices = new Vector2[] { new Vector2(0, 0), new Vector2(InnerCrossWidthDiagX, 0), new Vector2(FlagWidth, FlagHeight - InnerCrossWidthDiagY), new Vector2(FlagWidth, FlagHeight), new Vector2(FlagWidth - InnerCrossWidthDiagX, FlagHeight), new Vector2(0, InnerCrossWidthDiagY) };
                DrawPolygon(Svg, innerCross1Vertices, InnerCrossColor);
            }
        }
        
        private void DrawOuterDiag2()
        {
            Vector2[] cross2Vertices = new Vector2[] { new Vector2(0, FlagHeight), new Vector2(0, FlagHeight - CrossWidthDiagY), new Vector2(FlagWidth - CrossWidthDiagX, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, CrossWidthDiagY), new Vector2(CrossWidthDiagX, FlagHeight) };
            DrawPolygon(Svg, cross2Vertices, CrossColor);
            if (HasDiagonalModifiedLines)
            {
                EnhanceLine(Svg, new Vector2(0, FlagHeight), FlagCenter, CrossWidth, LineModifier, NumModifyRepeats / 2, ModifierSize * 1.25f, CrossColor);
                EnhanceLine(Svg, new Vector2(FlagWidth, 0), FlagCenter, CrossWidth, LineModifier, NumModifyRepeats / 2, ModifierSize * 1.25f, CrossColor);
            }
        }
        private void DrawInnerDiag2()
        {
            if(HasInnerCross)
            {
                Vector2[] innerCross2Vertices = new Vector2[] { new Vector2(0, FlagHeight), new Vector2(0, FlagHeight - InnerCrossWidthDiagY), new Vector2(FlagWidth - InnerCrossWidthDiagX, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, InnerCrossWidthDiagY), new Vector2(InnerCrossWidthDiagX, FlagHeight) };
                DrawPolygon(Svg, innerCross2Vertices, InnerCrossColor);
            }
        }

        private void DrawCenterCircle()
        {
            if (HasCenterCircle)
            {
                DrawCircle(Svg, CrossCenter, CenterCircleRadius, CrossColor);
                if (HasInnerCross) DrawCircle(Svg, CrossCenter, CenterCircleRadius - ((CrossWidth - InnerCrossWidth) * 0.5f), InnerCrossColor);
            }
        }
        private void DrawJaggedCenterSquare()
        {
            if (HasStraightModifiedLines && LineModifier == LineModifierType.Rectangles) DrawRectangle(Svg, CrossCenter.X - CrossWidth / 2 - ModifierSize, CrossCenter.Y - CrossWidth / 2 - ModifierSize, CrossWidth + 2 * ModifierSize, CrossWidth + 2 * ModifierSize, CrossColor);
        }
    }
}
