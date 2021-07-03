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
    class Pattern_Diagonal : FlagMainPattern
    {
        public enum Style
        {
            Split,
        }

        private Dictionary<Style, int> Styles = new Dictionary<Style, int>()
        {
            {Style.Split, 100 },
        };

        private const float DOUBLE_SPLIT_CHANCE = 0.25f;

        private const float SPLIT_COA_CHANCE = 0.5f;
        private const float TOP_RIGHT_COA_CHANCE = 0.3f;

        private const float MIN_CROSS_WIDTH = 0.02f;
        private const float MAX_CROSS_WIDTH = 0.25f;
        private const float INNER_CROSS_CHANCE = 0.25f;
        private const float CROSS_DIFFERENT_SIDE_COLORS_CHANCE = 0.25f;

        public override void DoApply()
        {
            float minCoaSize = 0.5f;
            float maxCoaSize = 0.95f;
            CoatOfArmsSize = RandomRange(minCoaSize * FlagHeight, maxCoaSize * FlagHeight);
            CoatOfArmsPosition = FlagCenter;

            switch (GetWeightedRandomEnum(Styles))
            {
                case Style.Split:
                    Color c1 = ColorManager.GetRandomColor();
                    Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                    CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(new List<Color>() { c1, c2 });

                    Vector2[] triangle1 = new Vector2[] { new Vector2(0, 0), new Vector2(FlagWidth, 0), new Vector2(0, FlagHeight) };
                    Vector2[] triangle2 = new Vector2[] { new Vector2(0, FlagHeight), new Vector2(FlagWidth, 0), new Vector2(FlagWidth, FlagHeight) };
                    DrawPolygon(Svg, triangle1, c1);
                    DrawPolygon(Svg, triangle2, c2);

                    // Double Split
                    if(R.NextDouble() < DOUBLE_SPLIT_CHANCE)
                    {
                        Color c3 = ColorManager.GetRandomColor(new List<Color>() { c1, c2 });
                        float minSplit2Start = 0.2f;
                        float maxSplit2Start = 0.6f;
                        float split2Start = RandomRange(minSplit2Start, maxSplit2Start);
                        Vector2[] triangle3 = new Vector2[] { new Vector2(FlagWidth * split2Start, FlagHeight), new Vector2(FlagWidth, FlagHeight * split2Start), new Vector2(FlagWidth, FlagHeight) };
                        DrawPolygon(Svg, triangle3, c3);

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
                    if (R.NextDouble() < SPLIT_COA_CHANCE) ApplyCoatOfArms(Svg);
                    break;
            }

            
        }
    }
}
