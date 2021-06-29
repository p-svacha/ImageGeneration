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
    public abstract class Symbol
    {
        protected FlagMainPattern Flag;
        protected Random R;

        protected const float FRAME_CHANCE = 0.1f;
        protected float MIN_FRAME_WIDTH = 0.5f; // relative to outer
        protected float MAX_FRAME_WIDTH = 0.9f; // relative to outer

        protected bool HasFrame;
        protected float FrameWidth;

        protected Symbol(FlagMainPattern flag, Random r)
        {
            Flag = flag;
            R = r;
            OverrideValues();

            if (R.NextDouble() < FRAME_CHANCE)
            {
                HasFrame = true;
                FrameWidth = Flag.RandomRange(MIN_FRAME_WIDTH, MAX_FRAME_WIDTH);
            }
        }

        protected virtual void OverrideValues() { }
        public abstract void Draw(SvgDocument Svg, Vector2 pos, float size, float angle, Color primaryColor, Color secondaryColor);

        public int RandomRange(int min, int max)
        {
            return R.Next(max - min + 1) + min;
        }
        public float RandomRange(float min, float max)
        {
            return (float)R.NextDouble() * (max - min) + min;
        }
    }
}
