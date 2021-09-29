using PortfolioCalculator.Entities;
using PortfolioCalculator.Entities.Enums;
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
        private static IEnumerable<Transaction> _transactions;
        private static IEnumerable<Quote> _quotes;

        static void Main(string[] args)
        {
            var line = Console.ReadLine();
            while(!string.IsNullOrEmpty(line))
            {
                var input = line.Split(";");
                var date = DateTime.Parse(input[0]);
                var investorId = input[1];

                /*code*/
                var result = Calculate(date, investorId);
                Console.WriteLine(result);
                line = Console.ReadLine();
            }
        }

        static decimal Calculate(DateTime date, string investorId)
        {
            var investments = GetInvestments(investorId);
            return investments.Sum(i => i.GetValue(date));
        }

        //AUSLAGERN!
        static IEnumerable<Investment> GetInvestments(string investorId)
        {
            var transactions = File.ReadAllLines(_investmentsFile)
                .Skip(1)
                .Select(v => new Investment(v))
                .Where(i => i.InvestorId == investorId);
            return
                transactions
                .Select(v =>
                    v.SetTransactions(GetTransactions())
                     .SetQuotes(GetQuotes())
                );
        }

        static IEnumerable<Transaction> GetTransactions()
        {
            if(_transactions == null)
            {
                _transactions = File.ReadAllLines(_transactionsFile)
                                    .Skip(1)
                                    .Select(v => new Transaction(v))
                                    .OrderBy(t => t.Date)
                                    .ToArray();
            }
            return _transactions;
        }

        static IEnumerable<Quote> GetQuotes()
        {
            if (_quotes == null)
            {
                _quotes = File.ReadAllLines(_quotesFile)
                            .Skip(1)
                            .Select(v => new Quote(v))
                            .OrderBy(q => q.Date)
                            .ToArray();
            }
            return _quotes;
        }
    }
}