using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public class ColorManager
    {
        private Random R;

        public ColorManager(Random r)
        {
            R = r;
        }

        private Dictionary<Color, int> Colors = new Dictionary<Color, int>
        {
            {Color.FromArgb(199,4,44), 100}, // Red
            {Color.FromArgb(255,255,255), 90}, // White
            {Color.FromArgb(0,85,164), 80}, // Blue
            {Color.FromArgb(0,140,69), 70}, // Green
            {Color.FromArgb(255,205,0), 70}, // Yellow
            {Color.FromArgb(0,0,0),70}, // Black

            {Color.FromArgb(117,170,219), 20}, // Light Blue
            {Color.FromArgb(235,116,0), 20}, // Orange
            {Color.FromArgb(0,83,78), 20}, // Jungle Green
            {Color.FromArgb(247,168,184), 20}, // Pink
            {Color.FromArgb(104,1,1), 20}, // Dark Red
            {Color.FromArgb(6,0,106), 20}, // Navy Blue
        };

        public Color GetRandomColor(List<Color> excludedColors = null)
        {
            if (excludedColors == null) excludedColors = new List<Color>();
            Dictionary<Color, int> colorCandidates = Colors.Where(x => !excludedColors.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);

            int probabilitySum = colorCandidates.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<Color, int> kvp in colorCandidates)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return Color.Transparent;
        }

        public Color GetRandomColor(params Color[] excludedColors)
        {
            return GetRandomColor(excludedColors.ToList());
        }

        /// <summary>
        /// Returns a list of x colors without duplicates.
        /// </summary>
        public List<Color> GetRandomColors(int amount)
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i < amount; i++) colors.Add(GetRandomColor(colors));
            return colors;
        }

        /// <summary>
        /// Returns a secondary color (usually for a framed symbol or coa). It will be unlike the primary color and have a good chance to be a color that is already used in flagColors, but in can also be a random one.
        /// </summary>
        public Color GetSecondaryColor(Color primaryColor, List<Color> flagColors)
        {
            List<Color> candidateColors = new List<Color>();
            candidateColors.AddRange(flagColors);
            if (candidateColors.Contains(primaryColor)) candidateColors.Remove(primaryColor);
            candidateColors.Add(GetRandomColor(candidateColors));
            return candidateColors[R.Next(0, candidateColors.Count)];
        }
    }
}
