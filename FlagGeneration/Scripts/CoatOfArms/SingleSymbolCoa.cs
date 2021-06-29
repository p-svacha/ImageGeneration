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
    class SingleSymbolCoa : CoatOfArms
    {
        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float size, Color primaryColor, List<Color> flagColors)
        {
            Symbol symbol = flag.GetRandomSymbol();
            Color secondaryColor = flag.ColorManager.GetSecondaryColor(primaryColor, flagColors);
            symbol.Draw(Svg, pos, size, 0, primaryColor, secondaryColor);
        }
    }
}
