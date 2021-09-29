using PortfolioCalculator.Entities;
using PortfolioCalculator.Entities.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PortfolioCalculator
{
    class Program
    {
        private const string _resourceFolder = "Resources";
        private static string _dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string _investmentsFile = Path.Join(_dllPath, _resourceFolder, "Investments.csv");
        private static string _quotesFile = Path.Join(_dllPath, _resourceFolder, "Quotes.csv");
        private static string _transactionsFile = Path.Join(_dllPath, _resourceFolder, "Transactions.csv");
        private static IEnumerable<Investment> _investments;
        private static Dictionary<string, IList<Transaction>> _transactions; //key = investmentId
        private static Dictionary<string, IList<Quote>> _quotes; //key = ISIN
        private static Dictionary<string, IList<Investment>> _fondInvestments; //key = FondsInvestor

        private static ILogger logger;

        static void Main(string[] args)
        {
            logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().CreateLogger();

            try
            {
                var line = Console.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var input = line.Split(";");
                    var date = DateTime.Parse(input[0]);
                    var investorId = input[1];

                    /*code*/
                    var result = Calculate(date, investorId);
                    Console.WriteLine($"{result:N2} Euro");
                    line = Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Unerwarteter Fehler!");
                throw;
            }
        }

        static decimal Calculate(DateTime date, string investorId)
        {
            var investments = GetInvestments().Where(i => i.InvestorId == investorId).ToArray();
            var total = 0m;
            for (var i = 0; i < investments.Length; i++)
            {
                logger.Information($"Berechne Investment {i + 1} von {investments.Length}...");
                total += GetValue(investments[i], date);
            }
            return total;
        }

        static decimal GetValue(Investment investment, DateTime date)
        {
            var relevantTransactions = GetTransactions()[investment.InvestmentId].Where(t => t.Date <= date);
            switch (investment.InvestmentType)
            {
                case InvestmentType.Fonds:
                    var dependedFondInvestments = GetFondsInvestments()[investment.FondsInvestor];
                    var totalPercentage = 0m;
                    foreach (var dependedFondInvestment in dependedFondInvestments)
                    {
                        var dfiRelevantTransactions = GetTransactions()[dependedFondInvestment.InvestmentId].Where(t => t.Date <= date);
                        totalPercentage += dfiRelevantTransactions.Sum(t => t.Value);
                    }
                    var holdingPercentage = relevantTransactions.Sum(t => t.Value);
                    var fondValue = decimal.Zero; //TODO woher bekomme ich diesen Wert?
                    return holdingPercentage * fondValue;
                case InvestmentType.Stock:
                    var holdingShares = relevantTransactions.Sum(t => t.Value);
                    var quote = GetQuotes()[investment.Isin].Where(q => q.Date <= date).LastOrDefault()?.PricePerShare;
                    return (holdingShares * quote) ?? 0;
                case InvestmentType.RealEstate:
                    var estateValue = relevantTransactions.Where(t => t.Type == TransactionType.Estate).Sum(t => t.Value);
                    var buildingValue = relevantTransactions.Where(t => t.Type == TransactionType.Building).Sum(t => t.Value);
                    return estateValue + buildingValue;
                default:
                    throw new NotImplementedException($"{nameof(InvestmentType)} '{investment.InvestmentType}' wurde nicht implementiert");
            }
        }

        static IEnumerable<Investment> GetInvestments()
        {
            if(_investments == null)
            {
                _investments = File.ReadAllLines(_investmentsFile)
                                    .Skip(1)
                                    .Select(v => new Investment(v))
                                    .ToArray();
            }
            return _investments;
        }

        static Dictionary<string, IList<Investment>> GetFondsInvestments()
        {
            if(_fondInvestments == null)
            {
                _fondInvestments = new Dictionary<string, IList<Investment>>();
                foreach(var investment in GetInvestments())
                {
                    if (investment.InvestmentType != InvestmentType.Fonds)
                        continue;
                    if (!_fondInvestments.ContainsKey(investment.FondsInvestor))
                        _fondInvestments.Add(investment.FondsInvestor, new List<Investment>());
                    _fondInvestments[investment.FondsInvestor].Add(investment);
                }
            }
            return _fondInvestments;
        }

        static Dictionary<string, IList<Transaction>> GetTransactions()
        {
            if(_transactions == null)
            {
                var transactions = File.ReadAllLines(_transactionsFile)
                                    .Skip(1)
                                    .Select(v => new Transaction(v))
                                    .OrderBy(t => t.Date)
                                    .ToArray();
                _transactions = new Dictionary<string, IList<Transaction>>();
                foreach(var transaction in transactions)
                {
                    if (!_transactions.ContainsKey(transaction.InvestmentId))
                        _transactions.Add(transaction.InvestmentId, new List<Transaction>());
                    _transactions[transaction.InvestmentId].Add(transaction);
                }
            }
            return _transactions;
        }

        static Dictionary<string, IList<Quote>> GetQuotes()
        {
            if (_quotes == null)
            {
                var quotes = File.ReadAllLines(_quotesFile)
                            .Skip(1)
                            .Select(v => new Quote(v))
                            .OrderBy(q => q.Date)
                            .ToArray();
                _quotes = new Dictionary<string, IList<Quote>>();
                foreach(var quote in quotes)
                {
                    if (!_quotes.ContainsKey(quote.Isin))
                        _quotes.Add(quote.Isin, new List<Quote>());
                    _quotes[quote.Isin].Add(quote);
                }
            }
            return _quotes;
        }
    }
}