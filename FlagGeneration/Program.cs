using Svg;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace FlagGeneration
{
    /// <summary>
    /// This program generates flag images.
    /// It has to be run via console with 3 parameters:
    /// 1. param: directory
    ///     decides where the files will be saved
    /// 2. param: "s" or "m"
    ///     -s: generate a single flag with a specified seed: 2nd param is the seed
    ///     -m: generate multiple random flags: 2nd param is the amount
    /// 3. param: # [int]
    ///     represents the seed or amount of flags depending on 2. param
    /// 4. param: "svg" or "png"
    ///     decides if the files will be saved .svg or .png
    /// </summary>
    class Program
    {
        public static string Args0_Path;
        public static string Args1_Type;
        public static int Args2_Int;
        public static string Args3_Format;
        // args[0] is file path where the flag gets saved to
        static void Main(string[] args)
        {
            Random seedRng = new Random();
            FlagGenerator Gen = new FlagGenerator();

            if(args.Length != 4) throw new Exception("Invalid parameters.\nThe program must be run with 4 parameters: TargetDirectory, s/m (s for single flag with specified seed, m for multiple random flags), int (seed for single flag, amount for multiple flags), svg/png (format)");

            Args0_Path = args[0];
            if (!Directory.Exists(Args0_Path)) throw new Exception("Directory " + Args0_Path + " not found.");
            Args1_Type = args[1];
            if (Args1_Type != "s" && Args1_Type != "m") throw new Exception("2. param needs to be \"s\" for a single flag or \"m\" for multiple flags.");
            if(!int.TryParse(args[2], out Args2_Int)) throw new Exception("3. param needs to be an int (seed for single flag or amount for multiple flags)");
            Args3_Format = args[3];
            if (Args3_Format != "svg" && Args3_Format != "png") throw new Exception("4. param needs to be \"svg\" for .svg files or \"png\" .png files.");

            if(Args1_Type == "s") // Generate a single flag with 
            {
                string fullPath = Args0_Path + "/generatedFlag";
                GenerateAndSaveFlag(Gen, fullPath, Args2_Int, Args3_Format);
            }
            else if(Args1_Type == "m") // Generate multiple random flags
            {
                for(int i = 0; i < Args2_Int; i++)
                {
                    int seed = seedRng.Next(Int32.MinValue, Int32.MaxValue);
                    string fullPath = Args0_Path + "/flag_" + i;
                    GenerateAndSaveFlag(Gen, fullPath, seed, Args3_Format);
                }
            } 
        }

        private static void GenerateAndSaveFlag(FlagGenerator gen, string path , int seed, string format)
        {
            SvgDocument Svg;
            Svg = gen.GenerateFlag(seed);
            string fullPath = path;
            if (format == "png")
            {
                fullPath += ".png";
                Svg.Draw().Save(fullPath, ImageFormat.Png);
            }
            else if (format == "svg")
            {
                fullPath += ".svg";
                Svg.Write(fullPath);
            }

            Console.WriteLine("Saved " + fullPath + " with seed " + seed);
        }
    }
}
