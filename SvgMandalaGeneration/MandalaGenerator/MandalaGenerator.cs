using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using static MandalaLanguage;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using System.Xml;
using System.Web.UI.WebControls;

public class MandalaGenerator
{
    public static readonly int MinStrokeWidth = 1;
    public static readonly int MaxStrokeWidth = 3;
    public static int StrokeWidth;

    public static readonly int MinDepth = 4;
    public static readonly int MaxDepth = 10;

    public static readonly int MinAreaToReproduce = 3000;

    // Elements
    public static Dictionary<int, List<MandalaElement>> Elements;
    public static float Size;

    //UI
    public static MainWindow Main;

    public static SvgDocument GenerateMandala(MainWindow main)
    {
        Random Random = new Random();

        Elements = new Dictionary<int, List<MandalaElement>>();

        Main = main;

        int maxDepth = Random.Next(MaxDepth - MinDepth) + MinDepth;
        StrokeWidth = Random.Next(MaxStrokeWidth - MinStrokeWidth) + MinStrokeWidth;

        SvgDocument SvgDocument = SvgHandler.NewSvgDocument(1000, 1000);
        
        List<MandalaElement> elements = new List<MandalaElement>();
        ME_Empty startElement = new ME_Empty(SvgDocument, 0, MandalaElement.ElementId++);
        Size = startElement.Area;
        elements.Add(startElement);

        XmlTextWriter writer = new XmlTextWriter("asdasdas.svg", Encoding.UTF8);
        SvgDocument.Write(writer);
        writer.Close();

        MandalaElement nextElement;
        while (elements.Count > 0)
        {
            // Get next element and elements with the same id
            nextElement = elements[0];
            List<MandalaElement> nextElementGroup = elements.Where(x => x.Id == nextElement.Id).ToList();
            nextElementGroup.Add(nextElement);

            // Remove elements with same id from queue
            elements = elements.Except(nextElementGroup).ToList();

            // Find which rules can be applied to elements
            List<MandalaLanguageRule> ruleCandidates = MandalaLanguageRules.Where(x => x.CanApply(nextElement)).ToList();

            if(ruleCandidates.Count > 0)
            {
                // Chose a applicable rule at random and initialize it
                MandalaLanguageRule chosenRule = ruleCandidates[Random.Next(ruleCandidates.Count)];
                chosenRule.Initialize(nextElement, Random);

                // Apply (draw) rule to all elements in group
                foreach (MandalaElement element in nextElementGroup)
                {
                    List<MandalaElement> resultShapes = chosenRule.Apply(element);

                    // Add outcoming shapes to the recursive queue
                    foreach (MandalaElement shape in resultShapes)
                    {
                        //if (shape.Depth < maxDepth) elements.Add(shape);
                        if (shape.Area > MinAreaToReproduce) elements.Add(shape);
                    }
                }
            }
            else
            {
                Console.WriteLine("No rules found for shape: " + nextElement.Type);
            }
        }
        XmlTextWriter writer2 = new XmlTextWriter("testSvg.svg", Encoding.UTF8);
        SvgDocument.Write(writer2);
        writer2.Close();
        UpdateInfoBox();

        

        return SvgDocument;
    }

    public static void AddElement(MandalaElement element)
    {
        if (Elements.ContainsKey(element.Id))
        {
            Elements[element.Id].Add(element);
        }
        else Elements.Add(element.Id, new List<MandalaElement>() { element });
    }

    public static void UpdateInfoBox()
    {
        Main.ElementGrid.Children.Clear();
        Main.ElementGrid.RowDefinitions.Clear();
        int row = 0;
        foreach(KeyValuePair<int, List<MandalaElement>> kvp in Elements)
        {
            Main.ElementGrid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(20) });
            System.Windows.Controls.TextBox Text = new System.Windows.Controls.TextBox();
            Text.SetValue(Grid.RowProperty, row);
            Text.SetValue(Grid.ColumnProperty, 0);
            Text.MouseEnter += HighlightMandalaElement;
            Text.MouseLeave += UnhighlightMandalaElement;
            Text.IsReadOnly = true;

            MandalaElement elem = kvp.Value[0];
           
            string text = elem.Id + "\t" + elem.Type + (elem.Type == MandalaElementType.StarSpike || elem.Type == MandalaElementType.CircleSector ? "\t" : "\t\t") + (int)elem.Area + "\t(" + (elem.Area/Size).ToString("0.00%") + ")";
            Text.Text = text;

            Main.ElementGrid.Children.Add(Text);
            row++;
        }
    }

    public static void HighlightMandalaElement(object sender, MouseEventArgs e)
    {
        System.Windows.Controls.TextBox text = (System.Windows.Controls.TextBox)sender;
        int id = int.Parse(new String(text.Text.TakeWhile(Char.IsDigit).ToArray()));

        foreach(MandalaElement elem in Elements[id])
        {
            elem.SvgElement.Fill = new SvgColourServer(Color.Red);
        }

        Main.RefreshFlagPanel();
    }

    public static void UnhighlightMandalaElement(object sender, MouseEventArgs e)
    {
        System.Windows.Controls.TextBox text = (System.Windows.Controls.TextBox)sender;
        int id = int.Parse(new String(text.Text.TakeWhile(Char.IsDigit).ToArray()));

        foreach (MandalaElement elem in Elements[id])
        {
            elem.SvgElement.Fill = new SvgColourServer(Color.Transparent);
        }

        Main.RefreshFlagPanel();
    }
}
