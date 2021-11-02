using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class Coa_SingleSymbol : CoatOfArms
    {
        private const float TILT_CHANCE = 0.1f; // Chance that the symbol is rotated a random amount

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float size, Color primaryColor, List<Color> flagColors)
        {
            Symbol symbol = flag.GetRandomSymbol();
            Color secondaryColor = flag.ColorManager.GetSecondaryColor(primaryColor, flagColors);

            float angle = 0f;
            if (R.NextDouble() < TILT_CHANCE) angle = R.Next(0, 360);

            symbol.Draw(Svg, pos, size, angle, primaryColor, secondaryColor);
        }
    }
}
