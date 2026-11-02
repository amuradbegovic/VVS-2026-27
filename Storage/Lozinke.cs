using System;
using System.Security.Cryptography;
using System.Text;

namespace LoanManagement.Storage
{
    public static class Lozinke
    {
        public static string Hesiraj(string pass)
        {
            var sha512 = SHA512.Create();
            byte[] bajti = Encoding.UTF8.GetBytes(pass);
            byte[] h = sha512.ComputeHash(bajti);
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < h.Length; i++)
            {
                bld.Append(h[i].ToString("x2"));
            }
            return bld.ToString();
        }
    }
}
