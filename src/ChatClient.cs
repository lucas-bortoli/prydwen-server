
using System.Net.Sockets;
using System.Text;

class ChatClient
{
    private readonly RpcClient RpcClient;
    public string? Nickname { get; private set; } = null;
    public bool IsAuthenticated { get; private set; } = false;

    public ChatClient(RpcClient rpcClient)
    {
        RpcClient = rpcClient;

        RpcClient.ReqHandler = HandleCommand;
    }

    private async Task<object> HandleCommand(Request<object> request)
    {
        string procedure = request.Action;

        Console.WriteLine($"Procedure chamada: {procedure}");

        if (procedure == "OPEN")
        {
            var reqBody = (Protocol.OpenConnectionInput)request.Content;

            // Ao mudar o nickname, desautenticar cliente
            Nickname = reqBody.Nickname;
            IsAuthenticated = false;

            return new object { };
        }
        else if (procedure == "LOGIN")
        {
            var reqBody = (Protocol.LoginNicknameInput)request.Content;

            if (Nickname is null) throw new ChatNicknameNotSetException();
            if (IsAuthenticated) throw new ChatAlreadyAuthenticatedException();

            // fazer autenticação...

            IsAuthenticated = true;

            return new object { };
        }
        else if (procedure == "REGISTER")
        {
            var reqBody = (Protocol.RegisterNicknameInput)request.Content;

            if (Nickname is null) throw new ChatNicknameNotSetException();
            if (IsAuthenticated) throw new ChatAlreadyAuthenticatedException();

            // fazer registro...

            IsAuthenticated = true;

            return new object { };
        }
        else if (procedure == "JOIN_TOPIC")
        {
            var reqBody = (Protocol.JoinTopicInput)request.Content;

            if (Nickname is null) throw new ChatNicknameNotSetException();
            if (!IsAuthenticated) throw new ChatNotAuthenticatedException();

            return new object { };
        }
        else if (procedure == "LEAVE_TOPIC")
        {
            var reqBody = (Protocol.JoinTopicInput)request.Content;

            if (Nickname is null) throw new ChatNicknameNotSetException();
            if (!IsAuthenticated) throw new ChatNotAuthenticatedException();

            return new object { };
        }
        else if (procedure == "SEND_MESSAGE")
        {
            var reqBody = (Protocol.SendMessageInput)request.Content;

            if (Nickname is null) throw new ChatNicknameNotSetException();
            if (!IsAuthenticated) throw new ChatNotAuthenticatedException();

            return new object { };
        }

        throw new Exception($"Procedure desconhecida: {procedure}");

    }
}
