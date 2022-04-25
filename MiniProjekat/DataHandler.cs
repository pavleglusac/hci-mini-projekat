using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace MiniProjekat
{

    public enum GDP_INTERVAL { QUARTERLY, ANNUAL }
    public enum TREASURY_INTERVAL { DAILY, WEEKLY, MONTHLY }

    public enum TREASURY_MATURITY { M3, Y2, Y5, Y7, Y10, Y30 }

    public class DataHandler
    {
        public string Units { get; set; }

        private Dictionary<TREASURY_MATURITY, String> dict;
        public DataHandler()
        {
            dict = new Dictionary<TREASURY_MATURITY, String>();
            dict[TREASURY_MATURITY.M3] = "3month";
            dict[TREASURY_MATURITY.Y2] = "2year";
            dict[TREASURY_MATURITY.Y5] = "5year";
            dict[TREASURY_MATURITY.Y7] = "7year";
            dict[TREASURY_MATURITY.Y10] = "10year";
            dict[TREASURY_MATURITY.Y30] = "30year";
        }


        public Data getGDP(GDP_INTERVAL interval)
        {
            string intervalValue = Enum.GetName(typeof(GDP_INTERVAL), interval).ToLower();
            string QUERY_URL = $"https://www.alphavantage.co/query?function=REAL_GDP&interval={intervalValue}&apikey=47X2H0EVB6P96YFT";
            Uri queryUri = new Uri(QUERY_URL);
            using (WebClient client = new WebClient())
            {

                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic json_data = js.Deserialize(client.DownloadString(queryUri), typeof(object));

                Data data = new Data();
                Double value;
                if (!json_data.ContainsKey("data"))
                {
                    return null;
                }
                Units = json_data["unit"];
                foreach (var dateValuePair in json_data["data"])
                {
                    if (!Double.TryParse(dateValuePair["value"], out value))
                    {
                        continue;
                    }
                    data.Dates.Add(dateValuePair["date"]);
                    data.Values.Add(value);
                }
                return data;
            }
        }

        public Data getTreasuryYield(TREASURY_INTERVAL interval, TREASURY_MATURITY maturity)
        {
            string intervalValue = Enum.GetName(typeof(TREASURY_INTERVAL), interval).ToLower();
            string maturityValue = dict[maturity].ToLower();
            string QUERY_URL = $"https://www.alphavantage.co/query?function=TREASURY_YIELD&interval={intervalValue}&maturity={maturityValue}&apikey=47X2H0EVB6P96YFT";
            Uri queryUri = new Uri(QUERY_URL);
            using (WebClient client = new WebClient())
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic json_data = js.Deserialize(client.DownloadString(queryUri), typeof(object));
                Double value;
                Data data = new Data();
                if (!json_data.ContainsKey("data"))
                {
                    return null;
                }
                Units = json_data["unit"];
                foreach (var dateValuePair in json_data["data"])
                {
                    if (!Double.TryParse(dateValuePair["value"], out value))
                    {
                        continue;
                    }
                    data.Dates.Add(dateValuePair["date"]);
                    data.Values.Add(value);
                }
                return data;
            }
        }
        public class Data
        {
            public List<string> Dates { get; set; }
            public List<Double> Values { get; set; }

            public Data()
            {
                Dates = new List<string>();
                Values = new List<Double>();
            }
        }

    }
}
