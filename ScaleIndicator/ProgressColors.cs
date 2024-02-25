using CsvHelper;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace ScaleIndicator
{
    public class ProgressColors : Control
    {
        // Custom event and delegate
        public event EventHandler<ColorEventArgs> OnClickedolor;

        public string colors_file { get; set; } = String.Empty;
        public List<IndexedColor> percentColors = new List<IndexedColor>();
        public Color grid_background { get; set; } = Colors.AliceBlue;

        public ProgressColors(string color_file_path)
        {
            this.colors_file = color_file_path;
            this.percentColors = ReadColorsFromCSV(color_file_path);
        }
        public ProgressColors(string color_file_path, Color color)
        {
            this.colors_file = color_file_path;
            this.grid_background = color;
            this.percentColors = ReadColorsFromCSV(color_file_path);
        }

        public void GenerateGridColumns(UIElement control)
        {
            if (control == null)
            {
                return;
            }
            if (control.GetType() != typeof(Grid))
            {
                return;
            }
            if (control is Grid grd)
            {
                grd.Background = new SolidColorBrush(this.grid_background);
                CreatePercentageColumnsFromCSV(grd);
            }
        }
        private void CreatePercentageColumnsFromCSV(Grid percentageGrid)
        {
            try
            {
                List<int> allNumbers = Enumerable.Range(0, 100).ToList();
                GridLength grsLenth = new GridLength(percentageGrid.Width / (allNumbers.Count));
                allNumbers.ForEach(number =>
                {
                    var pct = percentColors.FirstOrDefault(x => x.Index == number);
                    if (pct != null)
                    {
                        AddGridWithColumnColor(percentageGrid, grsLenth, number, pct.Color);
                    }
                    else
                    {
                        AddGridWithColumnColor(percentageGrid, grsLenth, number, Colors.Yellow);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddGridWithColumnColor(Grid percentageGrid,GridLength grsLenth, int percentage, Color columnColor)
        {
            ColumnDefinition column = new ColumnDefinition();
            column.Width = grsLenth;
            percentageGrid.ColumnDefinitions.Add(column);
            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(columnColor),
            };
            ResourceDictionary resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/ScaleIndicator;component/ProgressTimerDictionary.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            rectangle.Style = Application.Current.Resources["RectangleStyle"] as Style;
            rectangle.Tag = new Indexes() { Index = percentage };
            rectangle.VerticalAlignment = VerticalAlignment.Center;
            rectangle.ToolTip = $"{percentage + 1}%";
            rectangle.MouseDown += Rectangle_MouseDown;
            Grid.SetColumn(rectangle, percentage);
            percentageGrid.Children.Add(rectangle);
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ret = sender as Rectangle;
            if (ret != null)
            {
                SolidColorBrush brush = (SolidColorBrush)ret.Fill;
                if (brush.Color != Colors.Transparent)
                {
                    ColorEventArgs ergs = new ColorEventArgs(ret.Fill, (Indexes)ret.Tag);
                    OnClickedolor?.Invoke(this, ergs);
                }
            }
            else
            {
                MessageBox.Show("Not Found");
            }
        }

        public void updateGridColumnColor(Grid percentageGrid, int percentage, Color columnColor)
        {
            GetRectangleByColumnIndex(percentageGrid, percentage, columnColor);            
        }

        private void GetRectangleByColumnIndex(Grid grid, int columnIndex, Color color)
        {
            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            {
                var rect = grid.Children.OfType<Rectangle>().FirstOrDefault(rect => Grid.GetColumn(rect) == columnIndex);
                if (rect != null)
                {
                    rect.Fill = new SolidColorBrush(color);
                }
            }));
        }
        private List<IndexedColor> ReadColorsFromCSV(string csvFilePath)
        {
            if (File.Exists(csvFilePath) == false)
            {
                return new List<IndexedColor>();
            }
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<IndexedColor>().ToList();
            }
        }
        public void UpdateScaleColorAt(Grid percentageGrid, Color color, int startIndex, int count, int listIndex)
        {
            if (startIndex <= 0)
            {
                return;
            }
            startIndex = (startIndex - 1);
            var transparent = color;
            List<int> allNumbers = Enumerable.Range(startIndex, count).ToList();
            allNumbers.ForEach(number =>
            {
                var rect = percentageGrid.Children.OfType<Rectangle>().FirstOrDefault(rect => Grid.GetColumn(rect) == number);
                if (rect != null)
                {
                    var indexes = (Indexes)rect.Tag;
                    indexes.ListIndex = listIndex;
                    rect.Fill = new SolidColorBrush(transparent);
                    rect.Stroke = new SolidColorBrush(Color.FromArgb(100,color.A, color.G, color.B));
                }
            });
        }

    }
    public class IndexedColor
    {
        public int Index { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public Color Color => Color.FromRgb(Red, Green, Blue);
    }
    // Custom EventArgs class to pass additional data (color in this case)
    public class ColorEventArgs : EventArgs
    {
        public Indexes Index;

        public Brush Color { get; }

        public ColorEventArgs(Brush color, Indexes index)
        {
            Color = color;
            Index = index;
        }

        public override string ToString()
        {
            return $"Index: {Index}, Color: {Color}";
        }
    }

    public class Indexes
    {
        public int Index { get; set; }
        public int ListIndex { get; set; }
    }
}
