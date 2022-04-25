using LiveCharts;
using LiveCharts.Wpf;
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
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using System.ComponentModel;
using System.Globalization;

namespace MiniProjekat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public SeriesCollection LineSeriesCollection { get; set; }
        public SeriesCollection ColumnSeriesCollection { get; set; }
        public List<string> LineLabels { get; set; }
        public List<string> ColumnLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> Formatter { get; set; }

        private List<String> GDPIntervals = new List<String>() { "Quarterly", "Annual" };
        private List<String> TreasuryIntervals = new List<String>() { "Daily", "Weekly", "Monthly" };

        private DataHandler dataHandler = new DataHandler();
        private DataHandler.Data Data { get; set; }

        private Settings CurrentSettings { get; set; }

        private ZoomingOptions _zoomingMode;

        private TableView tableView;
        public ZoomingOptions ZoomingMode
        {
            get { return _zoomingMode; }
            set
            {
                _zoomingMode = value;
                OnPropertyChanged();
            }
        }

        private readonly int MAX_ENTRIES = 25;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            LineLabels = new List<string>();
            ColumnLabels = new List<string>();

            EntriesLabel.Content =  $"*max shown entries: {MAX_ENTRIES}";

            List<String> dataReferences = new List<String>() { "GDP", "Treasury Yields" };
            dataReferencePicker.ItemsSource = dataReferences;
            dataReferencePicker.SelectedIndex = 0;

            List<String> treasuryMaturities = new List<String>() { "3 Month", "2 Year", "5 Year", "7 Year", "10 Year", "30 Year" };
            maturityPicker.ItemsSource = treasuryMaturities;
            maturityPicker.SelectedIndex = 0;

            intervalPicker.ItemsSource = GDPIntervals;
            intervalPicker.SelectedIndex = 0;
            ZoomingMode = ZoomingOptions.Xy;
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataReferencePicker.SelectedValue != null && intervalPicker.SelectedValue != null)
            {
                DateTime? startDate, endDate;

                DataReference dataReference = ParseDataReference(dataReferencePicker.SelectedValue.ToString());
                var interval = ParseInterval(intervalPicker.SelectedValue.ToString());
                var maturity = ParseMaturity(maturityPicker.SelectedValue.ToString());

                startDate = startDatePicker.SelectedDate != null ? startDatePicker.SelectedDate.Value.Date : (DateTime?)null;
                endDate = endDatePicker.SelectedDate != null ? endDatePicker.SelectedDate.Value.Date : (DateTime?)null;

                // fill charts

                if (startDate > endDate)
                {
                    MessageBox.Show("Error: Start date can't be after end date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newSetttings = new Settings
                {
                    DataReference = dataReference,
                    Interval = interval,
                    Start = startDate,
                    End = endDate,
                    Maturity = maturity
                };

                if (CurrentSettings == null || !newSetttings.Equals(CurrentSettings))
                {
                    System.Diagnostics.Debug.Write($"{CurrentSettings} {newSetttings}");
                    CurrentSettings = newSetttings;
                    if (!ComputeData())
                    {
                        return;
                    }
                    if(Data.Values.Count == 0)
                    {
                        MessageBox.Show("No data", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    Clear();
                    DrawCharts();
                    UpdateTable(Data);
                }
                else
                {
                    if (LineSeriesCollection.Count == 0)
                    {
                        if(Data == null || Data.Values == null)
                        {
                            if(!ComputeData())
                            {
                                Clear();
                                return;
                            }
                        }
                        if (Data.Values.Count == 0)
                        {
                            MessageBox.Show("No data", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            Clear();
                            return;
                        }
                        Clear();
                        DrawCharts();
                        UpdateTable(Data);
                    }
                }
            }
        }

        private void UpdateTable(DataHandler.Data data)
        {
            if (tableView != null && Data != null)
            {
                tableView.Update(Data);
            }
        }

        private bool ComputeData()
        {
            if (CurrentSettings.DataReference == DataReference.GDP)
            {
                Data = dataHandler.getGDP((GDP_INTERVAL)CurrentSettings.Interval);
            }
            else
            {
                Data = dataHandler.getTreasuryYield((TREASURY_INTERVAL)CurrentSettings.Interval, CurrentSettings.Maturity);
            }
            Y1.Title = (dataHandler.Units == null ? "" : dataHandler.Units);
            Y2.Title = (dataHandler.Units == null ? "" : dataHandler.Units);

            if (Data == null)
            {
                MessageBox.Show("Error: Too many requests. Try again in 60 seconds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentSettings = null;
                return false;
            }


            FilterDataByDates();
            Data.Values.Reverse();
            Data.Dates.Reverse();
            Data.Values = Data.Values.Take(MAX_ENTRIES).ToList();
            Data.Dates = Data.Dates.Take(MAX_ENTRIES).ToList();
            Data.Values.Reverse();
            Data.Dates.Reverse();
            return true;
        }

        private void FilterDataByDates()
        {
            var format = "yyyy-MM-dd";
            var provider = CultureInfo.InvariantCulture;
            var dates = Data.Dates.Select(x => DateTime.ParseExact(x, format, provider));
            var values = Data.Values;
            DateTime? startDate = CurrentSettings.Start == null ? DateTime.MinValue : CurrentSettings.Start;
            DateTime? endDate = CurrentSettings.End == null ? DateTime.MaxValue : CurrentSettings.End;
            Dictionary<DateTime, double> dict = dates.Zip(values, (k, v) => new { k, v })
                                                     .ToDictionary(x => x.k, x => x.v);

            var filtered = dict.Where(x => x.Key >= startDate.Value && x.Key <= endDate.Value)
                               .ToDictionary(x => x.Key, x => x.Value);

            var sortedFiltered = new SortedDictionary<DateTime, double>(filtered).OrderBy(x => x.Key);

            Data.Values = sortedFiltered.Select(x => x.Value).ToList();
            Data.Dates = sortedFiltered.Select(x => x.Key.ToString("yyyy-MM-dd")).ToList();
        }

        private void DrawCharts()
        {
            SeparatorLine.Step = Math.Max(Data.Values.Count / 3, 1);
            SeparatorColumn.Step = Math.Max(Data.Values.Count / 3, 1);
            DrawLineChart();
            DrawColumnChart();
            DataContext = this;
        }

        private void DrawColumnChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMin = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#90EE02");

            var chartValues = new ChartValues<double>();
            var values = Data.Values;
            chartValues.AddRange(values);

            if (ColumnSeriesCollection == null)
                ColumnSeriesCollection = new SeriesCollection();
            ;
            var series = new ColumnSeries
            {
                Title = CurrentSettings.DataReference.ToString() + "\n" + CurrentSettings.Interval.ToString(),
                Values = chartValues,
                StrokeThickness = 2,
                PointGeometry = DefaultGeometries.Square
            };

            var minSeries = new ColumnSeries
            {
                Title = "MIN. VAL.",
                Stroke = brushMin,
                Fill = brushMin,
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Square
            };

            var maxSeries = new ColumnSeries
            {
                Title = "MAX. VAL.",
                Stroke = brushMax,
                Fill = brushMax,
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Square
            };

            ColumnSeriesCollection.Add(series);
            ColumnSeriesCollection.Add(minSeries);
            ColumnSeriesCollection.Add(maxSeries);

            double maxValue = values.Count() > 0 ? values.Max() : 0;
            double minValue = values.Count() > 0 ? values.Min() : 0;

            var mapper = new CartesianMapper<double>()
                        .X((value, index) => index)
                        .Y((value) => value)
                        .Fill((value, index) =>
                        {
                            return GetMinMaxBrush(value, maxValue, minValue);
                        })
                        .Stroke((value, index) =>
                        {
                            return GetMinMaxBrush(value, maxValue, minValue);
                        });


            ColumnLabels.AddRange(Data.Dates);

            Formatter = (value) =>
            {
                if (CurrentSettings == null || CurrentSettings.Interval == null)
                    return value.ToString();
                if (CurrentSettings.Interval.GetType() == typeof(GDP_INTERVAL))
                    return value.ToString() + "B$";
                else
                    return value.ToString() + "%";
            };
        }

        private void DrawLineChart()
        {

            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMin = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#90EE02");

            if (LineSeriesCollection == null)
                LineSeriesCollection = new SeriesCollection();

            var chartValues = new ChartValues<double>();
            var values = Data.Values;
            chartValues.AddRange(values);

            LineSeries lineSeries = new LineSeries
            {
                Title = CurrentSettings.DataReference.ToString() + "\n" + CurrentSettings.Interval.ToString(),
                Values = chartValues,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                PointForeground = brush
            };

            var minSeries = new LineSeries
            {
                Title = "MIN. VAL.",
                Stroke = brushMin,
                Fill = brushMin,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                PointForeground = brushMin,
                Values = new ChartValues<double>(),
            };

            var maxSeries = new LineSeries
            {
                Title = "MAX. VAL.",
                Stroke = brushMax,
                Fill = brushMax,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                PointForeground = brushMax,
                Values = new ChartValues<double>(),
            };

            LineSeriesCollection.Add(lineSeries);
            LineSeriesCollection.Add(minSeries);
            LineSeriesCollection.Add(maxSeries);


            double maxValue = values.Count() > 0 ? values.Max() : 0;
            double minValue = values.Count() > 0 ? values.Min() : 0;

            var mapper = new CartesianMapper<double>()
                        .X((value, index) => index)
                        .Y((value) => value)
                        .Fill((value, index) =>
                        {
                            return GetMinMaxBrush(value, maxValue, minValue);
                        })
                        .Stroke((value, index) =>
                        {
                            return GetMinMaxBrush(value, maxValue, minValue);
                        });
            Charting.For<double>(mapper, SeriesOrientation.All);
            LineLabels.AddRange(Data.Dates);
            YFormatter = (value) =>
            {
                if (CurrentSettings == null || CurrentSettings.Interval == null)
                    return value.ToString();
                if (CurrentSettings.Interval.GetType() == typeof(GDP_INTERVAL))
                    return value.ToString() + "B$";
                else
                    return value.ToString() + "%";
            };
        }

        private static object GetMinMaxBrush(double value, double maxValue, double minValue)
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMin = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#90EE02");
            if (value == maxValue)
                return brushMax;
            else if (value == minValue)
                return brushMin;
            else
                return brush;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // clear charts etc.
            Clear();
        }

        private void Clear()
        {
            X1.MinValue = double.NaN;
            X1.MaxValue = double.NaN;
            Y1.MinValue = double.NaN;
            Y1.MaxValue = double.NaN;
            X2.MinValue = double.NaN;
            X2.MaxValue = double.NaN;
            Y2.MinValue = double.NaN;
            Y2.MaxValue = double.NaN;
            LineLabels?.Clear();
            ColumnLabels?.Clear();
            LineSeriesCollection?.Clear();
            ColumnSeriesCollection?.Clear();
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            if (Data == null || LineSeriesCollection == null || LineSeriesCollection.Count() == 0 || Data.Values.Count == 0)
            {
                MessageBox.Show("Error: No data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (!Application.Current.Windows.Cast<Window>().Any(x => x == tableView))
                {
                    tableView = new TableView(Data);
                    tableView.TableUnits.Header = (dataHandler.Units == null ? "" : dataHandler.Units);
                    tableView.Show();
                }
                else
                {
                    MessageBox.Show("Error: Table View already opened.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void UpdateIntervals(object sender, RoutedEventArgs e)
        {
            DataReference dataReference = ParseDataReference(dataReferencePicker.SelectedValue.ToString());
            if (dataReference == DataReference.GDP)
            {
                intervalPicker.ItemsSource = GDPIntervals;
                intervalPicker.SelectedIndex = 0;
                maturityItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                intervalPicker.ItemsSource = TreasuryIntervals;
                intervalPicker.SelectedIndex = 0;
                maturityItem.Visibility = Visibility.Visible;
            }

        }

        public enum DataReference
        {
            GDP, TREASURY
        }
        public DataReference ParseDataReference(string text)
        {
            return text.ToLower().Contains("gdp") ? DataReference.GDP : DataReference.TREASURY;
        }

        public TREASURY_MATURITY ParseMaturity(string text)
        {
            if (text.ToLower().Contains("3 month"))
                return TREASURY_MATURITY.M3;
            else if (text.ToLower().Contains("2 year"))
                return TREASURY_MATURITY.Y2;
            else if (text.ToLower().Contains("5 year"))
                return TREASURY_MATURITY.Y5;
            else if (text.ToLower().Contains("7 year"))
                return TREASURY_MATURITY.Y7;
            else if (text.ToLower().Contains("10 year"))
                return TREASURY_MATURITY.Y10;
            else
                return TREASURY_MATURITY.Y30;
        }

        public Enum ParseInterval(string text)
        {
            if (text.ToLower().Contains("daily"))
                return TREASURY_INTERVAL.DAILY;
            else if (text.ToLower().Contains("weekly"))
                return TREASURY_INTERVAL.WEEKLY;
            else if (text.ToLower().Contains("monthly"))
                return TREASURY_INTERVAL.MONTHLY;
            else if (text.ToLower().Contains("quarterly"))
                return GDP_INTERVAL.QUARTERLY;
            else
                return GDP_INTERVAL.ANNUAL;
        }

        class Settings
        {
            public Enum Interval { get; set; }
            public DataReference DataReference { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }
            public TREASURY_MATURITY Maturity { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Settings settings &&
                       EqualityComparer<Enum>.Default.Equals(Interval, settings.Interval) &&
                       DataReference == settings.DataReference &&
                       Start == settings.Start &&
                       End == settings.End &&
                       Maturity == settings.Maturity;
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }

    }
}
