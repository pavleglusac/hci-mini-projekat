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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataReferencePicker.SelectedValue != null && intervalPicker.SelectedValue != null)
            {
                DateTime? startDate, endDate;

                DataReference dataReference = ParseDataReference(dataReferencePicker.SelectedValue.ToString());
                Interval interval = ParseInterval(intervalPicker.SelectedValue.ToString());

                startDate = startDatePicker.SelectedDate != null ? startDatePicker.SelectedDate.Value.Date : (DateTime?) null;
                endDate = endDatePicker.SelectedDate != null ? endDatePicker.SelectedDate.Value.Date : DateTime.Now;

                Console.Out.WriteLine(dataReference + " " + interval);
                Console.Out.WriteLine(startDate + " " + endDate);

                // fill charts
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // clear charts etc.
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            // show table
        }



        public enum DataReference
        {
            GDP, TREASURY
        }
        public enum Interval
        {
            DAILY, MONTHLY, YEARLY
        }
        public DataReference ParseDataReference(string text)
        {
            return text.ToLower().Contains("gdp") ? DataReference.GDP : DataReference.TREASURY;
        }
        public Interval ParseInterval(string text)
        {
            if (text.ToLower().Contains("daily"))
                return Interval.DAILY;
            else if (text.ToLower().Contains("monthly"))
                return Interval.MONTHLY;
            else
                return Interval.YEARLY;
        }
    }
}
