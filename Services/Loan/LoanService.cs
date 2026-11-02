using System;
using System.Collections.Generic;
using LoanManagement.Data;
using LoanManagement.Models;
using LoanManagement.Services.Repayment;
using Kupac = LoanManagement.Models.Customer;
using Kredit = LoanManagement.Models.Loan;

namespace LoanManagement.Services.Loan
{
    public class LoanService : ILoanService
    {
        private readonly DbClass _db;

        public LoanService(DbClass db)
        {
            _db = db;
        }

        public List<LoanCalculationResult> CalculateMonthlyPayment(List<double> principalAmounts, List<double> interestRates, List<int> loanTerms, int numberOfLoans)
        {
            if (principalAmounts.Count != numberOfLoans)
            {
                throw new ArgumentException("principalAmounts has wrong size");
            }
            if (interestRates.Count != numberOfLoans)
            {
                throw new ArgumentException("interestRates has wrong size");
            }
            if (loanTerms.Count != numberOfLoans)
            {
                throw new ArgumentException("loanTerms has wrong size");
            }

            List<LoanCalculationResult> rez = new List<LoanCalculationResult>();
            for (int i = 0; i < numberOfLoans; i++)
            {
                double p = principalAmounts[i];
                double k = interestRates[i];
                int t = loanTerms[i];

                if (p <= 0)
                {
                    throw new InvalidOperationException("Principal must be positive");
                }
                if (k <= 0)
                {
                    throw new InvalidOperationException("Interest rate must be positive");
                }
                if (t <= 0)
                {
                    throw new InvalidOperationException("Term must be positive");
                }

                double mr = k / 100 / 12;
                int n = t * 12;
                double rata = p * mr * Math.Pow(1 + mr, n) / (Math.Pow(1 + mr, n) - 1);

                LoanCalculationResult x = new LoanCalculationResult();
                x.Principal = p;
                x.InterestRate = k;
                x.TermInYears = t;
                x.MonthlyPayment = rata;
                x.NumberOfMonths = n;
                x.MonthlyInterestRate = mr;
                rez.Add(x);
            }
            return rez;
        }

        public double CalculateTotalInterest(double principal, double monthlyPayment, int termInYears)
        {
            if (principal <= 0)
            {
                throw new InvalidOperationException("Principal must be positive");
            }
            if (termInYears <= 0)
            {
                throw new InvalidOperationException("Term must be positive");
            }
            // total interest is payment minus principal
            double ukupno = monthlyPayment * termInYears * 12;
            double kamata = ukupno - principal;
            return kamata;
        }

        public int AddLoan(int customerId, List<LoanCalculationResult> loanResults, DateTime startDate)
        {
            if (loanResults == null)
            {
                throw new ArgumentException("loanResults is null");
            }
            Kupac cc = _db.GetCustomer(customerId);
            if (cc == null)
            {
                throw new InvalidOperationException("Customer " + customerId + " does not exist");
            }

            int lastLoanId = -1;

            for (int i = 0; i < loanResults.Count; i++)
            {
                double[] podaci = new double[5];
                podaci[0] = loanResults[i].Principal;
                podaci[1] = loanResults[i].InterestRate;
                podaci[2] = loanResults[i].TermInYears;
                podaci[3] = loanResults[i].MonthlyPayment;
                podaci[4] = 0;

                if (loanResults[i].MonthlyPayment <= 0)
                {
                    loanResults[i].MonthlyPayment = loanResults[i].Principal * (loanResults[i].InterestRate / 100 / 12) * Math.Pow(1 + loanResults[i].InterestRate / 100 / 12, loanResults[i].TermInYears * 12) / (Math.Pow(1 + loanResults[i].InterestRate / 100 / 12, loanResults[i].TermInYears * 12) - 1);
                    podaci[3] = loanResults[i].MonthlyPayment;
                    podaci[4] = 1;
                }
                if (podaci[4] == 1)
                {
                    Console.WriteLine("Payment was recalculated: " + podaci[3]);
                }

                // Generate next ID using AUTOINCREMENT pattern (max existing + 1)
                int x1 = 1;
                if (_db.Loans.Count > 0)
                {
                    x1 = _db.Loans[_db.Loans.Count - 1].Id + 1;
                }

                Kredit l = new Kredit(x1, customerId, podaci[0], podaci[1], (int)podaci[2], podaci[3], startDate);
                _db.AddLoan(l);
                Console.WriteLine("Loan saved with id " + x1);

                RepaymentService rs = new RepaymentService(_db);
                rs.ProcessPayment(customerId, podaci[3], startDate);
                lastLoanId = x1;
            }

            return lastLoanId;
        }
    }
}
