using System;
using System.Globalization;

namespace PortfolioCalculator.Entities
{
    public class Quote
    {
        public string Isin { get; set; }
        public DateTime Date { get; set; }
        public decimal PricePerShare { get; set; }

        public Quote(string csvLine)
        {
            var values = csvLine.Split(';');
            Isin = values[0];
            Date = DateTime.Parse(values[1], CultureInfo.InvariantCulture);
            PricePerShare = decimal.Parse(values[2], CultureInfo.InvariantCulture);
        }
    }
}
