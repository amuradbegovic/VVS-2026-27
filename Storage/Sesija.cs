using System;
using System.Text;

namespace LoanManagement.Storage
{
    public static class Sesija
    {
        public static int UserId { get; set; } = 0;
        public static string Username { get; set; } = "";
        public static string Role { get; set; } = "";
        public static string Token { get; set; } = "";

        public static string NapraviToken()
        {
            Random r = new Random();
            string slova = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                bld.Append(slova[r.Next(0, slova.Length)]);
            }
            string t = bld.ToString();
            return t;
        }

        public static bool JeLogovan()
        {
            if (Token == "")
            {
                return false;
            }
            return true;
        }

        public static void Odjava()
        {
            UserId = 0;
            Username = "";
            Role = "";
            Token = "";
        }
    }
}
