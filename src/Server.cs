using System.Net;
using System.Net.Sockets;

static class Server
{
    public static List<ChatClient> Clients { get; private set; } = new();

    public static async Task Start(string hostname, int port)
    {
        TcpListener tcpListener = new(IPAddress.Parse(hostname), port);
        tcpListener.Start();

        Console.WriteLine($"Servidor iniciado em {hostname}:{port}");
        Database.Connection.GetMessages("");
        while (true)
        {
            TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleSocket(tcpClient));
        }
    }

    private static async Task HandleSocket(TcpClient tcpSocket)
    {
        Console.WriteLine($"Connection from: [{tcpSocket.Client.RemoteEndPoint}]");

        try
        {
            using StreamReader reader = new StreamReader(tcpSocket.GetStream());
            ChatClient client = new ChatClient(tcpSocket);

            Clients.Add(client);

            // Receber mensagens do cliente, enquanto estiver conectado
            while (true)
            {
                string? line = await reader.ReadLineAsync();

                if (line == null)
                {
                    // The client has disconnected gracefully
                    Console.WriteLine("Client disconnected.");
                    break;
                }

                // Interpret the line from the protocol
                string commandKind = line.Split(' ')[0].ToUpper();
                string commandData = line.Substring(commandKind.Length);

                await client.HandleCommand(commandKind, commandData);
            }
        }
        catch (IOException ex)
        {
            // This can occur if the network stream is interrupted
            Console.WriteLine($"Network error: {ex.Message}");
        }
        catch (ObjectDisposedException ex)
        {
            // This can occur if the TcpClient or its stream is disposed
            Console.WriteLine($"Connection closed: {ex.Message}");
        }
        catch (Exception ex)
        {
            // General exception handling for any other unforeseen errors
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        finally
        {
            // Ensure the TcpClient is properly closed and disposed
            if (tcpSocket.Connected)
            {
                tcpSocket.Close();
            }
            tcpSocket.Dispose();
            Console.WriteLine("Connection closed and resources released.");
        }
    }
}