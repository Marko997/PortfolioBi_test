using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace PortfolioBi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string symbol = "GLW";
            DateTime startDate = new DateTime(2015, 1, 1);
            DateTime endDate = new DateTime(2015, 6, 30);
            DateTime endDateForDividend = new DateTime(2015, 2, 13);

            decimal startPrice;
            decimal sellPrice;
            decimal dividend;

            StockData stock = new StockData();

            var stockAwaiter = stock.GetStockData(symbol, startDate, endDate);
            var dividendAwaiter = stock.GetDividendData(symbol, startDate, endDateForDividend);

            if (stockAwaiter.Result == 1)
            {
                Console.WriteLine("Minimum average: " + Math.Round(stock.avgMin.Average(), 2) + "$");
                Console.WriteLine("Maximum average: " + Math.Round(stock.avgMax.Average(), 2) + "$");
                Console.WriteLine("Close average: " + Math.Round(stock.avgClose.Average(), 2) + "$");
                
            }
            decimal deviation = (decimal)Math.Round(stock.GetDeviation(stock.avgClose),2);
            Console.WriteLine("First most significant positive spike: " + stock.GetSpikes(stock.avgClose,deviation).Max()+"$");

            startPrice = stock.startPrice;
            sellPrice = stock.GetSpikes(stock.avgClose, 0).Max();

            if (dividendAwaiter.Result == 1)
            {
                dividend = stock.dividend * 1000;

                var roiResult = Math.Round(((sellPrice - startPrice) * 1000 + dividend) / (startPrice * 1000) * 100, 2);

                Console.WriteLine("ROI: " + roiResult + "%");
            }

            Console.ReadLine();
        }
    }

    class StockData
    {
        public List<decimal> avgMin = new List<decimal>();
        public List<decimal> avgMax = new List<decimal>();
        public List<decimal> avgClose = new List<decimal>(); 

        public decimal dividend;
        public decimal startPrice;


        public async Task<int> GetDividendData(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                var dividends = await Yahoo.GetDividendsAsync(symbol, startDate, endDate);
                var security = await Yahoo.Symbols(symbol).Fields(Field.LongName).QueryAsync(); //return IreadOnly list
                var ticker = security[symbol];
                var companyName = ticker[Field.LongName];

                for (int i = 0; i < dividends.Count; i++)
                {
                    Console.WriteLine(companyName + " Dividend: " +
                                      dividends.ElementAt(i).DateTime.Month +
                                      "/" + dividends.ElementAt(i).DateTime.Day +
                                      "/" + dividends.ElementAt(i).DateTime.Year +
                                      ": dividend: " + Math.Round(dividends.ElementAt(i).Dividend, 2)
                                      );
                    dividend = Math.Round(dividends.ElementAt(i).Dividend, 2);
                    Console.WriteLine(dividend + "sadsadsadsadasdasd");
                }
            }
            catch (Exception)
            {

                Console.WriteLine("Failed to get the symbol!");
            }
            return 1;
        }

        public double GetDeviation(List<decimal> values)
        {
            decimal avgPrice = values.Average();
            decimal variance = 0m;
            double deviation;

            for (int i = 0; i < values.Count; i++) //Calculating variance
            {
                variance += (values[i] - avgPrice)* (values[i] - avgPrice);
            }

            variance = variance / values.Count;

            deviation = Math.Sqrt((double)variance);

            return deviation;
        }

        public List<decimal> GetSpikes(List<decimal> values, decimal deviation)
        {
            List<decimal> spikes = new List<decimal>();
            decimal average = 0m;
            foreach (decimal val in values)
            {
                average += val;
            }
            average = average / values.Count;

            foreach (decimal val in values)
            {
                if (val > average + deviation)
                {
                    spikes.Add(Math.Round(val, 2));
                }

            }

            return spikes;

        }


        public async Task<int> GetStockData(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                var history = await Yahoo.GetHistoricalAsync(symbol, startDate, endDate);
                var security = await Yahoo.Symbols(symbol).Fields(Field.LongName).QueryAsync(); //return IreadOnly list
                var ticker = security[symbol];
                var companyName = (string)ticker[Field.LongName];

                for (int i = 0; i < history.Count; i++)
                {
                    Console.WriteLine(companyName + " Closing price on: " +
                                      history.ElementAt(i).DateTime.Month +
                                      "/" + history.ElementAt(i).DateTime.Day +
                                      "/" + history.ElementAt(i).DateTime.Year +
                                      ": $" + Math.Round(history.ElementAt(i).Close, 2) +
                                      " min price: " + Math.Round(history.ElementAt(i).Low, 2) +
                                      " max price: " + Math.Round(history.ElementAt(i).High, 2));

                    avgMin.Add(Math.Round(history.ElementAt(i).Low, 2));
                    avgMax.Add(Math.Round(history.ElementAt(i).High, 2));
                    avgClose.Add(Math.Round(history.ElementAt(i).Close, 2));

                    startPrice = avgClose.ElementAt(0);
                }

            }
            catch
            {

                Console.WriteLine("Failed to get the symbol!");
            }


            return 1;
        }
    }
}


