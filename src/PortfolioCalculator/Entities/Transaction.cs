using PortfolioCalculator.Entities.Enums;
using System;
using System.Globalization;

namespace PortfolioCalculator.Entities
{
    public class Transaction
    {
        public string InvestmentId { get; set; }
        public TransactionType Type { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }

        public Transaction(string csvLine)
        {
            var values = csvLine.Split(';');
            InvestmentId = values[0];
            Type = Enum.Parse<TransactionType>(values[1]);
            Date = DateTime.Parse(values[2], CultureInfo.InvariantCulture);
            Value = decimal.Parse(values[3], CultureInfo.InvariantCulture);
        }
    }
}
