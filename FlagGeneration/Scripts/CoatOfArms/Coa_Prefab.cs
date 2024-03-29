﻿using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    /// <summary>
    /// All SVG Prefabs are from https://svgsilh.com/
    /// SVG's should be close to square
    /// </summary>
    class Coa_Prefab : CoatOfArms
    {
        public override void Draw(SvgDocument Svg, FlagMainPattern flag, Random R, Vector2 pos, float size, Color primaryColor, List<Color> flagColors)
        {
            string prefabPath = AppContext.BaseDirectory + "../../Resources/CoatOfArms";
            string[] files = Directory.GetFiles(prefabPath);
            string chosenPath = files[R.Next(files.Length)];
            Console.WriteLine("Coa prefab id = " + chosenPath);

            SvgDocument prefab = SvgDocument.Open(chosenPath);
            prefab.Width = size;
            prefab.Height = size;
            prefab.X = pos.X - size / 2;
            prefab.Y = pos.Y - size / 2;
            SvgColourServer colServ = new SvgColourServer(primaryColor);
            prefab.Fill = colServ;
            prefab.Stroke = colServ;
            foreach (SvgElement elem in prefab.Children) FillElement(elem, colServ);

            Svg.Children.Add(prefab);
        }

        private void FillElement(SvgElement elem, SvgColourServer c)
        {
            if(!elem.Fill.Equals(new SvgColourServer(Color.Transparent))) elem.Fill = c;
            elem.Stroke = c;
            foreach (SvgElement elem2 in elem.Children) FillElement(elem2, c);
        }
    }
}
