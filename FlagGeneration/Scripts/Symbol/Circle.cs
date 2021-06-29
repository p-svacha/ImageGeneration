using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FlagGeneration.Geometry;

namespace FlagGeneration
{
    class Circle : Symbol
    {
        public Circle(FlagMainPattern flag, Random R) : base(flag, R) { }

        public override void Draw(SvgDocument Svg, Vector2 center, float size, float angle, Color primaryColor, Color secondaryColor)
        {
            Flag.DrawCircle(Svg, center, size / 2, primaryColor);

            if (HasFrame)
            {
                Flag.DrawCircle(Svg, center, (size / 2) * FrameWidth, secondaryColor);
            }
        }
    }
}
