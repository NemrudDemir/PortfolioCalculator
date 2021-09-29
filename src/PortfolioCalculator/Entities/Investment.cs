using PortfolioCalculator.Entities.Enums;
using System;

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
    }
}
