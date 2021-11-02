using Svg;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public abstract class FlagMainPattern
    {
        // General chances of coat of arms and symbol types appearing
        protected Dictionary<CoatOfArms, int> CoatOfArms = new Dictionary<CoatOfArms, int>()
        {
            { new Coa_Prefab(), 100 },
            { new Coa_SingleSymbol(), 100 },
            { new Coa_Framed(), 50 },
            { new Coa_SymbolCircle(), 60 },
        };
        protected Dictionary<string, int> Symbols = new Dictionary<string, int>() // Symbols also have to be added in GetRandomSymbol()
        {
            { "DefaultStar", 80 },
            { "Circle", 30 },
            { "SpecialStar", 60 },
            { "Cross", 20 }
        };

        // Chance of different line modifiers appearing
        protected enum LineModifierType
        {
            Rectangles,
            Spikes,
            Hook
        }
        protected Dictionary<LineModifierType, int> LineModifiers = new Dictionary<LineModifierType, int>()
        {
            { LineModifierType.Rectangles, 100 },
            { LineModifierType.Spikes, 60 },
            { LineModifierType.Hook, 80 },
        };

        // Flag Attributes
        protected SvgDocument Svg;
        protected Random R;
        public ColorManager ColorManager;
        protected const float FlagWidth = FlagGenerator.FLAG_WIDTH;
        protected const float FlagHeight = FlagGenerator.FLAG_HEIGHT;
        protected static Vector2 FlagCenter = new Vector2(FlagWidth / 2f, FlagHeight / 2f);
        protected static Vector2 FlagDimensions = new Vector2(FlagWidth, FlagHeight);

        protected List<Color> FlagColors = new List<Color>(); // Base colors of the flag, without symbols and coat of arms

        // Coat of Arms
        protected Color CoatOfArmsPrimaryColor;
        protected Vector2 CoatOfArmsPosition;    // Absolute position
        protected float CoatOfArmsSize;         // Absolute size

        public void ApplyPattern(SvgDocument SvgDocument, Random r)
        {
            Svg = SvgDocument;
            R = r;
            ColorManager = new ColorManager(R);
            FlagColors.Clear();
            DoApply();
        }

        public abstract void DoApply();

        protected void ApplyCoatOfArms(SvgDocument SvgDocument)
        {
            CoatOfArms coa = GetRandomCoa();
            coa.Draw(SvgDocument, this, R, CoatOfArmsPosition, CoatOfArmsSize, CoatOfArmsPrimaryColor, FlagColors);
        }

        public void DrawRectangle(SvgDocument SvgDocument, float startX, float startY, float width, float height, Color c)
        {
            AddFlagColor(c);
            SvgRectangle svgRectangle = new SvgRectangle()
            {
                X = startX,
                Y = startY,
                Width = width,
                Height = height,
                Fill = new SvgColourServer(c)
            };
            SvgDocument.Children.Add(svgRectangle);
        }

        public void DrawPolygon(SvgDocument SvgDocument, Vector2[] vertices, Color c)
        {
            AddFlagColor(c);
            SvgPointCollection points = new SvgPointCollection();
            foreach (Vector2 p in vertices)
            {
                points.Add(new SvgUnit(p.X));
                points.Add(new SvgUnit(p.Y));
            }

            SvgPolygon SvgPolygon = new SvgPolygon()
            {
                Points = points,
                Fill = new SvgColourServer(c),
                StrokeWidth = 0
            };
            SvgDocument.Children.Add(SvgPolygon);
        }

        public void DrawCircle(SvgDocument SvgDocument, Vector2 center, float radius, Color c)
        {
            AddFlagColor(c);
            SvgCircle SvgCircle = new SvgCircle()
            {
                CenterX = center.X,
                CenterY = center.Y,
                Radius = radius,
                Fill = new SvgColourServer(c)
            };
            SvgDocument.Children.Add(SvgCircle);
        }

        protected const float MIN_FILLED_SYMBOL_SIZE = FlagHeight * 0.05f;
        /// <summary>
        /// Fills a rectangle with apprpriately sized symbols
        /// </summary>
        protected void FillRectangleWithSymbols(SvgDocument Svg, Color color, Vector2 topLeft, float width, float height)
        {
            Symbol symbol = GetRandomSymbol();
            Color secondaryColor = ColorManager.GetSecondaryColor(color, FlagColors);

            float maxSymbolSize = Math.Min(width, height);
            if (maxSymbolSize < MIN_FILLED_SYMBOL_SIZE) return;

            float symbolSize = RandomRange(MIN_FILLED_SYMBOL_SIZE, maxSymbolSize);
            int rows = (int)(height / symbolSize);
            float totalRowMargin = height - rows * symbolSize;
            float rowStep = totalRowMargin / (rows + 1);
            
            int cols = (int)(width / symbolSize);
            float totalColMargin = width - cols * symbolSize;
            float colStep = totalColMargin / (cols + 1);

            for(int y = 0; y < rows; y++)
            {
                float yPos = rowStep + y * (rowStep + symbolSize);

                for(int x = 0; x < cols; x++)
                {
                    float xPos = colStep + x * (colStep + symbolSize);
                    Vector2 symbolPos = new Vector2(xPos + symbolSize * 0.5f, yPos + symbolSize * 0.5f);
                    symbol.Draw(Svg, symbolPos, symbolSize * 0.8f, 0f, color, secondaryColor);
                }
            }

        }

        /// <summary>
        /// Modifies a line from startPoint to endPoint with a given thickness with a certain pattern that extends the thickness of the line by modifierWidth
        /// </summary>
        protected void EnhanceLine(SvgDocument Svg, Vector2 startPoint, Vector2 endPoint, float lineThickness, LineModifierType modifierType, int numRepeats, float modifierWidth, Color lineColor)
        {
            Vector2 line = endPoint - startPoint;
            float lineLength = line.Length();
            float step = lineLength / (numRepeats * 2 + 1);
            for (int i = 0; i < numRepeats; i++)
            {
                Vector2 patternStartMidPoint = startPoint + line / lineLength * ((2 * i + 1) * step);
                Vector2 patternEndMidPoint = startPoint + line / lineLength * ((2 * i + 2) * step);
                Vector2 patternMidPoint = (patternStartMidPoint + patternEndMidPoint) / 2;
                if (modifierType == LineModifierType.Rectangles)
                {
                    Vector2[] vertices =
                    {
                        patternStartMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        patternEndMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        patternEndMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        patternStartMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                    };
                    DrawPolygon(Svg, vertices, lineColor);
                }
                else if(modifierType == LineModifierType.Spikes)
                {
                    Vector2[] vertices =
                    {
                        patternStartMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                        patternMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth * 1.5f),
                        patternEndMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                        patternEndMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                        patternMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth * 1.5f),
                        patternStartMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                    };
                    DrawPolygon(Svg, vertices, lineColor);
                }
                else if (modifierType == LineModifierType.Hook)
                {
                    float offset = 30;
                    Vector2 hookMidStart = patternStartMidPoint - Vector2.Normalize(line) * offset;
                    Vector2 hookMidEnd = patternEndMidPoint - Vector2.Normalize(line) * offset;
                    Vector2[] vertices =
                    {
                        patternStartMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                        hookMidStart + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        hookMidEnd + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        patternEndMidPoint + Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                        patternEndMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                        hookMidEnd - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        hookMidStart - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2 + modifierWidth),
                        patternStartMidPoint - Vector2.Normalize(new Vector2(line.Y, -line.X)) * (lineThickness / 2),
                    };
                    DrawPolygon(Svg, vertices, lineColor);
                }
            }
        }

        private void AddFlagColor(Color c)
        {
            if (!FlagColors.Contains(c)) FlagColors.Add(c);
        }

        public int RandomRange(int min, int max)
        {
            return R.Next(max - min + 1) + min;
        }
        public float RandomRange(float min, float max)
        {
            return (float)R.NextDouble() * (max - min) + min;
        }

        public CoatOfArms GetRandomCoa()
        {
            int probabilitySum = CoatOfArms.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<CoatOfArms, int> kvp in CoatOfArms)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return null;
        }

        public Symbol GetRandomSymbol()
        {
            int probabilitySum = Symbols.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<string, int> kvp in Symbols)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum)
                {
                    switch(kvp.Key)
                    {
                        case "DefaultStar":
                            return new Symbol_Default_Star(this, R);

                        case "Circle":
                            return new Symbol_Circle(this, R);

                        case "SpecialStar":
                            return new Symbol_Special_Star(this, R);

                        case "Cross":
                            return new Symbol_Cross(this, R);
                    }
                }
            }
            return null;
        }


        protected T GetWeightedRandomEnum<T>(Dictionary<T, int> weightDictionary) where T : System.Enum
        {
            int probabilitySum = weightDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<T, int> kvp in weightDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            throw new Exception();
        }

        protected int GetWeightedRandomInt(Dictionary<int, int> weightDictionary)
        {
            int probabilitySum = weightDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<int, int> kvp in weightDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            throw new Exception();
        }

        public float DegToRad(float angle)
        {
            return (float)((Math.PI / 180f) * angle);
        }
        public Vector2 GetPointOnCircle(Vector2 center, float radius, float angle)
        {
            float x = (float)(center.X + Math.Sin(DegToRad(angle)) * radius);
            float y = (float)(center.Y + Math.Cos(DegToRad(angle)) * radius);
            return new Vector2(x, y);
        }
        public float ClosestDistanceToFlagEdge(Vector2 p)
        {
            float min = p.X;
            if (FlagWidth - p.X < min) min = FlagWidth - p.X;
            if (p.Y < min) min = p.Y;
            if (FlagHeight - p.Y < min) min = FlagHeight - p.Y;
            return min;
        }
    }
}
