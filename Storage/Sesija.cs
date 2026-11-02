using System;
using System.Security.Cryptography;
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
            byte[] tokenData = new byte[32];  // 256 bita entropije
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenData);
            }
            return Convert.ToBase64String(tokenData);
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
