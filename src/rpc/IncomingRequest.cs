using System.Text.Json;

class Request<T>(string action, int requestId, T content)
{
    public readonly int RequestId = requestId;
    public readonly string Action = action;
    public readonly T Content = content;

    public static Request<T> FromRequestString(string line)
    {
        // Interpretar linha vinda do cliente
        string[] segments = line.Split(' ');
        if (!line.StartsWith("REQ "))
        {
            throw new ArgumentException($"Linha passada não é referente a uma requisição: {line}");
        }

        int requestId = int.Parse(segments[1]);

        // Extrair o identificador do request, removendo os dois pontos
        string action = segments[2].Remove(segments[2].Length - 1);
        // Ler corpo do request a partir dos dois pontos
        string content = line.Substring(line.IndexOf(':') + 2);

        T parsedContent = JsonSerializer.Deserialize<T>(content)!;
        return new Request<T>(action, requestId, parsedContent);
    }
}

class Response<T>(int requestId, bool isSuccessful, T content)
{
    public readonly int RequestId = requestId;
    public bool IsSuccessful = isSuccessful;
    public T Content = content;

    public override string ToString()
    {
        string status = IsSuccessful ? "OK" : "FAIL";
        string payload = JsonSerializer.Serialize(Content);

        return $"RES {RequestId} {status}: {payload}\n";
    }

    public static Response<T> FromResponseString(string line)
    {
        // Interpretar linha vinda do cliente
        string[] segments = line.Split(' ');
        if (!line.StartsWith("RES "))
        {
            throw new ArgumentException($"Linha passada não é referente a uma resposta: {line}");
        }

        int requestId = int.Parse(segments[1]);

        // Extrair o código de status OK/FAIL, removendo os dois pontos
        string statusCode = segments[2].Remove(segments[2].Length - 1);
        // Ler corpo da response a partir dos dois pontos
        string content = line.Substring(line.IndexOf(':') + 2);

        T parsedContent = JsonSerializer.Deserialize<T>(content)!;

        return new Response<T>(requestId, statusCode == "OK", parsedContent);
    }
}