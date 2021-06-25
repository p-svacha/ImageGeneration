using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Svg;

/// <summary>
/// Interaktionslogik für MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    SvgDocument SvgImage;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void GenerateFlag_Click(object sender, RoutedEventArgs e)
    {
        //SvgImage = SvgDocument.Open("E:\\rojava.svg");
        SvgImage = MandalaGenerator.GenerateMandala(this);

        RefreshFlagPanel();
    }

    private void FlagPanel_MouseWheel(object sender, MouseWheelEventArgs e)
    {

    }

    public void RefreshFlagPanel()
    {
        FlagPanel.Children.Clear();
        FlagPanel.Children.Add(SvgHandler.SvgToWpfImage(SvgImage));
    }

    private void FlagPanel_MouseMove(object sender, MouseEventArgs e)
    {

    }
}

