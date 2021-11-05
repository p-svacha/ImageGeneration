using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    /// <summary>
    /// This coat of arms consists of a single symbol that is generated within this class. The symbol is created with language rules and is not necessarily symmetric.
    /// </summary>
    public class Coa_GeneratedSymbol : CoatOfArms
    {
        private const int MIN_DEPTH = 2;
        private const int MAX_DEPTH = 5;
        
        private List<Rule> Rules = new List<Rule>()
        {
            new Rule_EmptyToCircle(),
            new Rule_EmptyToCircleEmpties(),
            new Rule_EmptyToEmptyExtend(),
            new Rule_EmptyToPolygon(),
            new Rule_EmptyToMoon(),
        };
        private List<Shape> Shapes = new List<Shape>();

        private Rule GetRandomRule(Random R, ShapeType sourceType)
        {
            Dictionary<Rule, int> candidateRules = Rules.ToDictionary(x => x, x => x.GetChanceToAppearFor(sourceType));
            if (candidateRules.Count == 0) throw new Exception("No rules found for " + sourceType.ToString());
            int probabilitySum = candidateRules.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<Rule, int> kvp in candidateRules)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return null;
        }

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float size, Color primaryColor, List<Color> flagColors = null)
        {
            Reset();

            float startAngle = R.Next(0, 4) * 90;
            Shapes.Add(new Shape_EmptyRoot(Shape.GetNextId(), Vector2.Zero, startAngle));

            int depth = Helper.RandomRange(R, MIN_DEPTH, MAX_DEPTH);
            int counter = 0;
            while((counter < 100) && (counter < depth || Shapes.All(x => x.IsEmpty) || Shapes.Any(x => x.IsEmpty && x.CanApplyRule)))
            {
                if(counter == depth - 1)
                {
                    foreach (Shape s in Shapes.Where(x => x.Type == ShapeType.Empty)) s.Type = ShapeType.EmptyLeaf;
                }
                ApplyRandomRule(Svg, flag, R, primaryColor);
                counter++;
            }

            float symbolMinX = Shapes.Where(x => !x.IsEmpty).Min(x => x.Position.X - x.Dimensions.X / 2);
            float symbolMaxX = Shapes.Where(x => !x.IsEmpty).Max(x => x.Position.X + x.Dimensions.X / 2);
            float symbolMinY = Shapes.Where(x => !x.IsEmpty).Min(x => x.Position.Y - x.Dimensions.Y / 2);
            float symbolMaxY = Shapes.Where(x => !x.IsEmpty).Max(x => x.Position.Y + x.Dimensions.Y / 2);
            float symbolSize = Math.Max(symbolMaxX - symbolMinX, symbolMaxY - symbolMinY);
            float scaleFactor = size / symbolSize;
            Vector2 symbolCenter = new Vector2((symbolMinX + symbolMaxX) / 2, (symbolMinY + symbolMaxY) / 2);

            // Console.WriteLine("Depth: " + depth + ", Center: " + symbolCenter + ", Dimensions: " + symbolMinX + "/" + symbolMinY + " => " + symbolMaxX + "/" + symbolMaxY + ": " + symbolSize);
            foreach (Shape s in Shapes.Where(x => !x.IsEmpty)) s.Draw(Svg, flag, R, pos - (symbolCenter * scaleFactor / 2), scaleFactor, primaryColor);
        }

        private void Reset()
        {
            Shapes.Clear();
        }
        private void ApplyRandomRule(SvgDocument Svg, FlagMainPattern flag, Random R, Color c)
        {
            List<int> candidateShapeIds = Shapes.Where(x => x.IsEmpty && x.CanApplyRule).Select(x => x.Id).Distinct().ToList();
            if (candidateShapeIds.Count == 0) return;
            int sourceId = candidateShapeIds[R.Next(0, candidateShapeIds.Count)];
            List<Shape> sourceShapes = Shapes.Where(x => x.Id == sourceId).ToList();
            ShapeType sourceType = sourceShapes[0].Type;

            Rule targetRule = GetRandomRule(R, sourceType);
            // Console.WriteLine("Rule chosen: " + targetRule.GetType().ToString());
            targetRule.GenerateNewRandomValues(R);
            
            List<int> nextIds = new List<int>();
            for (int i = 0; i < 10; i++) nextIds.Add(Shape.GetNextId());
            foreach (Shape targetShape in sourceShapes)
            {
                targetShape.CanApplyRule = false;
                targetRule.Apply(Svg, flag, Shapes, targetShape, c, nextIds.ToArray());
            }
        }


        public enum ShapeType
        {
            EmptyRoot,
            Empty,
            EmptyLeaf,
            Circle,
            Polygon,
            Moon
        }

        public abstract class Shape
        {
            private static int IdKey = 0;

            public int Id; // Shapes with the same id get applied the same rules
            public ShapeType Type;
            public Vector2 Position;
            public Vector2 Dimensions;
            public float Angle;
            public bool CanApplyRule;
            public bool IsEmpty;

            public Shape(int id, float angle)
            {
                Id = id;
                Angle = angle;
                CanApplyRule = true;
            }

            public abstract void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color);
            public static int GetNextId() { return IdKey++; }
        }

        public class Shape_Empty : Shape
        {
            public Shape_Empty(int id, Vector2 position, float angle) : base(id, angle)
            {
                Type = ShapeType.Empty;
                Position = position;
                Dimensions = Vector2.Zero;
                IsEmpty = true;
            }

            public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color) { }
        }

        public class Shape_EmptyRoot : Shape
        {
            public Shape_EmptyRoot(int id, Vector2 position, float angle) : base(id, angle)
            {
                Type = ShapeType.EmptyRoot;
                Position = position;
                Dimensions = Vector2.Zero;
                IsEmpty = true;
            }

            public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color) { }
        }

        public class Shape_EmptyLeaf : Shape
        {
            public Shape_EmptyLeaf(int id, Vector2 position, float angle) : base(id, angle)
            {
                Type = ShapeType.EmptyLeaf;
                Position = position;
                Dimensions = Vector2.Zero;
                IsEmpty = true;
            }

            public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color) { }
        }

        public class Shape_Circle : Shape
        {
            public Shape_Circle(int id, Vector2 position, float radius, float angle) : base(id, angle)
            {
                Type = ShapeType.Circle;
                Position = position;
                Dimensions = new Vector2(radius, radius);
                IsEmpty = false;
            }

            public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color)
            {
                flag.DrawCircle(Svg, pos + (Position * scaleFactor / 2), Dimensions.X * scaleFactor / 2, color);
            }
        }
        
        public class Shape_Polygon : Shape
        {
            private List<Vector2> Vertices;

            public Shape_Polygon(int id, Vector2 position, float angle, List<Vector2> vertices) : base(id, angle)
            {
                Type = ShapeType.Polygon;
                Vertices = vertices;
                float minX = Vertices.Min(x => x.X);
                float maxX = Vertices.Max(x => x.X);
                float minY = Vertices.Min(x => x.Y);
                float maxY = Vertices.Max(x => x.Y);
                Dimensions = new Vector2(maxX - minX, maxY - minY);
                //Vector2 localCenter = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
                //Console.WriteLine(localCenter);
                Position = position; // + localCenter;
                IsEmpty = false;
            }

            public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color)
            {
                List<Vector2> realVertices = new List<Vector2>();
                Vector2 worldPos = pos + (Position * scaleFactor) / 2;
                foreach (Vector2 v in Vertices)
                {
                    Vector2 localShiftedVertex = Position + v;
                    Vector2 scaledVertex = localShiftedVertex * scaleFactor / 2;
                    Vector2 shiftedVertex = pos + scaledVertex;
                    Vector2 rotatedVertex = Geometry.RotatePoint(shiftedVertex, worldPos, -Angle);
                    realVertices.Add(rotatedVertex);
                }
                flag.DrawPolygon(Svg, realVertices.ToArray(), color);
            }
        }

        public class Shape_Moon : Shape
        {
            private float OuterRadius;
            private float InnerRadius;
            private float InnerOffset;

            public Shape_Moon(int id, Vector2 position, float angle, float outerRadius, float innerRadius, float innerOffset) : base(id, angle)
            {
                Type = ShapeType.Polygon;
                OuterRadius = outerRadius;
                InnerRadius = innerRadius;
                InnerOffset = innerOffset;
                Position = position;
                Dimensions = new Vector2(outerRadius, outerRadius);
                IsEmpty = false;
            }

            public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float scaleFactor, Color color)
            {
                Vector2 worldPos = pos + (Position * scaleFactor / 2);
                float worldOuterRadius = OuterRadius * scaleFactor / 2;
                float worldInnerRadius = InnerRadius * scaleFactor / 2;
                float worldInnerOffset = InnerOffset * scaleFactor / 2;
                flag.DrawMoon(Svg, worldPos, worldOuterRadius, worldInnerRadius, worldInnerOffset, Angle, color);
            }
        }


        public abstract class Rule
        {
            public abstract int GetChanceToAppearFor(ShapeType type);
            public abstract void Apply(SvgDocument Svg, FlagMainPattern flag, List<Shape> shapes, Shape shape, Color c, int[] nextIds);
            public abstract void GenerateNewRandomValues(Random R);
        }

        public class Rule_EmptyToCircle : Rule
        {
            private float MIN_SIZE = 0.5f;
            private float MAX_SIZE = 2.5f;
            private float Size;
            public override int GetChanceToAppearFor(ShapeType type)
            {
                if (type == ShapeType.EmptyLeaf || type == ShapeType.Empty) return 6;
                else return 0;
            }
            public override void Apply(SvgDocument Svg, FlagMainPattern flag, List<Shape> shapes, Shape shape, Color c, int[] nextIds)
            {
                shapes.Add(new Shape_Circle(nextIds[0], shape.Position, Size, shape.Angle));
            }
            public override void GenerateNewRandomValues(Random R)
            {
                Size = Helper.RandomRange(R, MIN_SIZE, MAX_SIZE);
            }
        }
        public class Rule_EmptyToPolygon : Rule
        {
            private enum PolygonType
            {
                Square,
                Diamond,
                Triangle,
                Star
            }
            private Dictionary<PolygonType, int> PolygonTypes = new Dictionary<PolygonType, int>()
            {
                {PolygonType.Square, 10 },
                {PolygonType.Diamond, 10 },
                {PolygonType.Triangle, 5 },
                {PolygonType.Star, 8 },
            };
            private PolygonType CurrentPolygonType;

            private float MIN_SIZE = 0.5f;
            private float MAX_SIZE = 2.5f;
            private float Size;

            private float STRETCH_CHANCE = 0.25f;
            private float MIN_STRETCH_FACTOR = 2f;
            private float MAX_STRETCH_FACTOR = 5f;
            private float StretchFactor;

            private float FLIP_CHANCE = 0.5f;
            private bool IsFlipped;


            public override int GetChanceToAppearFor(ShapeType type)
            {
                if (type == ShapeType.EmptyLeaf || type == ShapeType.Empty) return 10;
                else return 0;
            }
            public override void Apply(SvgDocument Svg, FlagMainPattern flag, List<Shape> shapes, Shape shape, Color c, int[] nextIds)
            {
                List<Vector2> vertices = new List<Vector2>();
                if (CurrentPolygonType == PolygonType.Square)
                {
                    vertices = new List<Vector2>()
                    {
                        new Vector2(-Size / 2, -Size / 2),
                        new Vector2(Size / 2, -Size / 2),
                        new Vector2(Size / 2, Size / 2),
                        new Vector2(-Size / 2, Size / 2),
                    };
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        if(IsFlipped) vertices[i] *= new Vector2(StretchFactor / 2, 1);
                        else vertices[i] *= new Vector2(1, StretchFactor / 2);
                    }
                }

                if (CurrentPolygonType == PolygonType.Diamond)
                {
                    vertices = new List<Vector2>()
                    {
                        new Vector2(-Size / 2, 0),
                        new Vector2(0, -Size / 2),
                        new Vector2(Size / 2, 0),
                        new Vector2(0, Size / 2),
                    };
                    if (IsFlipped) vertices[3] *= StretchFactor;
                    else vertices[1] *= StretchFactor;
                }

                if(CurrentPolygonType == PolygonType.Triangle)
                {
                    float h = (float)(Math.Sqrt(Math.Pow(Size, 2) - Math.Pow(Size / 2, 2)));
                    vertices = new List<Vector2>()
                    {
                        new Vector2(0, - h/2),
                        new Vector2(-Size / 2, h/2),
                        new Vector2(Size / 2, h/2)
                    };
                    if (IsFlipped)
                    {
                        for (int i = 0; i < vertices.Count; i++) vertices[i] *= new Vector2(1, -1);
                    }
                    vertices[0] *= new Vector2(1, StretchFactor);
                }

                if(CurrentPolygonType == PolygonType.Star)
                {
                    vertices = Geometry.GetStarVertices(Vector2.Zero, 5, Size / 2, Size / 2 * 0.4f, IsFlipped ? 180 : 0);
                }

                shapes.Add(new Shape_Polygon(nextIds[0], shape.Position, shape.Angle, vertices));
            }
            public override void GenerateNewRandomValues(Random R)
            {
                CurrentPolygonType = Helper.GetWeightedRandomEnum<PolygonType>(R, PolygonTypes);
                Size = Helper.RandomRange(R, MIN_SIZE, MAX_SIZE);
                StretchFactor = R.NextDouble() < STRETCH_CHANCE ? Helper.RandomRange(R, MIN_STRETCH_FACTOR, MAX_STRETCH_FACTOR) : 1;
                IsFlipped = R.NextDouble() < FLIP_CHANCE;
            }
        }
        public class Rule_EmptyToMoon : Rule
        {
            private float MIN_OUTER_RADIUS = 0.5f;
            private float MAX_OUTER_RADIUS = 2.5f;
            private float OuterRadius;

            private float InnerRadius;

            private float MIN_INNER_OFFSET = 0.25f; // relative to outer radius
            private float MAX_INNER_OFFSET = 2.5f; // relative to outer radius
            private float InnerOffset;

            private const float SYMBOL_AT_INNER_CENTER_CHANCE = 0.25f;
            private bool HasSymbolAtInnerCenter;

            public override int GetChanceToAppearFor(ShapeType type)
            {
                if (type == ShapeType.EmptyLeaf || type == ShapeType.Empty) return 6;
                else return 0;
            }
            public override void Apply(SvgDocument Svg, FlagMainPattern flag, List<Shape> shapes, Shape shape, Color c, int[] nextIds)
            {
                shapes.Add(new Shape_Moon(nextIds[0], shape.Position, shape.Angle, OuterRadius, InnerRadius, InnerOffset));
                if (HasSymbolAtInnerCenter) 
                {
                    Vector2 innerCenter = Geometry.GetPointOnCircle(shape.Position, InnerOffset, shape.Angle);
                    shapes.Add(new Shape_EmptyLeaf(nextIds[1], innerCenter, shape.Angle));
                }
            }
            public override void GenerateNewRandomValues(Random R)
            {
                OuterRadius = Helper.RandomRange(R, MIN_OUTER_RADIUS, MAX_OUTER_RADIUS);
                InnerOffset = Helper.RandomRange(R, MIN_INNER_OFFSET * OuterRadius, MAX_INNER_OFFSET * OuterRadius);

                float minInnerRadius = Math.Abs(OuterRadius - InnerOffset);
                float maxInnerRadius = InnerOffset + OuterRadius;
                InnerRadius = Helper.RandomRange(R, minInnerRadius, maxInnerRadius);
                HasSymbolAtInnerCenter = R.NextDouble() < SYMBOL_AT_INNER_CENTER_CHANCE;
            }
        }

        public class Rule_EmptyToCircleEmpties : Rule
        {
            enum Modifier
            {
                None,
                OmitFirst,
                OmitHalf,
                OffsetFirst,
                OffsetAlternate,
                ReplaceOffsetFirst,
                ReplaceAlternate,
                ReplaceOffsetAlternate
            }
            private Dictionary<Modifier, int> Modifiers = new Dictionary<Modifier, int>()
            {
                {Modifier.None, 100 },
                {Modifier.OmitFirst, 15 },
                {Modifier.OmitHalf, 40 },
                {Modifier.OffsetFirst, 15 },
                {Modifier.OffsetAlternate, 50 },
                {Modifier.ReplaceAlternate, 30 },
                {Modifier.ReplaceOffsetFirst, 15 },
                {Modifier.ReplaceOffsetAlternate, 60 },
            };
            private Modifier Mod;
            private const float MOD_MIN_OFFSET_MULTIPLIER = 1.8f;
            private const float MOD_MAX_OFFSET_MULTIPLIER = 3f;
            private float ModOffsetMultiplier;
            private int ModAlternateId;

            private const int MIN_ELEMENTS = 2;
            private const int MAX_ELEMENTS = 8;
            private int NumElements;

            private const float MIN_RADIUS = 1f;
            private const float MAX_RADIUS = 3f;
            private float Radius;

            private const float KEEP_CENTER_CHANCE = 0.2f;
            private bool KeepCenter;

            private Dictionary<int, int> FlipAngles = new Dictionary<int, int>()
            {
                {0, 10 },
                {90, 2 },
                {180, 10 },
                {270, 2 }
            };
            private int FlipAngle;

            public override int GetChanceToAppearFor(ShapeType type)
            {
                if (type == ShapeType.EmptyRoot || type == ShapeType.Empty) return 10;
                else return 0;
            }
            public override void Apply(SvgDocument Svg, FlagMainPattern flag, List<Shape> shapes, Shape shape, Color c, int[] nextIds)
            {
                for(int i = 0; i < NumElements; i++)
                {
                    float angle = shape.Angle + FlipAngle + (360f / NumElements * i);

                    if (i == 0 && Mod == Modifier.OmitFirst) continue;
                    if ((angle < shape.Angle + FlipAngle + 90 || angle > shape.Angle + FlipAngle + 270) && Mod == Modifier.OmitHalf) continue;

                    float radius = Radius;
                    if (i == 0 && (Mod == Modifier.OffsetFirst || Mod == Modifier.ReplaceOffsetFirst)) radius *= ModOffsetMultiplier;
                    if (i % 2 == ModAlternateId && (Mod == Modifier.OffsetAlternate || Mod == Modifier.ReplaceOffsetAlternate)) radius *= ModOffsetMultiplier;
                    Vector2 offset = Geometry.GetPointOnCircle(shape.Position, radius, angle);

                    int id = nextIds[0];
                    if (i == 0 && Mod == Modifier.ReplaceOffsetFirst) id = nextIds[1];
                    if (i % 2 == ModAlternateId && (Mod == Modifier.ReplaceAlternate || Mod == Modifier.ReplaceOffsetAlternate)) id = nextIds[1];

                    shapes.Add(new Shape_Empty(id, shape.Position + offset, angle));
                }
                if (KeepCenter) shapes.Add(new Shape_EmptyLeaf(nextIds[2], shape.Position, shape.Angle));
            }
            public override void GenerateNewRandomValues(Random R)
            {
                NumElements = Helper.RandomRange(R, MIN_ELEMENTS, MAX_ELEMENTS);
                Radius = Helper.RandomRange(R, MIN_RADIUS, MAX_RADIUS);
                KeepCenter = R.NextDouble() < KEEP_CENTER_CHANCE;
                FlipAngle = Helper.GetWeightedRandomInt(R, FlipAngles);

                Mod = Helper.GetWeightedRandomEnum<Modifier>(R, Modifiers);
                while(Mod == Modifier.OmitHalf && NumElements < 4 ||
                    Mod == Modifier.OffsetAlternate && NumElements % 2 != 0 ||
                    Mod == Modifier.ReplaceOffsetAlternate && NumElements % 2 != 0 ||
                    Mod == Modifier.ReplaceAlternate && NumElements % 2 != 0)
                    Mod = Helper.GetWeightedRandomEnum<Modifier>(R, Modifiers);
                ModOffsetMultiplier = Helper.RandomRange(R, MOD_MIN_OFFSET_MULTIPLIER, MOD_MAX_OFFSET_MULTIPLIER);
                ModAlternateId = R.Next(0, 2);
            }
        }
        public class Rule_EmptyToEmptyExtend : Rule
        {
            private const float MIN_DISTANCE = 1f;
            private const float MAX_DISTANCE = 3f;
            private float Distance;

            public override int GetChanceToAppearFor(ShapeType type)
            {
                if (type == ShapeType.EmptyRoot) return 2;
                if (type == ShapeType.Empty) return 10;
                else return 0;
            }
            public override void Apply(SvgDocument Svg, FlagMainPattern flag, List<Shape> shapes, Shape shape, Color c, int[] nextIds)
            {
                shapes.Add(new Shape_EmptyLeaf(nextIds[0], shape.Position, shape.Angle)); // Copy of source
                shapes.Add(new Shape_Empty(nextIds[1], shape.Position + Geometry.GetPointOnCircle(Vector2.Zero, Distance, shape.Angle), shape.Angle)); // Extended
            }
            public override void GenerateNewRandomValues(Random R)
            {
                Distance = Helper.RandomRange(R, MIN_DISTANCE, MAX_DISTANCE);
            }
        }
        
    }
}
