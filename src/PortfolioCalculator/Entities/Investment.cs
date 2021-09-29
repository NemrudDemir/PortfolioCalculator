using PortfolioCalculator.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortfolioCalculator.Entities
{
    public class Investment
    {
        public string InvestmentId { get; set; }
        public string InvestorId { get; set; }
        public InvestmentType InvestmentType { get; set; }
        public string Isin { get; set; }
        public string City { get; set; }
        public string FondsInvestor { get; set; }
        public IEnumerable<Transaction> Transactions { get; private set; }
        public IEnumerable<Quote> Quotes { get; private set; }

        public Investment(string csvLine)
        {
            var values = csvLine.Split(';');
            InvestorId = values[0];
            InvestmentId = values[1];
            InvestmentType = Enum.Parse<InvestmentType>(values[2]);
            Isin = values[3];
            City = values[4];
            FondsInvestor = values[5];
        }

        public Investment SetTransactions(IEnumerable<Transaction> transactions)
        {
            Transactions = transactions.Where(t => t.InvestmentId == InvestmentId).OrderBy(t => t.Date);
            return this;
        }

        public Investment SetQuotes(IEnumerable<Quote> quotes)
        {
            Quotes = quotes.Where(q => q.Isin == Isin).OrderBy(q => q.Date);
            return this;
        }

        public decimal GetValue() => GetValue(DateTime.MaxValue);

        public decimal GetValue(DateTime date)
        {
            var relevantTransactions = Transactions.Where(t => t.Date <= date);
            switch (InvestmentType)
            {
                case InvestmentType.Fonds:
                    var holdingPercentage = relevantTransactions.Sum(t => t.Value);
                    var fondValue = decimal.Zero; //TODO woher bekomme ich diesen Wert?
                    return holdingPercentage * fondValue;
                case InvestmentType.Stock:
                    var holdingShares = relevantTransactions.Sum(t => t.Value);
                    var quote = Quotes.Where(q => q.Date <= date).LastOrDefault()?.PricePerShare;
                    return (holdingShares * quote) ?? 0;
                case InvestmentType.RealEstate:
                    var estateValue = relevantTransactions.Where(t => t.Type == TransactionType.Estate).Sum(t => t.Value);
                    var buildingValue = relevantTransactions.Where(t => t.Type == TransactionType.Building).Sum(t => t.Value);
                    return estateValue + buildingValue;
                default:
                    throw new NotImplementedException($"{nameof(InvestmentType)} '{InvestmentType}' wurde nicht implementiert");
            }
        }
    }
}
