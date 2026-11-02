using System;
using Microsoft.Data.Sqlite;

namespace LoanManagement.Storage
{
    public class Baza
    {
        private const string CONN = "Data Source=loanmanagement.db;Password=demo-db-pass-1";
        private const string CustomersTable = "Customers";
        private string adminLozinka = "admin-demo-pass-1";

        private SqliteConnection NovaVeza()
        {
            SqliteConnectionStringBuilder b = new SqliteConnectionStringBuilder(CONN);
            string fajl = b.DataSource;
            string sifra = b.Password;
            b.Password = "";
            return new SqliteConnection(b.ConnectionString);
        }

        public void Init()
        {
            SqliteConnection con = NovaVeza();
            con.Open();

            SqliteCommand c1 = new SqliteCommand("CREATE TABLE IF NOT EXISTS Customers (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, Email TEXT NOT NULL)", con);
            c1.ExecuteNonQuery();
            SqliteCommand c2 = new SqliteCommand("CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, Role TEXT NOT NULL)", con);
            c2.ExecuteNonQuery();
            SqliteCommand c3 = new SqliteCommand("CREATE TABLE IF NOT EXISTS Loans (Id INTEGER PRIMARY KEY, CustomerId INTEGER NOT NULL, Principal REAL NOT NULL, InterestRate REAL NOT NULL, TermInYears INTEGER NOT NULL, MonthlyPayment REAL NOT NULL, StartDate TEXT NOT NULL)", con);
            c3.ExecuteNonQuery();

            SqliteCommand cnt = new SqliteCommand("SELECT COUNT(*) FROM Users", con);
            long broj = (long)cnt.ExecuteScalar();
            if (broj == 0)
            {
                Console.WriteLine("Creating database with sample data...");

                string[] imena = { "Amra Hadzic", "Emir Kovac", "Lejla Begic", "Tarik Delic", "Selma Mujic" };
                string[] mailovi = { "amra@example.com", "emir@example.com", "lejla@example.com", "tarik@example.com", "selma@example.com" };
                for (int i = 0; i < 5; i++)
                {
                    SqliteCommand ins = new SqliteCommand("INSERT INTO Customers (Id, Name, Email) VALUES (@id, @n, @e)", con);
                    ins.Parameters.AddWithValue("@id", 1001 + i);
                    ins.Parameters.AddWithValue("@n", imena[i]);
                    ins.Parameters.AddWithValue("@e", mailovi[i]);
                    ins.ExecuteNonQuery();
                }

                string[] korisnici = { "admin", "marko", "ana" };
                string[] sifre = { adminLozinka, "marko123", "ana123" };
                string[] role = { "admin", "user", "user" };
                for (int i = 0; i < 3; i++)
                {
                    SqliteCommand ins = new SqliteCommand("INSERT INTO Users (Username, PasswordHash, Role) VALUES (@u, @h, @r)", con);
                    ins.Parameters.AddWithValue("@u", korisnici[i]);
                    ins.Parameters.AddWithValue("@h", Lozinke.Hesiraj(sifre[i]));
                    ins.Parameters.AddWithValue("@r", role[i]);
                    ins.ExecuteNonQuery();
                }

                double[] iznosi = { 20000, 5000, 150000, 8000 };
                double[] kamate = { 6.5, 9, 4.2, 7.75 };
                int[] godine = { 5, 2, 25, 3 };
                for (int i = 0; i < 4; i++)
                {
                    double mr = kamate[i] / 100 / 12;
                    int n = godine[i] * 12;
                    double rata = iznosi[i] * mr * Math.Pow(1 + mr, n) / (Math.Pow(1 + mr, n) - 1);
                    SqliteCommand ins = new SqliteCommand("INSERT INTO Loans (Id, CustomerId, Principal, InterestRate, TermInYears, MonthlyPayment, StartDate) VALUES (@id, @c, @p, @k, @g, @r, @d)", con);
                    ins.Parameters.AddWithValue("@id", 5001 + i);
                    ins.Parameters.AddWithValue("@c", 1001 + i);
                    ins.Parameters.AddWithValue("@p", iznosi[i]);
                    ins.Parameters.AddWithValue("@k", kamate[i]);
                    ins.Parameters.AddWithValue("@g", godine[i]);
                    ins.Parameters.AddWithValue("@r", rata);
                    ins.Parameters.AddWithValue("@d", "2023-03-01");
                    ins.ExecuteNonQuery();
                }
            }
            con.Close();
        }

        public bool EmailPostoji(string email)
        {
            bool rez = false;
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand($"SELECT COUNT(*) FROM {CustomersTable} WHERE Email = @e", con);
                cmd.Parameters.AddWithValue("@e", email);
                long broj = (long)cmd.ExecuteScalar();
                if (broj > 0)
                {
                    rez = true;
                }
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return rez;
        }

        public void DodajKupca(int id, string name, string email)
        {
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand("INSERT INTO Customers (Id, Name, Email) VALUES (@id, @n, @e)", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public string TraziKupce(string ime)
        {
            string sql = "SELECT Id, Name, Email FROM Customers WHERE Name LIKE @ime";
            string rez = "";
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand(sql, con);
                cmd.Parameters.AddWithValue("@ime", "%" + ime + "%");
                SqliteDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    rez = rez + rd.GetValue(0) + " | " + rd.GetValue(1) + " | " + rd.GetValue(2) + "\n";
                }
                rd.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Query was: " + sql);
                Console.WriteLine(ex);
            }
            return rez;
        }

        public int ObrisiKupca(string id)
        {
            string sql = "DELETE FROM Customers WHERE Id = @id";
            int obrisano = 0;
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", id);
                obrisano = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Query was: " + sql);
                Console.WriteLine(ex);
            }
            return obrisano;
        }

        public string DetaljiKredita(string id)
        {
            string rez = "";
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand("SELECT Id, CustomerId, Principal, InterestRate, TermInYears, MonthlyPayment, StartDate FROM Loans WHERE Id = @id", con);
                cmd.Parameters.AddWithValue("@id", int.Parse(id));
                SqliteDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    rez = "Loan " + rd.GetValue(0) + " | customer " + rd.GetValue(1) + " | principal " + rd.GetValue(2) +
                        " | rate " + rd.GetValue(3) + " | years " + rd.GetValue(4) + " | payment " + rd.GetValue(5) + " | start " + rd.GetValue(6);
                }
                rd.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return rez;
        }

        public int RegistrujKorisnika(string username, string password)
        {
            int rez = 0;
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand("INSERT INTO Users (Username, PasswordHash, Role) VALUES (@u, @h, @r)", con);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@h", Lozinke.Hesiraj(password));
                cmd.Parameters.AddWithValue("@r", "user");
                rez = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return rez;
        }

        public bool Prijava(string username, string password)
        {
            bool uspjelo = false;
            try
            {
                SqliteConnection con = NovaVeza();
                con.Open();
                SqliteCommand cmd = new SqliteCommand("SELECT Id, Role FROM Users WHERE Username = @u AND PasswordHash = @h", con);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@h", Lozinke.Hesiraj(password));
                SqliteDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    Sesija.UserId = rd.GetInt32(0);
                    Sesija.Username = username;
                    Sesija.Role = rd.GetString(1);
                    Sesija.Token = Sesija.NapraviToken();
                    uspjelo = true;
                }
                rd.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return uspjelo;
        }
    }
}

