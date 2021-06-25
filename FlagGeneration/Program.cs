using Svg;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace FlagGeneration
{
    /// <summary>
    /// This program has to be run via console, there are two ways to run it:
    /// A: To generate a single flag
    ///     FlagGenerator.exe xxx/Flags/testflag.png (42069)
    ///     1. param = save path (needs to end with .png), 2. param = seed (optional)
    /// B: To generate multiple flags
    ///     FlagGenerator.exe xxx/Flags 5
    ///     1. param = save path (must not end with .png), 2. param = amount of flags
    /// </summary>
    class Program
    {
        public static string SavePath;
        // args[0] is file path where the flag gets saved to
        static void Main(string[] args)
        {
            Random seedRng = new Random();
            FlagGenerator Gen = new FlagGenerator();

            SavePath = args[0];
            int intParam = 0;
            if(args.Length > 1) int.TryParse(args[1], out intParam);

            if (!SavePath.EndsWith(".png") && intParam > 0) // Generate multiple
            {
                int numFlags = intParam;
                System.Console.WriteLine("Generating " + numFlags + " random flags");

                if (intParam > 10) intParam = 10;
                for(int i = 0; i < intParam; i++)
                {
                    int seed = seedRng.Next(Int32.MinValue, Int32.MaxValue);
                    SvgDocument Svg = Gen.GenerateFlag(seed);
                    Svg.Draw().Save(SavePath + "seed_" + seed + ".png");
                }
            }

            else
            {
                int seed = intParam;
                if(!SavePath.EndsWith(".png")) throw new Exception("Path needs to end with .png");
                SvgDocument SvgFlag;
                if (seed != 0)
                {
                    System.Console.WriteLine("Generating flag with seed " + seed);
                    SvgFlag = Gen.GenerateFlag(seed);
                }
                else
                {
                    Console.WriteLine("Generating random flag");
                    SvgFlag = Gen.GenerateFlag();
                }

                //Svg.Write(args[0]); // write to svg file
                SvgFlag.Draw().Save(SavePath); // Write to png
            }
        }
    }
}
