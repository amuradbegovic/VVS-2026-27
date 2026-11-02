using LoanManagement.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                throw new ArgumentException("Email address is not valid");
            }

            // Let the database handle ID generation via AUTOINCREMENT
            Kupac k = new Kupac(0, name, email);  // ID will be 0 for now, will be set by DB
            _db.AddCustomer(k);

            // Note: For in-memory database, we need to get the ID differently
            // In a real scenario with persistent DB, DodajKupca returns the ID
            int customerId = _db.Customers[_db.Customers.Count - 1].Id;
            Console.WriteLine("Customer saved with id " + customerId);
            return customerId;
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
