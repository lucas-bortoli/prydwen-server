
using System.Net.Sockets;
using System.Text;

class ChatClient(TcpClient socket)
{
    readonly TcpClient Socket = socket;

    public string Nickname { get; private set; } = Guid.NewGuid().ToString();
    public bool IsOpen { get; private set; } = false;
    public bool IsAuthenticated { get; private set; } = false;

    private async Task HandleOpenCommand()
    {
        if (IsOpen)
        {
            await Socket.GetStream().WriteAsync(Encoding.UTF8.GetBytes("OK MAN!\n"));
        }

    }

    public async Task HandleCommand(string commandKind, string commandData)
    {
        Console.WriteLine($"Comando = {commandKind}    dados = {commandData}");

        switch (commandKind)
        {
            case "OPEN":
                break;
            case "LOGIN":
                break;
            case "REGISTER":
                break;
            case "JOIN_TOPIC":
                break;
            case "LEAVE_TOPIC":
                break;
            case "SEND_MESSAGE":
                break;
            default:
                throw new Exception($"Comando desconhecido: {commandKind}");
        }
    }
}
