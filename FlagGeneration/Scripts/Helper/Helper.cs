using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public static class Helper
    {
        public static float RandomRange(Random R, float min, float max)
        {
            return (float)R.NextDouble() * (max - min) + min;
        }

        public static int RandomRange(Random R, int min, int max)
        {
            return R.Next(max - min + 1) + min;
        }

        public static T GetWeightedRandomEnum<T>(Random R, Dictionary<T, int> weightDictionary) where T : System.Enum
        {
            int probabilitySum = weightDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<T, int> kvp in weightDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            throw new Exception();
        }

        public static int GetWeightedRandomInt(Random R, Dictionary<int, int> weightDictionary)
        {
            int probabilitySum = weightDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<int, int> kvp in weightDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            throw new Exception();
        }
    }
}
