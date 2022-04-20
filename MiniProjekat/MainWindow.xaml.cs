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

namespace MiniProjekat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection LineSeriesCollection { get; set; }
        public SeriesCollection ColumnSeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> Formatter { get; set; }

        private List<String> GDPIntervals = new List<String>() { "Quarterly", "Annual" };
        private List<String> TreasuryIntervals = new List<String>() { "Daily", "Weekly", "Monthly"};

        public MainWindow()
        {
            InitializeComponent();

            List<String> dataReferences = new List<String>() { "GDP", "Treasury Yields" };
            dataReferencePicker.ItemsSource = dataReferences;
            dataReferencePicker.SelectedIndex = 0;

            intervalPicker.ItemsSource = GDPIntervals;
            intervalPicker.SelectedIndex = 0;
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

        private void DrawColumnChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");

            ColumnSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50, 37, 28, 15 },
                    Stroke = brush,
                    Fill = brush
                }
            };
          
            //ColumnSeriesCollection[1].Values.Add(48d);

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            Formatter = value => value.ToString("N");
        }

        private void DrawLineChart()
        {
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5a7bfb");

            LineSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15,
                    Stroke = brush
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            /*
            LineSeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            }); 
            */

            // LineSeriesCollection[3].Values.Add(5d);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // clear charts etc.
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
