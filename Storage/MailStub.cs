using System;

namespace LoanManagement.Storage
{
    public class MailStub
    {
        private readonly string apiKljuc = Environment.GetEnvironmentVariable("MAIL_API_KEY")
                      ?? throw new InvalidOperationException("Mail API key not configured");
        private readonly string url = Environment.GetEnvironmentVariable("MAIL_SERVICE_URL")
                      ?? throw new InvalidOperationException("Mail service URL not configured");

        public string PosaljiDobrodoslicu(string email)
        {
            string header = "Authorization: Bearer " + apiKljuc;
            string tijelo = "to=" + email + "&text=Welcome";
            string poruka = "POST " + url + " [" + header + "] " + tijelo;
            // nothing is sent, only prepared
            Console.WriteLine("Welcome mail prepared for " + email);
            return poruka;
        }
    }
}
