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
            { new PrefabCoa(), 100 },
            { new SingleSymbolCoa(), 100 },
            { new FramedCoa(), 50 },
            { new SymbolCircleCoa(), 60 },
        };
        protected Dictionary<string, int> Symbols = new Dictionary<string, int>()
        {
            { "DefaultStar", 80 },
            { "Circle", 40 },
            { "SpecialStar", 60 },
        };

        // Flag Attributes
        protected Random R;
        public ColorManager ColorManager;
        protected const float FlagWidth = FlagGenerator.FLAG_WIDTH;
        protected const float FlagHeight = FlagGenerator.FLAG_HEIGHT;
        protected static Vector2 FlagCenter = new Vector2(FlagWidth / 2f, FlagHeight / 2f);

        // Coat of Arms
        protected float CoatOfArmsChance;       // (0-1)
        protected Color CoatOfArmsColor;
        protected Vector2 CoatOfArmsPosition;    // Absolute position
        protected float CoatOfArmsSize;         // Absolute size

        public abstract void Apply(SvgDocument SvgDocument, Random r);

        protected void ApplyCoatOfArms(SvgDocument SvgDocument)
        {
            if (R.NextDouble() < CoatOfArmsChance)
            {
                CoatOfArms coa = GetRandomCoa();
                coa.Draw(SvgDocument, this, CoatOfArmsPosition, CoatOfArmsSize, CoatOfArmsColor, R);
            }
        }

        public void DrawRectangle(SvgDocument SvgDocument, float startX, float startY, float width, float height, Color c)
        {
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
            SvgCircle SvgCircle = new SvgCircle()
            {
                CenterX = center.X,
                CenterY = center.Y,
                Radius = radius,
                Fill = new SvgColourServer(c)
            };
            SvgDocument.Children.Add(SvgCircle);
        }

        public int RandomRange(int min, int max)
        {
            return R.Next(max - min) + min;
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
                            return new Default_Star(R);

                        case "Circle":
                            return new Circle(R);

                        case "SpecialStar":
                            return new Special_Star(R);
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
