using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public class FlagGenerator
    {
        public const int FLAG_WIDTH = 1200;
        public const int FLAG_HEIGHT = 800;

        static Random R = new Random();

        private Dictionary<FlagMainPattern, int> MainPatterns = new Dictionary<FlagMainPattern, int>()
        {
            { new Stripes(), 100 },
            { new CoaOnly(), 60 },
            { new Diagonal(), 50 },
            { new NordicCross(), 50 },
            { new ScatteredStripesAndSymbols(), 60 },
        };

        public SvgDocument GenerateFlag()
        {
            SvgDocument SvgDoc = new SvgDocument()
            {
                Width = FLAG_WIDTH,
                Height = FLAG_HEIGHT
            };

            FlagMainPattern mainPattern = GetRandomMainPattern();
            mainPattern.Apply(SvgDoc, R);

            return SvgDoc;
        }

        public SvgDocument GenerateFlag(int seed)
        {
            R = new Random(seed);
            return GenerateFlag();
        }

        public FlagMainPattern GetRandomMainPattern()
        {
            int probabilitySum = MainPatterns.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<FlagMainPattern, int> kvp in MainPatterns)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return null;
        }
    }
}
