using System;
using System.Collections.Generic;
using LoanManagement.Data;
using LoanManagement.Models;
using LoanManagement.Services.Customer;
using LoanManagement.Services.Loan;
using LoanManagement.Services.Repayment;
using LoanManagement.Storage;

namespace LoanManagement
{
    static class Program
    {
        static DbClass db = new DbClass();
        static Baza baza = new Baza();
        static MailStub mail = new MailStub();

        private const string MENU_CHOICE_REGISTER = "1";
        private const string MENU_CHOICE_LOGIN = "2";
        private const string MENU_CHOICE_ADD_CUSTOMER = "3";
        private const string MENU_CHOICE_SEARCH_CUSTOMER = "4";
        private const string MENU_CHOICE_SHOW_LOAN = "5";
        private const string MENU_CHOICE_DELETE_CUSTOMER = "6";
        private const string MENU_CHOICE_PERFORMANCE = "7";
        private const string MENU_CHOICE_EXIT = "0";

        static void Main(string[] args)
        {
            baza.Init();

            if (args.Contains("--demo"))
            {
                Demo();
                return;
            }

            CustomerService cs = new CustomerService(db);
            string passwordPrompt = "Enter password: ";
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Loan management ===");
                if (Sesija.JeLogovan())
                {
                    Console.WriteLine($"Logged in as: {Sesija.Username} (token {Sesija.Token})");
                }
                DisplayMenu();
                string izbor = Console.ReadLine();

                switch (izbor)
                {
                    case MENU_CHOICE_REGISTER:
                        HandleRegister(passwordPrompt);
                        break;
                    case MENU_CHOICE_LOGIN:
                        HandleLogin(passwordPrompt);
                        break;
                    case MENU_CHOICE_ADD_CUSTOMER:
                        HandleAddCustomer(cs);
                        break;
                    case MENU_CHOICE_SEARCH_CUSTOMER:
                        HandleSearchCustomer();
                        break;
                    case MENU_CHOICE_SHOW_LOAN:
                        HandleShowLoanDetails();
                        break;
                    case MENU_CHOICE_DELETE_CUSTOMER:
                        HandleDeleteCustomer();
                        break;
                    case MENU_CHOICE_PERFORMANCE:
                        PerformanceScenario();
                        break;
                    case MENU_CHOICE_EXIT:
                    case null:
                        return;
                    default:
                        Console.WriteLine("Unknown choice");
                        break;
                }
            }
        }

        private static void HandleRegister(string passwordPrompt)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write(passwordPrompt);
            string password = Console.ReadLine();
            int result = baza.RegistrujKorisnika(username, password);
            if (result > 0)
            {
                Console.WriteLine("User registered");
                mail.PosaljiDobrodoslicu($"{username}@example.com");
            }
        }

        private static void HandleLogin(string passwordPrompt)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write(passwordPrompt);
            string password = Console.ReadLine();
            bool success = baza.Prijava(username, password);
            if (success)
            {
                Console.WriteLine($"Login ok, session token: {Sesija.Token}");
            }
            else
            {
                Console.WriteLine("Wrong username or password");
            }
        }

        private static void HandleAddCustomer(CustomerService cs)
        {
            Console.Write("Name: ");
            string name = Console.ReadLine();
            Console.Write("Email: ");
            string email = Console.ReadLine();
            if (baza.EmailPostoji(email))
            {
                Console.WriteLine("Customer with this email already exists");
                return;
            }

            try
            {
                int id = cs.AddCustomer(name, email);
                baza.DodajKupca(name, email);
                mail.PosaljiDobrodoslicu(email);
                Console.WriteLine($"Added customer {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void HandleSearchCustomer()
        {
            Console.Write("Name to search: ");
            string name = Console.ReadLine();
            string result = baza.TraziKupce(name);
            Console.WriteLine(result);
        }

        private static void HandleShowLoanDetails()
        {
            if (!RequireLogin())
            {
                return;
            }
            Console.Write("Loan id: ");
            string id = Console.ReadLine();
            Console.WriteLine(baza.DetaljiKredita(id));
        }

        private static void HandleDeleteCustomer()
        {
            if (!RequireLogin())
            {
                return;
            }
            Console.Write("Customer id to delete: ");
            string id = Console.ReadLine();
            int deletedRows = baza.ObrisiKupca(id);
            Console.WriteLine($"Deleted rows: {deletedRows}");
        }

        static void DisplayMenu()
        {
            Console.WriteLine("1. Register user");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Add customer");
            Console.WriteLine("4. Search customer by name");
            Console.WriteLine("5. Show loan details by id");
            Console.WriteLine("6. Delete customer by id");
            Console.WriteLine("7. Performance scenario");
            Console.WriteLine("0. Exit");
            Console.Write("Choice: ");
        }

        private static bool RequireLogin()
        {
            if (!Sesija.JeLogovan())
            {
                Console.WriteLine("Please login first");
                return false;
            }
            return true;
        }

        static void PerformanceScenario()
        {
            DbClass pdb = new DbClass();
            CustomerService cs = new CustomerService(pdb);
            RepaymentService rs = new RepaymentService(pdb);

            int cid = cs.AddCustomer("Perf Customer", "perf@example.com");
            DateTime start = DateTime.Now.AddYears(-20);
            Loan l = new Loan(7001, cid, 100000, 5.5, 30, 567.79, start);
            pdb.AddLoan(l);
            for (int i = 0; i < 3000; i++)
            {
                Payment p = new Payment(i + 1, 7001, 25, start.AddDays(i * 2));
                pdb.AddPayment(p);
            }
            Console.WriteLine($"Payments in memory: {pdb.Payments.Count}");

            DateTime t1 = DateTime.Now;
            double b = 0;
            for (int k = 0; k < 220; k++)
            {
                b = rs.CalculateRemainingBalance(7001);
            }
            DateTime t2 = DateTime.Now;
            TimeSpan razlika = t2 - t1;
            Console.WriteLine($"Remaining balance: {b}");
            Console.WriteLine($"Elapsed ms: {razlika.TotalMilliseconds}");
        }

        static void Demo()
        {
            DbClass ddb = new DbClass();
            CustomerService cs = new CustomerService(ddb);
            LoanService ls = new LoanService(ddb);
            RepaymentService rs = new RepaymentService(ddb);

            int cid = cs.AddCustomer("Demo Customer", "demo@example.com");
            Console.WriteLine(cs.GetCustomerDetails(cid));

            List<double> pr = new List<double>();
            List<double> ir = new List<double>();
            List<int> tr = new List<int>();
            pr.Add(25000);
            ir.Add(6.5);
            tr.Add(5);
            pr.Add(10000);
            ir.Add(8);
            tr.Add(2);
            List<LoanCalculationResult> rez = ls.CalculateMonthlyPayment(pr, ir, tr, 2);
            for (int i = 0; i < rez.Count; i++)
            {
                Console.WriteLine(rez[i]);
                Console.WriteLine($"Total interest: {ls.CalculateTotalInterest(rez[i].Principal, rez[i].MonthlyPayment, rez[i].TermInYears)}");
            }

            int lid = ls.AddLoan(cid, rez, DateTime.Now.AddMonths(-6));
            Console.WriteLine($"Loan id: {lid}");
            rs.ProcessPayment(cid, 500, DateTime.Now.AddMonths(-3));
            Console.WriteLine($"Remaining balance: {rs.CalculateRemainingBalance(lid)}");

            try
            {
                rs.CalculateRemainingBalance(1);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Caught: {ex.Message}");
            }
        }
    }
}
