using System;
using System.Collections.Generic;
using LoanManagement.Data;
using LoanManagement.Models;
using Kredit = LoanManagement.Models.Loan;

namespace LoanManagement.Services.Repayment
{
    public class RepaymentService : IRepaymentService
    {
        private readonly DbClass _db;

        public RepaymentService(DbClass db)
        {
            _db = db;
        }

        public double CalculateRemainingBalance(int loanId)
        {
            Kredit l = _db.GetLoan(loanId);
            if (l == null)
            {
                throw new InvalidOperationException("Loan " + loanId + " does not exist");
            }

            double bal = l.Principal;
            double ukupnoUplaceno = 0;
            int cnt = 0;
            bool flg = false;
            string log = "";

            // total of all payments
            for (int i = 0; i < _db.Payments.Count; i++)
            {
                if (_db.Payments[i].LoanId == loanId)
                {
                    ukupnoUplaceno = ukupnoUplaceno + _db.Payments[i].Amount;
                    cnt = cnt + 1;
                }
            }

            // interest on the principal
            int mj = (DateTime.Now.Year - l.StartDate.Year) * 12 + (DateTime.Now.Month - l.StartDate.Month);
            for (int m = 0; m < mj; m++)
            {
                double mr = l.InterestRate / 100 / 12;
                bal = bal * Math.Pow(1 + mr, 1);
            }

            // subtract every payment together with the interest it saved
            for (int i = 0; i < _db.Payments.Count; i++)
            {
                if (_db.Payments[i].LoanId == loanId && _db.Payments[i].PaymentDate >= l.StartDate)
                {
                    double pv = _db.Payments[i].Amount;
                    int mp = (DateTime.Now.Year - _db.Payments[i].PaymentDate.Year) * 12 + (DateTime.Now.Month - _db.Payments[i].PaymentDate.Month);
                    for (int m = 0; m < mp; m++)
                    {
                        double mr = l.InterestRate / 100 / 12;
                        pv = pv * Math.Pow(1 + mr, 1);
                    }
                    bal = bal - pv;
                    log = log + "p" + i + "=" + pv + ";";
                }
            }

            if (bal < 0)
            {
                bal = 0;
                flg = true;
            }
            if (DateTime.Now > l.StartDate.AddYears(l.TermInYears) && (flg == false || cnt == 0))
            {
                Console.WriteLine("Loan " + loanId + " is past its term and still owes " + bal);
            }
            return bal;
        }

        public void ProcessPayment(int customerId, double amount, DateTime paymentDate)
        {
            // payment goes to the newest loan of the customer
            Kredit nadjen = null;
            //Customer ccc = _db.GetCustomer(customerId);
            for (int i = 0; i < _db.Loans.Count; i++)
            {
                if (_db.Loans[i].CustomerId == customerId)
                {
                    nadjen = _db.Loans[i];
                    i = _db.Loans.Count;
                }
            }
            if (nadjen == null)
            {
                throw new InvalidOperationException("Customer " + customerId + " has no loan");
            }

            // Generate next ID using AUTOINCREMENT pattern (max existing + 1)
            int x1 = 1;
            if (_db.Payments.Count > 0)
            {
                x1 = _db.Payments[_db.Payments.Count - 1].Id + 1;
            }

            Payment pay = new Payment(x1, nadjen.Id, amount, paymentDate);
            _db.AddPayment(pay);
            Console.WriteLine("Payment " + x1 + " booked on loan " + nadjen.Id);
        }
    }
}
