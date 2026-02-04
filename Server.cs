using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
class Server
{
    private Database db = new Database();

    public void Start(int port)
    {
        db.InitDb("data");

        using var listener = new TcpListener(IPAddress.Any, port);
        try
        {
            listener.Start();
            Console.WriteLine($"Server pid:{Process.GetCurrentProcess().Id} running on port {port}...");

            while (true)
            {
                try
                {
                    if (listener.Pending())
                    {
                        var client = listener.AcceptTcpClient();
                        ThreadPool.QueueUserWorkItem(HandleClient, client);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Listener error: " + ex.Message);
                }

                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Server error: " + ex.Message);
        }
        finally
        {
            try { listener.Stop(); } catch {}
            try { db.Close(); } catch {}
        }
    }

    private void HandleClient(object? obj)
    {
        using var client = (TcpClient)obj!;
        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[1024];

            const string artpath = "./art";
            if (File.Exists(artpath))
            {
                string content = File.ReadAllText(artpath);
                var artBytes = Encoding.UTF8.GetBytes(content + "\n");
                stream.Write(artBytes, 0, artBytes.Length);
            }

            var prompt1 = Encoding.UTF8.GetBytes("Enter your username: ");
            stream.Write(prompt1, 0, prompt1.Length);
            int read1 = 0;
            try { read1 = stream.Read(buffer, 0, buffer.Length); } catch {}
            var username = Encoding.UTF8.GetString(buffer, 0, read1).Trim();

            var prompt2 = Encoding.UTF8.GetBytes("Enter your message: ");
            stream.Write(prompt2, 0, prompt2.Length);
            int read2 = 0;
            try { read2 = stream.Read(buffer, 0, buffer.Length); } catch {}
            var message = Encoding.UTF8.GetString(buffer, 0, read2).Trim();

            try { db.InsertMessage(username, message); } catch (Exception ex) { Console.Error.WriteLine("DB error: " + ex.Message); }

            var thank = Encoding.UTF8.GetBytes("Message saved! Bye!\n");
            stream.Write(thank, 0, thank.Length);
            stream.Flush();
            try { client.Client.Shutdown(SocketShutdown.Both); } catch {}
            try { client.Close(); } catch {}
        }
        catch (Exception ex)
        {
            try { client.Close(); } catch {}
            Console.Error.WriteLine("Client handling error: " + ex.Message);
        }
    }
}
