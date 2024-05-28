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
            LSocket socket = new LSocket(tcpSocket.GetStream());
            ChatClient client = new ChatClient(socket);

            socket.ConsumeStream().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
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

    private static async Task HandleCommandRequest()
    {

    }
}