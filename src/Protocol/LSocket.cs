using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Protocol;

// Referência https://jonathancrozier.com/blog/implementing-the-request-response-pattern-using-c-sharp-with-json-rpc-and-websockets

class LSocket(NetworkStream stream)
{
    public delegate Task<Response> OnRequestReceived(int id, string kind, string? args);

    public readonly NetworkStream stream = stream;
    private int requestCounter = 0;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<Response>> _responses = new();

    // Envia um request para o outro par.
    public async Task<Response> SendRequestToOtherSide(string kind, object? payload)
    {
        int requestId = ++requestCounter;
        string imperative = kind.Trim().ToUpper();
        string serializedPayload = JsonSerializer.Serialize(payload);
        string requestLine = $"REQ {requestId} {imperative}: {serializedPayload}\n";

        // Registrar que queremos receber uma resposta relacionada ao requestId eventualmente
        TaskCompletionSource<Response> tcs = new TaskCompletionSource<Response>();
        _responses.TryAdd(requestId, tcs);

        try
        {
            // Enviar requisição pelo socket
            await stream.WriteAsync(Encoding.UTF8.GetBytes(requestLine));

            // Esperar nesse ponto até que a resposta seja recebida na outra função
            Task.WaitAll([tcs.Task]);

            if (tcs.Task.IsCompleted)
            {
                // Obtemos nossa resposta em task.Result, devemos interpretá-la
                return tcs.Task.Result;
            }
            else
            {
                // Timeout
                // TODO: Lidar com erros de forma melhor
                throw new TimeoutException("Timeout no request");
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine("Erro {0}", exception.ToString());
            throw;
        }
        finally
        {
            _responses.TryRemove(requestId, out _);
        }
    }

    public async Task ConsumeStream()
    {
        try
        {
            using StreamReader reader = new StreamReader(stream);

            // Receber mensagens do cliente, enquanto estiver conectado
            while (true)
            {
                string? line = await reader.ReadLineAsync();

                // Verificar se é um EOF (end of file)
                if (line == null)
                {
                    // The client has disconnected gracefully
                    Console.WriteLine("Client disconnected.");
                    break;
                }

                // Interpretar linha vinda do cliente
                string[] segments = line.Split(' ');
                if (line.StartsWith("REQ "))
                {
                    int requestId = int.Parse(segments[1]);
                    // Extrair o identificador do request
                    string requestKind = segments[2];
                    string? requestBody = null;

                    // Verificar se o identificador termina com dois pontos (ex. "OPEN:")
                    if (requestKind[requestKind.Length - 1] == ':')
                    {
                        // Remover dois pontos do identificador
                        requestKind = requestKind.Remove(requestKind.Length - 1);
                        requestBody = line.Substring(line.IndexOf(':') + 2);
                    }

                    // OnRequestReceived.(requestId, requestKind, requestBody);
                    // HandleRequest(int id, string kind, string? args)
                }
                else if (line.StartsWith("RES "))
                {
                    // Extrair o identificador do request que está sendo respondido
                    int requestId = int.Parse(segments[1]);

                    // Se o request ocorreu com sucesso, esse segmento será "OK"
                    string responseKind = segments[2];
                    string? responseBody = null;

                    // Verificar se o identificador termina com dois pontos (ex. "OK:")
                    if (responseKind[responseKind.Length - 1] == ':')
                    {
                        // Remover dois pontos do identificador
                        responseKind = responseKind.Remove(responseKind.Length - 1);
                        responseBody = line.Substring(line.IndexOf(':') + 2);
                    }

                    TaskCompletionSource<Response>? task;
                    if (_responses.TryGetValue(requestId, out task))
                    {
                        task.SetResult(new Response()
                        {
                            RequestID = requestId,
                            IsSuccess = responseKind == "OK",
                            PayloadSerialized = responseBody
                        });
                    }

                    // HandleResponse(int id, string responseKind, string? responseBody)
                }
            }
        }
        catch (IOException ex)
        {
            // This can occur if the network stream is interrupted
            Console.WriteLine($"Network error: {ex.Message}");
            throw;
        }
        catch (ObjectDisposedException ex)
        {
            // This can occur if the TcpClient or its stream is disposed
            Console.WriteLine($"Connection closed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // General exception handling for any other unforeseen errors
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }
}