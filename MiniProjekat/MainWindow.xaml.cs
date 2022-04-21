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

namespace MiniProjekat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public SeriesCollection LineSeriesCollection { get; set; }
        public SeriesCollection ColumnSeriesCollection { get; set; }
        public string[] LineLabels { get; set; }
        public string[] ColumnLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> Formatter { get; set; }

        private List<String> GDPIntervals = new List<String>() { "Quarterly", "Annual" };
        private List<String> TreasuryIntervals = new List<String>() { "Daily", "Weekly", "Monthly"};

        private DataHandler dataHandler = new DataHandler();
        private DataHandler.Data Data { get; set; }

        private Settings CurrentSettings { get; set; }

        private ZoomingOptions _zoomingMode;
        public ZoomingOptions ZoomingMode
        {
            get { return _zoomingMode; }
            set
            {
                _zoomingMode = value;
                OnPropertyChanged();
            }
        }

        private readonly int MAX_ENTRIES = 20;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            List<String> dataReferences = new List<String>() { "GDP", "Treasury Yields" };
            dataReferencePicker.ItemsSource = dataReferences;
            dataReferencePicker.SelectedIndex = 0;

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

                startDate = startDatePicker.SelectedDate != null ? startDatePicker.SelectedDate.Value.Date : (DateTime?) null;
                endDate = endDatePicker.SelectedDate != null ? endDatePicker.SelectedDate.Value.Date : DateTime.Now.Date;

                Console.Out.WriteLine(dataReference + " " + interval);
                Console.Out.WriteLine(startDate + " " + endDate);

                // fill charts
                
                var newSetttings = new Settings
                {
                    DataReference = dataReference,
                    Interval = interval,
                    Start = startDate,
                    End = endDate,
                };

                if(CurrentSettings == null || !newSetttings.Equals(CurrentSettings))
                {
                    ComputeData(dataReference, interval);
                    System.Diagnostics.Debug.Write($"{CurrentSettings} {newSetttings}");
                    CurrentSettings = newSetttings;
                    Clear();
                    DrawCharts();
                }
                else
                {
                    if(LineSeriesCollection.Count == 0)
                    {
                        DrawCharts();
                    }
                }
            }
        }

        private void ComputeData(DataReference dataReference, Enum interval)
        {
            if (dataReference == DataReference.GDP)
            {
                Data = dataHandler.getGDP((GDP_INTERVAL)interval);
            }
            else
            {
                Data = dataHandler.getTreasuryYield((TREASURY_INTERVAL)interval, TREASURY_MATURITY.M3);
            }

            Data.Values = Data.Values.Take(MAX_ENTRIES).ToList();
            Data.Dates = Data.Dates.Take(MAX_ENTRIES).ToList();
        }

        private void DrawCharts()
        {
            DrawLineChart();
            DrawColumnChart();
            DataContext = this;
        }

        private void DrawColumnChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");
            SolidColorBrush brushLight = (SolidColorBrush)new BrushConverter().ConvertFrom("#9caffc");

            var chartValues = new ChartValues<double>();
            var values = Data.Values;
            chartValues.AddRange(values);

            if(ColumnSeriesCollection == null)
                ColumnSeriesCollection = new SeriesCollection();
;
            var series = new ColumnSeries
            {
                Title = CurrentSettings.DataReference.ToString() + "\n" + CurrentSettings.Interval.ToString(),
                Values = chartValues,
                Stroke = brushLight,
                Fill = brushLight,
                StrokeThickness = 2,
            };

            ColumnSeriesCollection.Add(series);

            double maxValue = values.Count() > 0 ? values.Max() : 0;
            double minValue = values.Count() > 0 ? values.Min() : 0;

            var mapper = new CartesianMapper<double>()
                        .X((value, index) => index)
                        .Y((value) => value)
                        .Fill((value, index) =>
                        {
                            if ((value == maxValue) || (value == minValue))
                                return brushMax;
                            else
                                return brushLight;
                        })
                        .Stroke((value, index) =>
                        {
                            if ((value == maxValue) || (value == minValue))
                                return brushMax;
                            else
                                return brush;
                        });


            ColumnLabels = Data.Dates.ToArray();
            Formatter = value => value.ToString("C");
        }

        private void DrawLineChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");
            
            if(LineSeriesCollection == null)
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
                Stroke = brush
            };

            LineSeriesCollection.Add(lineSeries);

            double maxValue = values.Count() > 0 ? values.Max() : 0;
            double minValue = values.Count() > 0 ? values.Min() : 0;

            var mapper = new CartesianMapper<double>()
                        .X((value, index) => index)
                        .Y((value) => value)
                        .Fill((value, index) =>
                        {
                            if ((value == maxValue) || (value == minValue))
                                return brushMax;
                            else
                                return brush;
                        })
                        .Stroke((value, index) =>
                        {
                            if ((value == maxValue) || (value == minValue))
                                return brushMax;
                            else
                                return brush;
                        });
            Charting.For<double>(mapper, SeriesOrientation.All);
            LineLabels = Data.Dates.ToArray();
            YFormatter = value => value.ToString("C");
        }



        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // clear charts etc.
            Clear();
        }

        private void Clear()
        {
            LineSeriesCollection?.Clear();
            ColumnSeriesCollection?.Clear();
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            // show table
        }

        private void UpdateIntervals(object sender, RoutedEventArgs e)
        {
            DataReference dataReference = ParseDataReference(dataReferencePicker.SelectedValue.ToString());
            if (dataReference == DataReference.GDP)
            {
                intervalPicker.ItemsSource = GDPIntervals;
                intervalPicker.SelectedIndex = 0;
            }
            else
            {
                intervalPicker.ItemsSource = TreasuryIntervals;
                intervalPicker.SelectedIndex = 0;
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

            public override bool Equals(object obj)
            {
                return obj is Settings settings &&
                       EqualityComparer<Enum>.Default.Equals(Interval, settings.Interval) &&
                       DataReference == settings.DataReference &&
                       Start == settings.Start &&
                       End == settings.End;
            }

            public override string ToString()
            {
                return $"{Interval} {DataReference} {Start} {End}";
            }
        }

    }
}
