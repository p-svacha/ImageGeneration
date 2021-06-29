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
    class NordicCross : FlagMainPattern
    {
        private const float MIN_CROSS_WIDTH = 0.07f; // relative to flag width
        private const float MAX_CROSS_WIDTH = 0.25f; // relative to flag width

        private const float SHIFT_CHANCE = 0.5f; // chance that the center of the cross is shifted to the left
        private const float SHIFT_FACTOR = 0.35f; // relative to width how much on the left it is (0 = full left)

        private const float INNER_CROSS_CHANCE = 0.4f;

        private const float BOTTOM_HALF_DIFFERENT_COLOR_CHANCE = 0.25f;

        private const float CENTER_CIRCLE_CHANCE = 0.35f;
        private float CenterCircleRadius;

        private const float COA_TOP_LEFT_CHANCE = 0.2f;
        private const float COA_CENTER_CHANCE = 0.5f; // chance IF it has a center circle
        private const float CORNER_SYMBOL_CHANCE = 0.2f;

        private bool IsShifted, LowerHalfDifferentColor, HasInnerCross, HasCenterCircle, HasTopLeftCoa;

        public override void Apply(SvgDocument SvgDocument, Random r)
        {
            R = r;
            ColorManager = new ColorManager(R);

            float minCoaSize = 0.5f;
            float maxCoaSize = 0.95f;
            CoatOfArmsSize = RandomRange(minCoaSize * FlagHeight, maxCoaSize * FlagHeight);
            CoatOfArmsPosition = FlagCenter;

            Color backgroundColor = ColorManager.GetRandomColor();
            AddUsedColor(backgroundColor);
            Color crossColor = ColorManager.GetRandomColor(backgroundColor);
            AddUsedColor(crossColor);
            Color innerCrossColor = ColorManager.GetRandomColor(crossColor);
            Color lowerHalfColor = ColorManager.GetRandomColor(backgroundColor, crossColor);

            // Cross width
            float crossWidth = RandomRange(MIN_CROSS_WIDTH, MAX_CROSS_WIDTH) * FlagWidth;
            float innerCrossWidth = 0f;

            // Shift
            float crossCenterX = FlagCenter.X;
            if(R.NextDouble() < SHIFT_CHANCE)
            {
                IsShifted = true;
                crossCenterX = FlagWidth * SHIFT_FACTOR;
            }
            Vector2 crossCenter = new Vector2(crossCenterX, FlagCenter.Y);
            float crossStartX = crossCenterX - crossWidth / 2;
            float crossStartY = FlagHeight / 2 - crossWidth / 2;

            // Background
            DrawRectangle(SvgDocument, 0f, 0f, FlagWidth, FlagHeight, backgroundColor);


            // Bottom half different color
            if(R.NextDouble() < BOTTOM_HALF_DIFFERENT_COLOR_CHANCE)
            {
                LowerHalfDifferentColor = true;
                AddUsedColor(lowerHalfColor);
                DrawRectangle(SvgDocument, 0, FlagHeight / 2 + crossWidth / 2, FlagWidth - (FlagWidth - crossCenterX) - crossWidth / 2, FlagHeight / 2 - crossWidth / 2, lowerHalfColor);
                DrawRectangle(SvgDocument, FlagWidth - (FlagWidth - crossCenterX) + crossWidth / 2, FlagHeight / 2 + crossWidth / 2, FlagWidth - crossCenterX - crossWidth / 2, FlagHeight / 2 - crossWidth / 2, lowerHalfColor);
            }

            // Center circle
            if(R.NextDouble() < CENTER_CIRCLE_CHANCE)
            {
                HasCenterCircle = true;
                float circleMinRadius = crossWidth * 0.9f;
                float circleMaxRadius = crossWidth * 1.25f;
                CenterCircleRadius = RandomRange(circleMinRadius, circleMaxRadius);
                DrawCircle(SvgDocument, crossCenter, CenterCircleRadius, crossColor);
            }

            // Horizontal stripe
            DrawRectangle(SvgDocument, 0, crossStartY, FlagWidth, crossWidth, crossColor);
            // Verical stripe
            DrawRectangle(SvgDocument, crossStartX, 0, crossWidth, FlagHeight, crossColor);

            // Double cross
            if (R.NextDouble() < INNER_CROSS_CHANCE)
            {
                HasInnerCross = true;
                AddUsedColor(innerCrossColor);
                float innerCrossMinWidth = crossWidth * 0.3f;
                float innerCrossMaxWidth = crossWidth * 0.8f;
                innerCrossWidth = RandomRange(innerCrossMinWidth, innerCrossMaxWidth);
                DrawRectangle(SvgDocument, 0, FlagHeight / 2 - innerCrossWidth / 2, FlagWidth, innerCrossWidth, innerCrossColor);
                DrawRectangle(SvgDocument, crossCenterX - innerCrossWidth / 2, 0, innerCrossWidth, FlagHeight, innerCrossColor);
                if (HasCenterCircle) DrawCircle(SvgDocument, crossCenter, CenterCircleRadius - ((crossWidth - innerCrossWidth) * 0.5f), innerCrossColor);
            }

            // Coat of arms
            if (R.NextDouble() < COA_TOP_LEFT_CHANCE)
            {
                HasTopLeftCoa = true;
                List<Color> candidateColors = new List<Color>() { crossColor };
                if (LowerHalfDifferentColor) candidateColors.Add(lowerHalfColor);
                if (HasInnerCross) candidateColors.Add(innerCrossColor);
                CoatOfArmsPrimaryColor = candidateColors[R.Next(0, candidateColors.Count)];
                CoatOfArmsPosition = new Vector2(crossStartX * 0.5f, crossStartY * 0.5f);
                CoatOfArmsSize = FlagHeight * 0.3f;
                ApplyCoatOfArms(SvgDocument);
            }
            else if(R.NextDouble() < COA_CENTER_CHANCE && HasCenterCircle)
            {
                CoatOfArmsPosition = crossCenter;
                float coaMinSize = CenterCircleRadius * 0.9f;
                float coaMaxSize = CenterCircleRadius * 1.7f;
                CoatOfArmsSize = RandomRange(coaMinSize, coaMaxSize);
                List<Color> candidateColors = new List<Color>() { backgroundColor };
                if (LowerHalfDifferentColor) candidateColors.Add(lowerHalfColor);
                if (HasInnerCross) candidateColors.Add(crossColor);
                CoatOfArmsPrimaryColor = candidateColors[R.Next(0, candidateColors.Count)];
                ApplyCoatOfArms(SvgDocument);
            }
            if(R.NextDouble() < CORNER_SYMBOL_CHANCE && !IsShifted && !HasTopLeftCoa)
            {
                Symbol symbol = GetRandomSymbol();

                Color topSymbolColor, botSymbolColor;
                Color topSymbolColorSecondary, botSymbolColorSecondary;

                if (!LowerHalfDifferentColor || R.NextDouble() < 0.5f)
                {
                    List<Color> candidateColors = new List<Color>() { crossColor };
                    if (HasInnerCross) candidateColors.Add(innerCrossColor);
                    topSymbolColor = candidateColors[R.Next(0, candidateColors.Count)];
                    botSymbolColor = topSymbolColor;
                    topSymbolColorSecondary = ColorManager.GetSecondaryColor(topSymbolColor, FlagColors);
                    botSymbolColorSecondary = topSymbolColorSecondary;
                }
                else
                {
                    topSymbolColor = lowerHalfColor;
                    botSymbolColor = backgroundColor;
                    topSymbolColorSecondary = backgroundColor;
                    botSymbolColorSecondary = lowerHalfColor;
                }

                float symbolMaxSize = FlagHeight * 0.5f - crossWidth * 0.5f;
                float symbolMinSize = symbolMaxSize * 0.5f;
                float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

                float left = crossStartX * 0.5f;
                float right = crossStartX + crossWidth + ((FlagWidth - crossStartX - crossWidth) * 0.5f);
                float top = crossStartY * 0.5f;
                float bottom = crossStartY + crossWidth + ((FlagHeight - crossStartY - crossWidth) * 0.5f);

                symbol.Draw(SvgDocument, new Vector2(left, top), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                symbol.Draw(SvgDocument, new Vector2(right, top), symbolSize, 0f, topSymbolColor, topSymbolColorSecondary);
                symbol.Draw(SvgDocument, new Vector2(left, bottom), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
                symbol.Draw(SvgDocument, new Vector2(right, bottom), symbolSize, 0f, botSymbolColor, botSymbolColorSecondary);
            }
        }
    }
}
