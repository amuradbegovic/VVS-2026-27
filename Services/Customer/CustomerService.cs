using System;
using System.Collections.Generic;
using LoanManagement.Data;
using Kupac = LoanManagement.Models.Customer;

namespace LoanManagement.Services.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly DbClass _db;

        public CustomerService(DbClass db)
        {
            _db = db;
        }

        public int AddCustomer(string name, string email)
        {
            if (name == null)
            {
                throw new ArgumentException("Name is null");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("Name is empty");
            }
            if (email.IndexOf("@") < 0)
            {
                throw new ArgumentException("Email is not valid");
            }

            Random r = new Random();
            int x1 = 0;
            bool flg = false;
            while (!flg)
            {
                x1 = r.Next(1000, 9999);
                flg = true;
                for (int i = 0; i < _db.Customers.Count; i++)
                {
                    if (_db.Customers[i].Id == x1)
                    {
                        flg = false;
                    }
                }
            }

            Kupac k = new Kupac(x1, name, email);
            _db.AddCustomer(k);
            Console.WriteLine("Customer saved with id " + x1);
            return x1;
        }

        public string GetCustomerDetails(int customerId)
        {
            Kupac k = _db.GetCustomer(customerId);
            if (k == null)
            {
                throw new InvalidOperationException("Customer " + customerId + " does not exist");
            }
            string s = "Id: " + k.Id + ", Name: " + k.Name + ", Email: " + k.Email;
            return s;
        }
    }
}
