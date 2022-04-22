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
using System.Windows.Shapes;

namespace MiniProjekat
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : Window
    {
        public static string minValue;
        public static string maxValue;
        public TableView(DataHandler.Data data)
        {
            InitializeComponent();
            minValue = data.Values.Min().ToString();
            maxValue = data.Values.Max().ToString();
            FillTable(data);
        }

        private void FillTable(DataHandler.Data data)
        {
            this.Clear();
            Console.Out.WriteLine(minValue + " " + maxValue);
            for (int i = 0; i < data.Dates.Count(); i++)
            {
                Console.Out.WriteLine(data.Dates[i] + " " + data.Values[i].ToString());
                table.Items.Add(new DataItem
                {
                    Date = data.Dates[i],
                    Value = data.Values[i].ToString()
                });
            }
        }

        public void Update(DataHandler.Data data)
        {
            minValue = data.Values.Min().ToString();
            maxValue = data.Values.Max().ToString();
            FillTable(data);
        }

        public void Clear()
        {
            table.Items.Clear();
        }
        

    }
    public class DataItem
    {
        public string Date { get; set; }
        public string Value { get; set; }
        public bool IsMaxValue { get { return Value == TableView.maxValue; } }
        public bool IsMinValue { get { return Value == TableView.minValue; } }

    }
}
