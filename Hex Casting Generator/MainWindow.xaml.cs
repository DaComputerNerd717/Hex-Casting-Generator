using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using Hex_Casting_Generator.Graphs;

namespace Hex_Casting_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Graphs.Path? path = null;

        bool beenRun = false;
        static int genAStar = 0;

        static int color1 = 0x3380cc;
        static int color2 = 0xfb80fb; 
        static int color3 = 0x9880cb; 
        static int color4 = 0x910698;

        static int antialias = 1; //would be a boolean, similar to limit vals; 1 is truthy, 0 falsey

        public static int limitVals = 1; //disabled if == 0

        public MainWindow()
        {
            InitializeComponent();
            genButton.Click += GenButton_Click2;
            this.SizeChanged += OnSizeChange;
            displayButton.Click += DispButton_Click;
            menuSelectColors.Click += SelectColor_Click;
            color1Box.TextChanged += Color1Pick;
            color2Box.TextChanged += Color2Pick;
            color3Box.TextChanged += Color3Pick;
            color4Box.TextChanged += Color4Pick;
            btnApplyColor.Click += ApplyColor;
            menuLimitVals.Checked += EnableValLimit;
            menuLimitVals.Unchecked += DisableValLimit;
            menuAliasing.Checked += EnableAntialiasing;
            menuAliasing.Unchecked += DisableAntialiasing;
            menuAStar.Checked += EnableAStar;
            menuAStar.Unchecked += DisableAStar;
        }

        Regex colorMatch3 = new(@"([\da-f])([\da-f])([\da-f])");
        Regex colorMatch6 = new(@"([\da-f]{2})([\da-f]{2})([\da-f]{2})");

        public void EnableAntialiasing(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref antialias, 1);
        }

        public void DisableAntialiasing(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref antialias, 0);
        }

        public void EnableValLimit(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref limitVals, 1);
        }

        public void DisableValLimit(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref limitVals, 0);
        }

        public void EnableAStar(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref genAStar, 1);
        }

        public void DisableAStar(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref genAStar, 0);
        }

        public void ApplyColor(object sender, RoutedEventArgs e)
        {
            colorPopup.IsOpen = false;
            if (beenRun)
            {
                patternPanel.Dispatcher.BeginInvoke(() => {

                    Graphs.Path? currentPath = Interlocked.Exchange(ref this.path, null);
                    if (currentPath != null)
                    {
                        Graphs.Path copy = currentPath!.Copy();
                        Interlocked.Exchange(ref this.path, copy); //copy back
                        Polyline? start;
                        Line[] lines = CreatePathSegments(currentPath, patternPanel, out start);
                        patternPanel.Children.Clear();
                        if (start != null)
                        {
                            Canvas.SetLeft(start, 0);
                            Canvas.SetTop(start, 0);
                            AddChild(patternPanel, start);
                        }
                        foreach (Line l in lines)
                        {
                            Canvas.SetLeft(l, 0);
                            Canvas.SetTop(l, 0);
                            AddChild(patternPanel, l);
                        }
                    }
                });
            }
        }

        public void Color1Pick(object sender, EventArgs e)
        {
            
            string s = color1Box.Text;
            Match m;
            bool matched3 = false;
            if (colorMatch6.IsMatch(s))
            {
                m = colorMatch6.Match(s);
            }
            else if (colorMatch3.IsMatch(s))
            {
                m = colorMatch3.Match(s);
                matched3 = true;
            }
            else
            {
                return;
            }
            int r = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
            if (matched3)
            {
                r += r << 4;
                g += g << 4;
                b += b << 4;
            }
            Interlocked.Exchange(ref color1, r << 16 | g << 8 | b);
            color1Sample.Fill = new SolidColorBrush(Color.FromRgb((byte) r, (byte) g, (byte) b));
        }

        public void Color2Pick(object sender, EventArgs e)
        {

            string s = color2Box.Text;
            Match m;
            bool matched3 = false;
            if (colorMatch6.IsMatch(s))
            {
                m = colorMatch6.Match(s);
            }
            else if (colorMatch3.IsMatch(s))
            {
                m = colorMatch3.Match(s);
                matched3 = true;
            }
            else
            {
                return;
            }
            int r = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
            if (matched3)
            {
                r += r << 4;
                g += g << 4;
                b += b << 4;
            }
            Interlocked.Exchange(ref color2, r << 16 | g << 8 | b);
            color2Sample.Fill = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
        }

        public void Color3Pick(object sender, EventArgs e)
        {

            string s = color3Box.Text;
            Match m;
            bool matched3 = false;
            if (colorMatch6.IsMatch(s))
            {
                m = colorMatch6.Match(s);
            }
            else if (colorMatch3.IsMatch(s))
            {
                m = colorMatch3.Match(s);
                matched3 = true;
            }
            else
            {
                return;
            }
            int r = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
            if (matched3)
            {
                r += r << 4;
                g += g << 4;
                b += b << 4;
            }
            Interlocked.Exchange(ref color3, r << 16 | g << 8 | b);
            color3Sample.Fill = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
        }

        public void Color4Pick(object sender, EventArgs e)
        {

            string s = color4Box.Text;
            Match m;
            bool matched3 = false;
            if (colorMatch6.IsMatch(s))
            {
                m = colorMatch6.Match(s);
            }
            else if (colorMatch3.IsMatch(s))
            {
                m = colorMatch3.Match(s);
                matched3 = true;
            }
            else
            {
                return;
            }
            int r = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
            if (matched3)
            {
                r += r << 4;
                g += g << 4;
                b += b << 4;
            }
            Interlocked.Exchange(ref color4, r << 16 | g << 8 | b);
            color4Sample.Fill = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));

        }

        public void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            colorPopup.IsOpen = true;
        }

        public void OnSizeChange(object sender, SizeChangedEventArgs e)
        {
            if (beenRun)
            {
                patternPanel.Dispatcher.BeginInvoke(() => {

                    Graphs.Path? currentPath = Interlocked.Exchange(ref this.path, null);
                    if (currentPath != null)
                    {
                        Graphs.Path copy = currentPath!.Copy();
                        Interlocked.Exchange(ref this.path, copy); //copy back
                        Polyline? start;
                        Line[] lines = CreatePathSegments(currentPath, patternPanel, out start);
                        patternPanel.Children.Clear();
                        if (start != null) {
                            Canvas.SetLeft(start, 0);
                            Canvas.SetTop(start, 0);
                            AddChild(patternPanel, start);
                        }
                        foreach (Line l in lines)
                        {
                            Canvas.SetLeft(l, 0);
                            Canvas.SetTop(l, 0);
                            AddChild(patternPanel, l);
                        }
                    }
                });
            }
        }

        public void DispButton_Click(object sender, RoutedEventArgs e)
        {
            beenRun = true;
            int rows = 0, cols = 0;
            bool inputs = int.TryParse(rowsBox.Text, out rows) &&
                int.TryParse(colsBox.Text, out cols) && outputBox.Text.Any();
            if (inputs)
            {
                HexGraph graph = new(rows, cols);
                Graphs.Path? path = Graphs.Path.FromString(outputBox.Text, graph);
                Interlocked.Exchange(ref this.path, path);
                if (path != null)
                {
                    patternPanel.Dispatcher.BeginInvoke(() =>
                    {
                        Graphs.Path? currentPath = Interlocked.Exchange(ref this.path, null);
                        if (currentPath != null)
                        {
                            Graphs.Path copy = currentPath!.Copy();
                            Interlocked.Exchange(ref this.path, copy); //copy back
                            Polyline? start;
                            Line[] lines = CreatePathSegments(currentPath, patternPanel, out start);
                            patternPanel.Children.Clear();
                            if (start != null)
                            {
                                Canvas.SetLeft(start, 0);
                                Canvas.SetTop(start, 0);
                                AddChild(patternPanel, start);
                            }
                            foreach (Line l in lines)
                            {
                                Canvas.SetLeft(l, 0);
                                Canvas.SetTop(l, 0);
                                AddChild(patternPanel, l);
                            }
                        }
                    });
                }
            }
        }

        public void GenButton_Click(object sender, RoutedEventArgs e)
        {
            beenRun = true;
            int rows = 0, cols = 0, target = 0, carryOver = 0;
            bool inputs = int.TryParse(rowsBox.Text, out rows) &&
                int.TryParse(colsBox.Text, out cols) &&
                int.TryParse(targetBox.Text, out target) &&
                int.TryParse(carryBox.Text, out carryOver);
            if (inputs)
            {
                this.Title = "Hex Casting Pattern Generator - Generating";
                HexGraph graph = new(rows, cols);
                PathGenBase gen;
                if(genAStar == 0)
                    gen = new PathGenerator(target, graph, carryOver); //beam search
                else
                    gen = new PathGenAStar(target, graph); //A*
                Graphs.Path? path = gen.FindPath(); //todo move to new thread
                Interlocked.Exchange(ref this.path, path);
                outputBox.Text = path?.ToString() ?? "No path found";
                if (path != null)
                {
                    patternPanel.Dispatcher.BeginInvoke(() =>
                    {
                        Graphs.Path? currentPath = Interlocked.Exchange(ref this.path, null);
                        if (currentPath != null)
                        {
                            Graphs.Path copy = currentPath!.Copy();
                            Interlocked.Exchange(ref this.path, copy); //copy back
                            Polyline? start;
                            Line[] lines = CreatePathSegments(currentPath, patternPanel, out start);
                            patternPanel.Children.Clear();
                            foreach (Line l in lines)
                            {
                                patternPanel.Children.Add(l);
                                Canvas.SetLeft(l, 0);
                                Canvas.SetTop(l, 0);
                                //AddChild(patternPanel, l);
                            }
                            if (start != null)
                            {
                                patternPanel.Children.Add(start);
                                Canvas.SetLeft(start, 0);
                                Canvas.SetTop(start, 0);
                                //AddChild(patternPanel, start);
                            }
                        }
                    });
                }
                this.Title = "Hex Casting Pattern Generator";
            }
        }

        public async void GenButton_Click2(object sender, RoutedEventArgs e)
        {
            beenRun = true;
            int rows = 0, cols = 0, target = 0, carryOver = 0;
            bool inputs = int.TryParse(rowsBox.Text, out rows) &&
                int.TryParse(colsBox.Text, out cols) &&
                int.TryParse(targetBox.Text, out target) &&
                int.TryParse(carryBox.Text, out carryOver);
            if (inputs)
            {
                this.Title = "Hex Casting Pattern Generator - Generating";
                HexGraph graph = new(rows, cols);
                PathGenBase gen;
                if (genAStar == 0)
                    gen = new PathGenerator(target, graph, carryOver); //beam search
                else
                    gen = new PathGenAStar(target, graph); //A*
                await GenPathAsync(gen).ContinueWith((t) =>
                {
                    Interlocked.Exchange(ref this.path, t.Result);
                    if (path != null)
                    {
                        patternPanel.Dispatcher.Invoke(() =>
                        {
                            Graphs.Path? currentPath = Interlocked.Exchange(ref this.path, null);
                            if (currentPath != null)
                            {
                                Graphs.Path copy = currentPath!.Copy();
                                Interlocked.Exchange(ref this.path, copy); //copy back
                                Polyline? start;
                                patternPanel.Children.Clear();
                                Line[] lines = CreatePathSegments(currentPath, patternPanel, out start);
                                foreach (Line l in lines)
                                {
                                    AddChild(patternPanel, l);
                                    Canvas.SetLeft(l, 0);
                                    Canvas.SetTop(l, 0);
                                }
                                if (start != null)
                                {
                                    AddChild(patternPanel, start);
                                    Canvas.SetLeft(start, 0);
                                    Canvas.SetTop(start, 0);
                                }
                            }
                            outputBox.Text = path?.ToString() ?? "No path found";
                            this.Title = "Hex Casting Pattern Generator";
                        });
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private async Task<Graphs.Path?> GenPathAsync(PathGenBase gen)
        {
            return await Task.Run(() => {
                return gen.FindPath();
            });
        }

        public static void AddChild(Visual parent, UIElement child)
        {
            if (InternalAddChild(parent, child))
            {
                return;
            }
            throw new NotSupportedException();
        }
        private static bool InternalAddChild(Visual parent, UIElement child)
        {
            Panel? panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Add(child);
                return true;
            }
            for (int i = VisualTreeHelper.GetChildrenCount(parent) - 1; i != -1; i--)
            {
                Visual? target = VisualTreeHelper.GetChild(parent, i) as Visual;
                if (target != null && InternalAddChild(target, child))
                {
                    return true;
                }
            }
            return false;
        }

        public static Line[] CreatePathSegments(Graphs.Path p, Canvas patternPanel, out Polyline? start) 
        {
            List<Line> segments = new();
            Bounds bounds = GetSpacingAround(p, patternPanel);
            List<Node> nodes = p.GetNodesCopy();
            Color[] colors = FourColorStops(p);
            int antialiasing = Interlocked.Exchange(ref antialias, -1);
            Interlocked.Exchange(ref antialias, antialiasing); //write back
            bool first = true;
            start = null;
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                double x1, x2, y1, y2;
                (x1, y1) = ConvertToScreenCoords(nodes[i].u, nodes[i].v, bounds);
                (x2, y2) = ConvertToScreenCoords(nodes[i + 1].u, nodes[i + 1].v, bounds);
                if (first)
                {
                    start = GetStartMark(x1, y1, x2, y2, bounds.spacing);
                    first = false;
                }
                GradientStopCollection stopColl = new()
                {
                    new(colors[i], 0),
                    new(colors[i], 0.4),
                    new(colors[i+1], 0.6),
                    new(colors[i+1], 1)
                };
                LinearGradientBrush brush = new(stopColl, new Point(x1, y1), new Point(x2, y2));
                brush.MappingMode = BrushMappingMode.Absolute;
                Line ln = new()
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = brush,
                    StrokeThickness =  /**/ Math.Max(3, bounds.spacing / 36.0),//*/ 30,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round
                };
                if(antialiasing == 0)
                    RenderOptions.SetEdgeMode(ln, EdgeMode.Aliased);
                segments.Add(ln);
            }
            segments.Reverse();
            return segments.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">The x coordinate of the mark</param>
        /// <param name="y">The y coordinate of the mark</param>
        /// <param name="x2">The x coordinate the mark points to</param>
        /// <param name="y2">The y coordinate the mark points to</param>
        /// <param name="spacing">The spacing between nodes</param>
        /// <returns></returns>
        public static Polyline GetStartMark(double x, double y, double x2, double y2, double spacing)
        {
            //We want an equilateral triangle with height probably about 0.2*spacing
            double dirLen = Math.Sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y));
            double dx = (x2 - x) / dirLen;
            double dy = (y2 - y) / dirLen;
            //{dx, dy} is the normalized vector between x2 and y2
            //now to rescale it to make it where one point will sit
            dx *= spacing / 12.0;
            dy *= spacing / 12.0;
            //need to get the rotated versions. What's the 60 degree rotation transformation again?
            //[ 0.5          sqrt(0.75) ]
            //[ sqrt(0.75)   -0.5       ]
            //this is clockwise, counter is with the sqrt(0.75) signs changed
            //wait, I need 120, not 60
            //[ -0.5        -sqrt(0.75)     ]
            //[ sqrt(0.75)  -0.5            ]
            //again, opposite signs on the roots for opposite direction
            double root = Math.Sqrt(0.75);
            double dx2, dy2, dx3, dy3;
            dx2 = dx * -0.5 + dy * root;
            dy2 = dx * -root + dy * -0.5;
            dx3 = dx * -0.5 + dy * -root;
            dy3 = dx * root + dy * -0.5;
            //where in the triangle is the center, anyways?
            //1/3 of the height
            PointCollection points = new()
            {
                new Point(dx2 + x + dx*6, dy2 + y + dy*6),
                new Point(dx + x + dx*6, dy + y + dy*6),
                new Point(dx3 + x + dx*6, dy3 + y + dy*6),
                new Point(dx2 + x + dx*6, dy2 + y + dy*6)
            };
            int startColor = Interlocked.Exchange(ref color1, -1);
            Interlocked.Exchange(ref color1, startColor);//write back
            SolidColorBrush brush = new(Color.FromRgb((byte) (startColor >> 16), (byte)((startColor >> 8) & 255), (byte)(startColor & 255)));
            Polyline poly = new()
            {
                Points = points,
                Stroke = brush,
                StrokeThickness = 1,
                StrokeLineJoin = PenLineJoin.Miter,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                Fill = brush
            };
            int antialiasing = Interlocked.Exchange(ref antialias, -1);
            Interlocked.Exchange(ref antialias, antialiasing); //write back
            if (antialiasing == 0)
                RenderOptions.SetEdgeMode(poly, EdgeMode.Aliased);
            return poly;
        }

        public static Color[] FourColorStops(Graphs.Path path)
        {
            Node[] nodes = path.GetNodesCopy().ToArray();

            Color[] colors = new Color[4];
            //load the colors saved as integers in the main thread
            int color = Interlocked.Exchange(ref color1, -1);
            Interlocked.Exchange(ref color1, color);
            colors[0] = Color.FromRgb((byte)(color >> 16), (byte)((color >> 8) & 255), (byte)(color & 255));
            color = Interlocked.Exchange(ref color2, -1);
            Interlocked.Exchange(ref color2, color);
            colors[1] = Color.FromRgb((byte)(color >> 16), (byte)((color >> 8) & 255), (byte)(color & 255));
            color = Interlocked.Exchange(ref color3, -1);
            Interlocked.Exchange(ref color3, color);
            colors[2] = Color.FromRgb((byte)(color >> 16), (byte)((color >> 8) & 255), (byte)(color & 255));
            color = Interlocked.Exchange(ref color4, -1);
            Interlocked.Exchange(ref color4, color);
            colors[3] = Color.FromRgb((byte)(color >> 16), (byte)((color >> 8) & 255), (byte)(color & 255)); //the one not in the old pattern

            Color currentColor = colors[0];
            int currentDex = 0;

            Dictionary<Tuple<int, int>, List<int>> colorsReached = new(); //uses the coords and indices just to be sure it's matching correctly

            //As before, each node gets a gradient stop, offset wont matter since the offset will be set later
            //may as well return colors honestly, but oh well
            Color[] stops = new Color[nodes.Length];
            for(int i = 0; i < nodes.Length; i++)
            {
                Tuple<int, int> coords = new(nodes[i].u, nodes[i].v);
                if (!colorsReached.ContainsKey(coords))
                {
                    colorsReached.Add(coords, new());
                }
                else
                {
                    //if we've been here before, make sure we haven't been here in this color
                    while (colorsReached[coords].Count != 4 && colorsReached[coords].Contains(currentDex))
                    {   //should never be infinite, but may as well make sure
                        currentDex++;
                        currentDex %= 4;
                        currentColor = colors[currentDex];
                    }
                }
                colorsReached[coords].Add(currentDex);
                stops[i] = currentColor;
            }
            return stops;
        }

        public static Tuple<double, double> ConvertToScreenCoords(int row, int diag, Bounds bounds)
        {
            double col = diag + 0.5 * row;
            double y = bounds.topMargin + (row - bounds.minRow) * bounds.spacing * Math.Sqrt(0.75);
            double x = bounds.leftMargin + (col - bounds.minCol) * bounds.spacing;
            return new(x, y);
        }

        public static Bounds GetSpacingAround(Graphs.Path p, Canvas patternPanel)
        {
            //first, find the width and height covered
            double minDiag = double.MaxValue;
            double minCol = double.MaxValue, maxCol = double.MinValue;
            double minRow = double.MaxValue, maxRow = double.MinValue;
            foreach (Node n in p.GetNodesCopy())
            {
                double row = n.u;
                double col = n.v + 0.5 * n.u;
                minCol = Math.Min(minCol, col);
                minRow = Math.Min(minRow, row);
                maxCol = Math.Max(maxCol, col);
                maxRow = Math.Max(maxRow, row);
                minDiag = Math.Min(minDiag, n.v);
            }
            double spacing;
            double hSpacing = patternPanel.RenderSize.Width / (maxCol - minCol);
            double vSpacing = patternPanel.RenderSize.Height / ((maxRow - minRow + 1) * Math.Sqrt(0.75));
            spacing = Math.Min(hSpacing, vSpacing);
            double leftMargin = (patternPanel.RenderSize.Width - spacing * (maxCol - minCol)) * 0.5;
            double topMargin = (patternPanel.RenderSize.Height - spacing * (maxRow - minRow) * Math.Sqrt(0.75)) * 0.5;
            //leftMargin = 0;
            //topMargin = 0;
            Bounds b = new()
            {
                spacing = spacing,
                leftMargin = leftMargin,
                topMargin = topMargin,
                minRow = minRow,
                minDiag = minDiag,
                minCol = minCol
            };
            return b;
        }
    }

    public class Bounds
    {
        public double topMargin, leftMargin, spacing;
        public double minRow, minDiag, minCol;
    }
}
