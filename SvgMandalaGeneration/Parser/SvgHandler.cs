using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Svg;

public static class SvgHandler
{

    public static SvgDocument NewSvgDocument (int width, int height)
    {
        SvgDocument newSvg = new SvgDocument();
        newSvg.Width = width;
        newSvg.Height = height;
        SvgRectangle background = new SvgRectangle
        {
            Width = width,
            Height = height,
            Fill = new SvgColourServer(System.Drawing.Color.FromArgb(255, 255, 255))
        };
        newSvg.Children.Add(background);
        return newSvg;
    }


    public static Image SvgToWpfImage(SvgDocument svgDoc)
    {
        System.Drawing.Bitmap bitmap = svgDoc.Draw();
        BitmapImage bitmapImage = BitmapToImageSource(bitmap);

        Image img = new Image();
        img.Stretch = Stretch.None;
        img.HorizontalAlignment = HorizontalAlignment.Center;
        img.VerticalAlignment = VerticalAlignment.Center;
        img.Source = bitmapImage;

        return img;
    }

    private static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }
    }
}

