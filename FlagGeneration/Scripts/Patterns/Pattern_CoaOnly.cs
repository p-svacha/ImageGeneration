﻿using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class Pattern_CoaOnly : FlagMainPattern
    {
        public enum Style
        {
            Plain,
            Frame,
            Diamond
        }

        private Dictionary<Style, int> Styles = new Dictionary<Style, int>()
        {
            {Style.Plain, 100 },
            {Style.Frame, 80 },
            {Style.Diamond, 60 }
        };

        private const float MIN_COA_SIZE_REL = 0.5f;

        private const float MIN_FRAME_SIZE = 0.05f; // relative to flag height
        private const float MAX_FRAME_SIZE = 0.3f; // relative to flag height
        private const float MAX_DIAMOND_FRAME_SIZE = 0.3f; // relative to flag height

        public override void DoApply()
        {
            Style style = GetRandomStyle();

            CoatOfArmsPosition = FlagCenter;

            Color bgColor, secColor;
            List<Color> coaCandidateColors = new List<Color>();
            float minCoaSizeRel = 0, maxCoaSizeRel = 0;
            switch(style)
            {
                case Style.Plain:
                    bgColor = ColorManager.GetRandomColor();
                    CoatOfArmsPrimaryColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
                    minCoaSizeRel = 0.6f;
                    maxCoaSizeRel = 0.95f;
                    DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight, bgColor);
                    break;

                case Style.Frame:
                    bgColor = ColorManager.GetRandomColor();
                    secColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
                    coaCandidateColors.Add(bgColor);
                    coaCandidateColors.Add(ColorManager.GetRandomColor(secColor));
                    CoatOfArmsPrimaryColor = coaCandidateColors[R.Next(0, coaCandidateColors.Count)];

                    float frameHeightRel = RandomRange(MIN_FRAME_SIZE, MAX_FRAME_SIZE);
                    float frameSize = frameHeightRel * FlagHeight;
                    DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight, bgColor);
                    // Frame
                    DrawRectangle(Svg, frameSize, frameSize, FlagWidth - 2*frameSize, FlagHeight - 2*frameSize, secColor);

                    minCoaSizeRel = 0.3f;
                    maxCoaSizeRel = 1f - (2f * frameHeightRel);
                    break;

                case Style.Diamond:
                    bgColor = ColorManager.GetRandomColor();
                    secColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
                    coaCandidateColors.Add(bgColor);
                    coaCandidateColors.Add(ColorManager.GetRandomColor(secColor));
                    CoatOfArmsPrimaryColor = coaCandidateColors[R.Next(0, coaCandidateColors.Count)];

                    float frameSizeXRel = RandomRange(0f, MAX_DIAMOND_FRAME_SIZE);
                    float frameSizeYRel = RandomRange(0f, MAX_DIAMOND_FRAME_SIZE);
                    float frameSizeX = frameSizeXRel * FlagWidth;
                    float frameSizeY = frameSizeYRel * FlagHeight;
                    DrawRectangle(Svg, 0, 0, FlagWidth, FlagHeight, bgColor);
                    // Frame
                    Vector2[] vertices = new Vector2[]
                    {
                        new Vector2(FlagWidth/2, frameSizeY),
                        new Vector2(FlagWidth - frameSizeX, FlagHeight / 2),
                        new Vector2(FlagWidth/2, FlagHeight - frameSizeY),
                        new Vector2(frameSizeX, FlagHeight/2)
                    };
                    DrawPolygon(Svg, vertices, secColor);

                    minCoaSizeRel = 0.3f;
                    maxCoaSizeRel = 1f - (3 * Math.Max(frameSizeXRel, frameSizeYRel));
                    break;
            }

            

            float minCoaSize = FlagHeight * minCoaSizeRel;
            float maxCoaSize = FlagHeight * maxCoaSizeRel;
            CoatOfArmsSize = RandomRange(minCoaSize, maxCoaSize);

            ApplyCoatOfArms(Svg);
        }

        public Style GetRandomStyle()
        {
            int probabilitySum = Styles.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<Style, int> kvp in Styles)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return Style.Plain;
        }
    }
}
