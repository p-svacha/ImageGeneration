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
    class Diagonal : FlagMainPattern
    {
        public enum Style
        {
            Split,
            Cross
        }

        private Dictionary<Style, int> Styles = new Dictionary<Style, int>()
        {
            {Style.Split, 100 },
            {Style.Cross, 100 },
        };

        private const float DOUBLE_SPLIT_CHANCE = 0.25f;

        private const float SPLIT_COA_CHANCE = 0.5f;
        private const float TOP_RIGHT_COA_CHANCE = 0.3f;

        private const float MIN_CROSS_WIDTH = 0.02f;
        private const float MAX_CROSS_WIDTH = 0.25f;
        private const float INNER_CROSS_CHANCE = 0.25f;
        private const float CROSS_DIFFERENT_SIDE_COLORS_CHANCE = 0.25f;

        public override void Apply(SvgDocument SvgDocument, Random r)
        {
            R = r;
            ColorManager = new ColorManager(R);

            float minCoaSize = 0.5f;
            float maxCoaSize = 0.95f;
            CoatOfArmsSize = RandomRange(minCoaSize * FlagHeight, maxCoaSize * FlagHeight);
            CoatOfArmsPosition = FlagCenter;

            switch (GetWeightedRandomEnum(Styles))
            {
                case Style.Split:
                    Color c1 = ColorManager.GetRandomColor();
                    Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                    AddUsedColor(c1);
                    AddUsedColor(c2);
                    CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(new List<Color>() { c1, c2 });

                    Vector2[] triangle1 = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(0, FlagHeight) };
                    Vector2[] triangle2 = new Vector2[] { new Vector2(0, FlagHeight), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, FlagHeight) };
                    DrawPolygon(SvgDocument, triangle1, c1);
                    DrawPolygon(SvgDocument, triangle2, c2);

                    // Double Split
                    if(R.NextDouble() < DOUBLE_SPLIT_CHANCE)
                    {
                        Color c3 = ColorManager.GetRandomColor(new List<Color>() { c1, c2 });
                        AddUsedColor(c3);
                        float minSplit2Start = 0.2f;
                        float maxSplit2Start = 0.6f;
                        float split2Start = RandomRange(minSplit2Start, maxSplit2Start);
                        Vector2[] triangle3 = new Vector2[] { new Vector2(FlagWidth * split2Start, FlagHeight), new Vector2(FlagWidth, FlagHeight * split2Start), new Vector2(FlagWidth, FlagHeight) };
                        DrawPolygon(SvgDocument, triangle3, c3);

                        CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(new List<Color>() { c1 });
                        minCoaSize = 0.2f;
                        maxCoaSize = 0.5f;
                        CoatOfArmsSize = RandomRange(FlagHeight * minCoaSize, FlagHeight * maxCoaSize);
                        CoatOfArmsPosition = new Vector2(50 + CoatOfArmsSize / 2, 50 + CoatOfArmsSize / 2);
                    }

                    // Top right coa
                    if(R.NextDouble() < TOP_RIGHT_COA_CHANCE)
                    {
                        CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(new List<Color>() { c1 });
                        minCoaSize = 0.2f;
                        maxCoaSize = 0.5f;
                        CoatOfArmsSize = RandomRange(FlagHeight * minCoaSize, FlagHeight * maxCoaSize);
                        CoatOfArmsPosition = new Vector2(50 + CoatOfArmsSize / 2, 50 + CoatOfArmsSize / 2);
                    }

                    // Coa
                    if (R.NextDouble() < SPLIT_COA_CHANCE) ApplyCoatOfArms(SvgDocument);
                    break;

                case Style.Cross:
                    Color bg = ColorManager.GetRandomColor();
                    Color crossColor = ColorManager.GetRandomColor(new List<Color>() { bg });
                    AddUsedColor(bg);
                    AddUsedColor(crossColor);
                    DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, bg);
                    float crossWidth = RandomRange(MIN_CROSS_WIDTH, MAX_CROSS_WIDTH);
                    float crossWidthAbsX = crossWidth * FlagWidth;
                    float crossWidthAbsY = crossWidth * FlagHeight;

                    if (R.NextDouble() < CROSS_DIFFERENT_SIDE_COLORS_CHANCE)
                    {
                        Color sideColor = ColorManager.GetRandomColor(new List<Color>() { bg, crossColor });
                        AddUsedColor(sideColor);
                        Vector2[] leftTriangle = new Vector2[] { new Vector2(0, 0), FlagCenter, new Vector2(0, FlagHeight) };
                        Vector2[] rightTriangle = new Vector2[] { new Vector2(FlagWidth, 0), FlagCenter, new Vector2(FlagWidth, FlagHeight) };
                        DrawPolygon(SvgDocument, leftTriangle, sideColor);
                        DrawPolygon(SvgDocument, rightTriangle, sideColor);
                    }

                    Vector2[] cross1Vertices = new Vector2[] { new Vector2(0, 0), new Vector2(crossWidthAbsX, 0), new Vector2(FlagWidth, FlagHeight - crossWidthAbsY), new Vector2(FlagWidth, FlagHeight), new Vector2(FlagWidth - crossWidthAbsX, FlagHeight), new Vector2(0, crossWidthAbsY) };
                    Vector2[] cross2Vertices = new Vector2[] { new Vector2(0, FlagHeight), new Vector2(0, FlagHeight - crossWidthAbsY), new Vector2(FlagWidth -  crossWidthAbsX, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, crossWidthAbsY), new Vector2(crossWidthAbsX, FlagHeight) };
                    DrawPolygon(SvgDocument, cross1Vertices, crossColor);
                    DrawPolygon(SvgDocument, cross2Vertices, crossColor);

                    if(R.NextDouble() < INNER_CROSS_CHANCE)
                    {
                        Color innerCrossColor = ColorManager.GetRandomColor(new List<Color>() { crossColor });
                        AddUsedColor(innerCrossColor);
                        float innerCrossWidth = RandomRange(crossWidth * 0.2f, crossWidth * 0.8f);
                        float innerCrossWidthAbsX = innerCrossWidth * FlagWidth;
                        float innerCrossWidthAbsY = innerCrossWidth * FlagHeight;
                        Vector2[] innerCross1Vertices = new Vector2[] { new Vector2(0, 0), new Vector2(innerCrossWidthAbsX, 0), new Vector2(FlagWidth, FlagHeight - innerCrossWidthAbsY), new Vector2(FlagWidth, FlagHeight), new Vector2(FlagWidth - innerCrossWidthAbsX, FlagHeight), new Vector2(0, innerCrossWidthAbsY) };
                        Vector2[] innerCross2Vertices = new Vector2[] { new Vector2(0, FlagHeight), new Vector2(0, FlagHeight - innerCrossWidthAbsY), new Vector2(FlagWidth - innerCrossWidthAbsX, 0), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, innerCrossWidthAbsY), new Vector2(innerCrossWidthAbsX, FlagHeight) };
                        DrawPolygon(SvgDocument, innerCross1Vertices, innerCrossColor);
                        DrawPolygon(SvgDocument, innerCross2Vertices, innerCrossColor);
                    }
                    break;
            }

            
        }
    }
}
