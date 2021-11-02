using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public class Pattern_Checkers : FlagMainPattern
    {
        private const float SYMMETRICAL_TILES_CHANCE = 0.9f; // Chance that there are the same amount of x and y tiles
        private const float DIAGONAL_SHIFT_CHANCE = 0.5f;

        Color CheckersColor1, CheckersColor2, CheckersColor3;

        private enum Axis
        {
            Straight,
            Diagonal
        }
        private Dictionary<Axis, int> AxisChances = new Dictionary<Axis, int>()
        {
            {Axis.Straight, 1 },
            {Axis.Diagonal, 1 }
        };

        private Dictionary<int, int> NumTiles = new Dictionary<int, int>()
        {
            {2, 300 },
            {3, 200 },
            {4, 50 },
            {5, 80 },
        };

        private enum SpecialShift
        {
            None,
            XAxis,
            YAxis
        }
        private Dictionary<SpecialShift, int> SpecialShifts = new Dictionary<SpecialShift, int>()
        {
            {SpecialShift.None, 90 },
            {SpecialShift.XAxis, 5 },
            {SpecialShift.YAxis, 5 }
        };

        private enum ColorScheme
        {
            Alternating,
            AlternatingPlus,
        }
        private Dictionary<ColorScheme, int> ColorSchemes = new Dictionary<ColorScheme, int>()
        {
            {ColorScheme.Alternating, 75 },
            {ColorScheme.AlternatingPlus, 25 }
        };

        private enum SymbolPattern
        {
            None,
            Corners,
            Alternating,
            EveryTile,
            Center
        }
        private Dictionary<SymbolPattern, int> SymbolPatterns = new Dictionary<SymbolPattern, int>()
        {
            {SymbolPattern.None, 50 },
            {SymbolPattern.Corners, 25 },
            {SymbolPattern.Alternating, 15 },
            {SymbolPattern.EveryTile, 15 },
            {SymbolPattern.Center, 50 }
        };

        public override void DoApply()
        {
            CheckersColor1 = ColorManager.GetRandomColor();
            CheckersColor2 = ColorManager.GetRandomColor(CheckersColor1);
            CheckersColor3 = ColorManager.GetRandomColor(CheckersColor1, CheckersColor2);
            DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight, CheckersColor1);

            Axis axis = GetWeightedRandomEnum(AxisChances);

            bool isSymmetrical = R.NextDouble() < SYMMETRICAL_TILES_CHANCE;
            int numTilesX = GetWeightedRandomInt(NumTiles);
            int numTilesY = isSymmetrical ? numTilesX : GetWeightedRandomInt(NumTiles);

            SpecialShift specialShift = GetWeightedRandomEnum(SpecialShifts);
            ColorScheme colorScheme = GetWeightedRandomEnum(ColorSchemes);
            if (colorScheme == ColorScheme.AlternatingPlus) specialShift = SpecialShift.None;
            SymbolPattern symbolPattern = GetWeightedRandomEnum(SymbolPatterns);

            switch (axis)
            {
                case Axis.Straight:
                    DrawStraightSymbols(numTilesX, numTilesY, colorScheme, specialShift, symbolPattern);
                    break;

                case Axis.Diagonal:
                    DrawDiagonalSymbols(numTilesX, numTilesY, colorScheme, specialShift, symbolPattern);
                    break;
            }

        }

        /// <summary>
        /// Draws symbols on a straight checker pattern according to the given modifiers
        /// </summary>
        private void DrawStraightSymbols(int numTilesX, int numTilesY, ColorScheme colorScheme, SpecialShift specialShift, SymbolPattern symbolPattern)
        {
            float tileWidth = FlagWidth / numTilesX;
            float tileHeight = FlagHeight / numTilesY;
            for (int y = 0; y < numTilesY + 1; y++)
            {
                for (int x = 0; x < numTilesX + 1; x++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        Color color = CheckersColor2;
                        if (colorScheme == ColorScheme.AlternatingPlus && y % 2 == 1) color = CheckersColor3;
                        float xPos = x * tileWidth;
                        if (specialShift == SpecialShift.XAxis && y % 2 == 1 && numTilesY > 2) xPos -= tileWidth / 2;
                        float yPos = y * tileHeight;
                        if (specialShift == SpecialShift.YAxis && x % 2 == 1 && numTilesX > 2) yPos -= tileHeight / 2;
                        DrawRectangle(Svg, xPos, yPos, tileWidth, tileHeight, color);
                    }
                }
            }

            if (specialShift != SpecialShift.None) return; // Don't draw symbols on special shifted patterns

            Symbol symbol = GetRandomSymbol();
            float symbolMaxSize = Math.Min(tileWidth, tileHeight) * 0.95f;
            float symbolMinSize = symbolMaxSize * 0.6f;
            float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

            switch (symbolPattern)
            {
                case SymbolPattern.Center:

                    int xCoordCenter = numTilesX / 2;
                    int yCoordCenter = numTilesY / 2;
                    bool hasSingleCenter = (numTilesX % 2 == 1 && numTilesY == 1);
                    if (hasSingleCenter) // If there is only one center checker field, we take a coa instead of symbol
                    {
                        CoatOfArms coa = GetRandomCoa();
                        DrawStraightCoaOnCheckerField(coa, symbolSize, tileWidth, tileHeight, xCoordCenter, yCoordCenter);
                    }
                    else
                    {
                        DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, xCoordCenter, yCoordCenter);
                        if (numTilesX % 2 == 0) DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, xCoordCenter - 1, yCoordCenter);
                        if (numTilesY % 2 == 0) DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, xCoordCenter, yCoordCenter - 1);
                        if (numTilesX % 2 == 0 && numTilesY % 2 == 0) DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, xCoordCenter - 1, yCoordCenter - 1);
                    }
                    break;

                case SymbolPattern.Corners:
                    DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, 0, 0);
                    DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, numTilesX - 1, 0);
                    DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, 0, numTilesY - 1);
                    DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, numTilesX - 1, numTilesY - 1);
                    break;

                case SymbolPattern.Alternating:
                    int alternatePattern = R.Next(0, 2);
                    for (int y = 0; y < numTilesY + 1; y++)
                    {
                        for (int x = 0; x < numTilesX + 1; x++)
                        {
                            if ((x + y) % 2 == alternatePattern)
                            {
                                DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, x, y);
                            }
                        }
                    }
                    break;

                case SymbolPattern.EveryTile:
                    for (int y = 0; y < numTilesY + 1; y++)
                    {
                        for (int x = 0; x < numTilesX + 1; x++)
                        {
                            DrawStraightSymbolOnCheckerField(symbol, symbolSize, tileWidth, tileHeight, x, y);
                        }
                    }
                    break;

            }
        }

        /// <summary>
        /// Draws symbols on a diagonal checker pattern according to the given modifiers
        /// </summary>
        private void DrawDiagonalSymbols(int numTilesX, int numTilesY, ColorScheme colorScheme, SpecialShift specialShift, SymbolPattern symbolPattern)
        {
            bool isShifted = R.NextDouble() < DIAGONAL_SHIFT_CHANCE;

            float diamondWidth = FlagWidth / (numTilesX - 1);
            float diamondHeight = FlagHeight / (numTilesY - 1);
            for (int y = 0; y < numTilesY; y++)
            {
                for (int x = 0; x < numTilesX; x++)
                {
                    Color color = CheckersColor2;
                    if (colorScheme == ColorScheme.AlternatingPlus && (x + y) % 2 == 0) color = CheckersColor3;

                    Vector2 center = GetDiagonalCheckerPosition(x, y, diamondWidth, diamondHeight, false, isShifted, specialShift);

                    Vector2[] vertices = new Vector2[] {
                                new Vector2(center.X - diamondWidth / 2, center.Y),
                                new Vector2(center.X, center.Y - diamondHeight / 2),
                                new Vector2(center.X + diamondWidth / 2, center.Y),
                                new Vector2(center.X, center.Y + diamondHeight / 2)
                            };
                    DrawPolygon(Svg, vertices, color);

                }
            }

            if (specialShift != SpecialShift.None) return; // Don't draw symbols on special shifted patterns

            Symbol symbol = GetRandomSymbol();

            float symbolMaxSize = Geometry.GetTriangleHeight(new Vector2(diamondWidth, 0), new Vector2(0, diamondHeight), new Vector2(0, 0));
            float symbolMinSize = symbolMaxSize * 0.4f;
            float symbolSize = RandomRange(symbolMinSize, symbolMaxSize);

            switch (symbolPattern)
            {
                case SymbolPattern.Center:
                    int xCoordCenter = numTilesX / 2;
                    int yCoordCenter = numTilesY / 2;
                    bool singleCenter = (isShifted && (numTilesX % 2 == numTilesY % 2)) || (!isShifted && (numTilesX % 2 != numTilesY % 2)); // (((numTilesX % 2 == 1 && isShifted) || (numTilesX % 2 == 0 && !isShifted)) && ((numTilesY % 2 == 1 && isShifted) || (numTilesY % 2 == 0 && !isShifted)));
                    if (singleCenter) // If there is only one center checker field, we take a coa instead of symbol
                    {
                        bool halfShift = numTilesX % 2 == 0;

                        if (!isShifted && (numTilesX % 2 != numTilesY % 2))
                        {
                            if (numTilesX % 2 == 0)
                            {
                                xCoordCenter--;
                                halfShift = false;
                            }
                            else
                            {
                                halfShift = true;
                            }
                        }

                        CoatOfArms coa = GetRandomCoa();
                        DrawDiagonalCoaOnCheckerField(coa, symbolSize, diamondWidth, diamondHeight, xCoordCenter, yCoordCenter, halfShift, isShifted);
                    }
                    else if (numTilesX > 2 && numTilesY > 2)
                    {
                        if (numTilesX % 2 == 0)
                        {
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter, yCoordCenter, true, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter - 1, yCoordCenter, true, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter - 1, yCoordCenter - 1, false, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter - 1, yCoordCenter, false, isShifted);
                        }
                        else
                        {
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter, yCoordCenter, true, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter, yCoordCenter + 1, true, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter, yCoordCenter, false, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, xCoordCenter - 1, yCoordCenter, false, isShifted);
                        }

                    }
                    break;

                case SymbolPattern.Corners:
                    if (!isShifted || numTilesX <= 2 || numTilesY <= 2) break;
                    DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, 1, 1, true, isShifted);
                    DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, numTilesX - 1, 1, true, isShifted);
                    DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, 1, numTilesY - 1, true, isShifted);
                    DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, numTilesX - 1, numTilesY - 1, true, isShifted);
                    break;

                case SymbolPattern.Alternating:
                    bool fullAlternating = R.NextDouble() < 0.5f; // Full alternating means every second tile gets a symbol, otherwise only every second of each color gets a symbol
                    if(fullAlternating)
                    {
                        bool halfShift = R.NextDouble() < 0.5f;
                        for (int y = 0; y < numTilesY; y++)
                        {
                            for (int x = 0; x < numTilesX; x++)
                            {
                                DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, x, y, halfShift, isShifted);
                            }
                        }
                    }
                    else
                    {
                        int alternatePattern = R.Next(0, 2);
                        int startX = isShifted ? 1 : 0;
                        for (int y = 1; y < numTilesY - 1; y++)
                        {
                            for (int x = startX; x < numTilesX - 1; x++)
                            {
                                if ((x + y) % 2 == alternatePattern)
                                {
                                    DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, x, y, false, isShifted);
                                }
                            }
                        }
                    }
                    break;

                case SymbolPattern.EveryTile:
                    for (int y = 0; y < numTilesY; y++)
                    {
                        for (int x = 0; x < numTilesX; x++)
                        {
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, x, y, true, isShifted);
                            DrawDiagonalSymbolOnCheckerField(symbol, symbolSize, diamondWidth, diamondHeight, x, y, false, isShifted);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws a symbol on a straight checker field at the given coordinates
        /// </summary>
        private void DrawStraightSymbolOnCheckerField(Symbol symbol, float symbolSize, float tileWidth, float tileHeight, int xCoord, int yCoord)
        {
            float xPos = xCoord * tileWidth + tileWidth / 2;
            float yPos = yCoord * tileHeight + tileHeight / 2;
            Color symbolColor = ((xCoord + yCoord) % 2 == 0) ? CheckersColor1 : CheckersColor2;
            Color symbolColor2 = symbolColor == CheckersColor1 ? CheckersColor2 : CheckersColor1;
            symbol.Draw(Svg, new Vector2(xPos, yPos), symbolSize, 0, symbolColor, symbolColor2);
        }

        /// <summary>
        /// Draws a coat of arms on a straight checker field at the given coordinates
        /// </summary>
        private void DrawStraightCoaOnCheckerField(CoatOfArms coa, float coaSize, float tileWidth, float tileHeight, int xCoord, int yCoord)
        {
            float xPos = xCoord * tileWidth + tileWidth / 2;
            float yPos = yCoord * tileHeight + tileHeight / 2;
            Color coaColor = ((xCoord + yCoord) % 2 == 0) ? CheckersColor1 : CheckersColor2;
            List<Color> otherColors = new List<Color>() { CheckersColor1, CheckersColor2, CheckersColor3 };
            otherColors.Remove(coaColor);
            coa.Draw(Svg, this, R, new Vector2(xPos, yPos), coaSize, coaColor, otherColors);
        }

        /// <summary>
        /// Draws a symbol on a diagonal checker field at the given coordinates
        /// </summary>
        private void DrawDiagonalSymbolOnCheckerField(Symbol symbol, float symbolSize, float diamondWidth, float diamondHeight, int xCoord, int yCoord, bool halfShift, bool isShifted)
        {
            Vector2 center = GetDiagonalCheckerPosition(xCoord, yCoord, diamondWidth, diamondHeight, halfShift, isShifted, SpecialShift.None);
            Color symbolColor = halfShift ? CheckersColor2 : CheckersColor1;
            Color symbolColor2 = symbolColor == CheckersColor1 ? CheckersColor2 : CheckersColor1;
            symbol.Draw(Svg, new Vector2(center.X, center.Y), symbolSize, 0, symbolColor, symbolColor2);
        }

        /// <summary>
        /// Draws a coat of arms on a diagonal checker field at the given coordinates
        /// </summary>
        private void DrawDiagonalCoaOnCheckerField(CoatOfArms coa, float coaSize, float diamondWidth, float diamondHeight, int xCoord, int yCoord, bool halfShift, bool isShifted)
        {
            Vector2 center = GetDiagonalCheckerPosition(xCoord, yCoord, diamondWidth, diamondHeight, halfShift, isShifted, SpecialShift.None);
            Color coaColor = halfShift ? CheckersColor2 : CheckersColor1;
            List<Color> otherColors = new List<Color>() { CheckersColor1, CheckersColor2, CheckersColor3 };
            otherColors.Remove(coaColor);
            coa.Draw(Svg, this, R, new Vector2(center.X, center.Y), coaSize, coaColor, otherColors);
        }

        /// <summary>
        /// Returns the position of the center of a diagonal checker field given its coordinates and modifiers
        /// </summary>
        private Vector2 GetDiagonalCheckerPosition(int x, int y, float diamondWidth, float diamondHeight, bool halfShift, bool isShifted, SpecialShift specialShift)
        {
            float xPosCenter = x * diamondWidth + diamondWidth / 2;
            if (specialShift == SpecialShift.XAxis && y % 2 == 1) xPosCenter -= diamondWidth / 2;
            if (isShifted) xPosCenter -= diamondWidth / 2;

            float yPosCenter = y * diamondHeight;
            if (specialShift == SpecialShift.YAxis && x % 2 == 1) yPosCenter -= diamondHeight / 2;

            if(halfShift)
            {
                xPosCenter -= diamondWidth / 2;
                yPosCenter -= diamondHeight / 2;
            }

            return new Vector2(xPosCenter, yPosCenter);
        }
    }
}
