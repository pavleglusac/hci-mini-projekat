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
    public partial class MainWindow : Window
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                endDate = endDatePicker.SelectedDate != null ? endDatePicker.SelectedDate.Value.Date : DateTime.Now;

                Console.Out.WriteLine(dataReference + " " + interval);
                Console.Out.WriteLine(startDate + " " + endDate);

                // fill charts
                if(dataReference == DataReference.GDP)
                {
                    Data = dataHandler.getGDP((GDP_INTERVAL)interval);
                }
                else
                {
                    Data = dataHandler.getTreasuryYield((TREASURY_INTERVAL)interval, TREASURY_MATURITY.M3);
                }

                DrawCharts();
                DrawLineChart();
            }
        }

        private void DrawCharts()
        {
            DrawLineChart();
            DrawColumnChart();
            DataContext = this;
        }

        private void DrawChartsNoContext()
        {
            DrawLineChart();
            DrawColumnChart();
        }

        private void DrawColumnChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");

            var chartValues = new ChartValues<double>();
            var values = Data.Values;
            chartValues.AddRange(values);

            ColumnSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = chartValues,
                    Stroke = brush,
                    Fill = brush
                }
            };

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
                        });


            ColumnLabels = Data.Dates.ToArray();
            Formatter = value => value.ToString("C");
        }

        private void DrawLineChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");
            SolidColorBrush brushMax = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");

            LineSeriesCollection = new SeriesCollection();

            var chartValues = new ChartValues<double>();
            var values = Data.Values;
            Data.Values.ForEach(x => System.Diagnostics.Debug.WriteLine(x));
            chartValues.AddRange(values);

            LineSeries lineSeries = new LineSeries
            {
                Title = "Series 3",
                Values = chartValues,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 15,
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
                        });
            Charting.For<double>(mapper, SeriesOrientation.All);

            LineLabels = Data.Dates.ToArray();
            YFormatter = value => value.ToString("C");
        }


        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // clear charts etc.
            LineSeriesCollection = new SeriesCollection();
            ColumnSeriesCollection = new SeriesCollection();
            DrawChartsNoContext();
            System.Diagnostics.Debug.WriteLine("CLICKED CLEAAR");
            System.Diagnostics.Debug.WriteLine($"{Data.Dates.Count} {Data.Values.Count}");
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
    }
}
