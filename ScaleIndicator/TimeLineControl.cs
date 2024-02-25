using CsvHelper;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScaleIndicator
{
    public class TimeLineControl : Grid
    {
        public TimeLineControl()
        {
            Loaded += TimeLineControl_Loaded;
            SizeChanged += TimeLineControl_SizeChanged;
        }

        public static readonly DependencyProperty ColorsFilePathProperty =
            DependencyProperty.Register("ColorsFilePath", typeof(string), typeof(TimeLineControl));

        public string ColorsFilePath
        {
            get { return (string)GetValue(ColorsFilePathProperty); }
            set { SetValue(ColorsFilePathProperty, value); }
        }



        public Color DefautlTimeLineColor
        {
            get { return (Color)GetValue(DefautlTimeLineColorProperty); }
            set { SetValue(DefautlTimeLineColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefautlTimeLineColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefautlTimeLineColorProperty =
            DependencyProperty.Register("DefautlTimeLineColor", typeof(Color), typeof(TimeLineControl), new PropertyMetadata(Colors.AliceBlue));


        public static readonly DependencyProperty GridBackgroundColorProperty =
            DependencyProperty.Register("GridBackgroundColor", typeof(Color), typeof(TimeLineControl), new PropertyMetadata(Colors.WhiteSmoke));

        public Color GridBackgroundColor
        {
            get { return (Color)GetValue(GridBackgroundColorProperty); }
            set { SetValue(GridBackgroundColorProperty, value); }
        }        

        // Using a DependencyProperty as the backing store for ColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnWidthProperty =
            DependencyProperty.Register("ColumnWidth", typeof(double), typeof(TimeLineControl), new PropertyMetadata(0.0, OnColumnWidthChanged));

        public double ColumnWidth
        {
            get { return (double)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!double.IsNaN((double)e.NewValue))
            {
                TimeLineControl control = (TimeLineControl)d as TimeLineControl;
                control.Width = (double)e.NewValue;
                AdjustGridColumnsWidth(d as Grid, (double)e.NewValue);
            }
        }
        
        public static readonly DependencyProperty UpdateScaleMethodProperty =
            DependencyProperty.Register("UpdateScaleMethod", typeof(Action), typeof(TimeLineControl));

        public Action UpdateScaleMethod
        {
            get { return (Action)GetValue(UpdateScaleMethodProperty); }
            set { SetValue(UpdateScaleMethodProperty, value); }
        }       

        // Using a DependencyProperty as the backing store for Data. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ObservableCollection<object>), typeof(TimeLineControl), new PropertyMetadata(default(ObservableCollection<object>), OnDataPropertyChanged));

        public ObservableCollection<object> Data
        {
            get { return (ObservableCollection<object>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TimeLineControl;
            if (control != null)
            {
                control.OnDataChanged(new DataChangedEventArgs<object>((object)e.OldValue, (object)e.NewValue));
            }
        }

        public event EventHandler<DataChangedEventArgs<object>> DataChanged;

        protected virtual void OnDataChanged(DataChangedEventArgs<object> e)
        {
            DataChanged?.Invoke(this, e);
        }        

        private void TimeLineControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //AdjustGridColumnsWidth();
        }

        private void TimeLineControl_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateGridColumns();
        }

        public void ResetScale()
        {
            try
            {
                if (this == null)
                    return;

                this.Children.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void GenerateGridColumns()
        {
            if (this == null)
                return;

            this.Background = new SolidColorBrush(DefautlTimeLineColor);
            CreatePercentageColumnsFromCSV(this);
        }

        private static void AdjustGridColumnsWidth(Grid grid,double width)
        {
            try
            {
                if (grid == null)
                    return;

                List<ColumnDefinition> existingColumns = grid.ColumnDefinitions.ToList();

                double columnWidth = width / existingColumns.Count;
                foreach (ColumnDefinition column in existingColumns)
                {
                    column.Width = new GridLength(columnWidth);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreatePercentageColumnsFromCSV(Grid percentageGrid)
        {
            try
            {
                List<int> allNumbers = Enumerable.Range(0, 100).ToList();
                CreateColumns(percentageGrid, allNumbers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReGenerateColumns(int range)
        {
            List<int> allNumbers = Enumerable.Range(0, range).ToList();
            CreateColumns(this, allNumbers);
        }

        private void CreateColumns(Grid percentageGrid, List<int> allNumbers)
        {
            GridLength grsLenth = new GridLength(percentageGrid.Width / (allNumbers.Count));
            allNumbers.ForEach(number =>
            {
                var pct = PercentColors.FirstOrDefault(x => x.Index == number);
                if (pct != null)
                {
                    AddGridWithColumnColor(percentageGrid, grsLenth, number, pct.Color);
                }
                else
                {
                    AddGridWithColumnColor(percentageGrid, grsLenth, number, GridBackgroundColor);
                }
            });
        }

        private void AddGridWithColumnColor(Grid percentageGrid, GridLength grsLenth, int percentage, Color columnColor)
        {
            ColumnDefinition column = new ColumnDefinition();
            column.Width = grsLenth;
            percentageGrid.ColumnDefinitions.Add(column);
            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(columnColor),
            };
            rectangle.Style = FindResource("RectangleStyle") as Style;
            rectangle.Tag = new Indexes() { Index = percentage };
            rectangle.VerticalAlignment = VerticalAlignment.Center;
            rectangle.ToolTip = $"{percentage + 1}%";
            rectangle.MouseDown += Rectangle_MouseDown;
            Grid.SetColumn(rectangle, percentage);
            percentageGrid.Children.Add(rectangle);
        }

        public void SelectGridColumnByListIndex(int gridSelectedIndex)
        {
            foreach (var columnDefinition in ColumnDefinitions)
            {
                
            }
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
                    OnClickedColor?.Invoke(this, ergs);
                }
            }
            else
            {
                MessageBox.Show("Not Found");
            }
        }

        public event EventHandler<ColorEventArgs> OnClickedColor;

        private List<IndexedColor> _percentColors;
        public List<IndexedColor> PercentColors
        {
            get
            {
                if (_percentColors == null)
                {
                    _percentColors = ReadColorsFromCSV(ColorsFilePath);
                }
                return _percentColors;
            }
        }

        public void UpdateScaleColorAt(System.Windows.Media.Color color, int startIndex, int count, int listIndex)
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
                var rect = this.Children.OfType<System.Windows.Shapes.Rectangle>().FirstOrDefault(rect => Grid.GetColumn(rect) == number);
                if (rect != null)
                {
                    var indexes = (Indexes)rect.Tag;
                    indexes.ListIndex = listIndex;
                    indexes.StartIndex = startIndex;
                    indexes.EndIndex = startIndex + count;
                    rect.Fill = new SolidColorBrush(transparent);
                    rect.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, color.A, color.G, color.B));
                }
            });
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
    }
    public class IndexedColor
    {
        public int Index { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public System.Windows.Media.Color Color => System.Windows.Media.Color.FromRgb(Red, Green, Blue);
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

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class DataChangedEventArgs<T> : EventArgs
    {
        public object OldValue { get; }
        public object NewValue { get; }

        public DataChangedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
