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
using static FlagGeneration.Geometry;

namespace FlagGeneration
{
    class Coa_SymbolCircle : CoatOfArms
    {

        private const int MIN_SYMBOLS = 2;
        private const int MAX_SYMBOLS = 14;

        private const float STATIC_ANGLE_CHANCE = 0.4f; 

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float size, Color primaryColor, List<Color> flagColors)
        {
            Symbol symbol = flag.GetRandomSymbol();
            Color secondaryColor = flag.ColorManager.GetSecondaryColor(primaryColor, flagColors);

            bool hasStaticAngle = R.NextDouble() < STATIC_ANGLE_CHANCE;

            int numSymbols = flag.RandomRange(MIN_SYMBOLS, MAX_SYMBOLS);
            float minSymbolSize = size * 0.1f + (Math.Min((MAX_SYMBOLS - numSymbols), 10) * size * 0.02f);
            float maxSymbolSize = size * 0.1f + (Math.Min((MAX_SYMBOLS - numSymbols), 10) * size * 0.02f);
            float symbolSize = flag.RandomRange(minSymbolSize, maxSymbolSize);
            float angleStep = 360f / numSymbols;
            float radius = size * 0.5f - symbolSize * 0.5f;

            int startAngle = R.Next(0, 3) * 90;

            for(int i = 0; i < numSymbols; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 position = Geometry.GetPointOnCircle(pos, radius, angle);
                float symbolAngle = hasStaticAngle ? 0 : angle;
                symbol.Draw(Svg, position, symbolSize, symbolAngle, primaryColor, secondaryColor);
            }
        }
    }
}
