using System;
using System.Text;
using System.Threading;
using Microsoft.Data.Sqlite;

class Database
{
    private SqliteConnection? connection;

    public bool InitDb(string name)
    {
        try
        {
            connection = new SqliteConnection("Data Source=" + name + ".db");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Chat(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Message TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Init DB error: " + ex.Message);
            try { connection?.Close(); } catch {}
            connection = null;
            return false;
        }
    }

    public void InsertMessage(string username, string message)
    {
        if (connection == null) return;
        try
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Chat (Username, Message) VALUES (@user, @msg)";
            cmd.Parameters.AddWithValue("@user", username);
            cmd.Parameters.AddWithValue("@msg", message);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Message from {username} saved!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Insert message error: " + ex.Message);
        }
    }

    public void Close()
    {
        try
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Close DB error: " + ex.Message);
        }
    }
}

class Program
{
    public static void Main()
    {
        var server = new Server();
        Console.CancelKeyPress += (s, e) => { Environment.Exit(0); };
        try
        {
            server.Start(6767);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Fatal: " + ex.Message);
        }
    }
}
