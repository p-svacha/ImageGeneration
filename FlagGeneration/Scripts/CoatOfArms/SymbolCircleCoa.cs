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
    class SymbolCircleCoa : CoatOfArms
    {

        private const int MIN_SYMBOLS = 2;
        private const int MAX_SYMBOLS = 14;

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Vector2 pos, float size, Color c, Random R)
        {
            Symbol symbol = flag.GetRandomSymbol();

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
                Vector2 position = flag.GetPointOnCircle(pos, radius, angle);
                symbol.Draw(Svg, flag, position, symbolSize, angle, c);
            }
        }
    }
}
