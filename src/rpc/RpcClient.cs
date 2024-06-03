using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

// Referência https://jonathancrozier.com/blog/implementing-the-request-response-pattern-using-c-sharp-with-json-rpc-and-websockets

class RpcClient(NetworkStream stream)
{
    public readonly NetworkStream stream = stream;
    public Func<Request<object>, Task<object>>? ReqHandler = null;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> _pendingResponses = new();
    private int _requestCounter = 0;

    // Envia um request para o outro par.
    public async Task<Res> SendRequest<Req, Res>(string kind, Req payload, bool requireResponse = true)
    {
        int requestId = ++_requestCounter;
        string imperative = kind.Trim().ToUpper();
        string serializedPayload = JsonSerializer.Serialize(payload);

        // Registrar que queremos receber uma resposta relacionada ao requestId eventualmente
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        Task<string> task = tcs.Task;
        _pendingResponses.TryAdd(requestId, tcs);

        try
        {
            // Enviar requisição pelo socket
            string requestLine = $"REQ {requestId} {imperative}: {serializedPayload}\n";
            await stream.WriteAsync(Encoding.UTF8.GetBytes(requestLine));

            // Esperar nesse ponto até que a resposta seja recebida na outra função
            Task.WaitAll([task]);

            if (task.IsCompleted)
            {
                // Obtemos nossa resposta em task.Result, devemos interpretá-la
                string serializedResponse = task.Result;
                Response<Res> response = Response<Res>.FromResponseString(serializedResponse);

                if (!response.IsSuccessful)
                {
                    throw new Exception($"Resposta falhou! {response.Content}");
                }

                return response.Content;
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
            _pendingResponses.TryRemove(requestId, out _);
        }
    }

    public async Task StartListening()
    {
        try
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

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

                if (line.StartsWith("RES "))
                {
                    Response<object> response = Response<object>.FromResponseString(line);

                    // Encontrar a task referente a essa resposta
                    TaskCompletionSource<string>? task;
                    if (_pendingResponses.TryGetValue(response.RequestId, out task))
                    {
                        task.SetResult(line);
                    }
                }
                else if (line.StartsWith("REQ "))
                {
                    Request<object> req = Request<object>.FromRequestString(line);

                    // Lidar com esse request
                    Response<object> res;

                    // Verificar se essa procedure existe...
                    try
                    {
                        object returnedValue = await ReqHandler!(req);
                        res = new Response<object>(req.RequestId, true, returnedValue);
                    }
                    catch (Exception ex)
                    {
                        res = new Response<object>(req.RequestId, false, new Dictionary<string, string> {
                            { "error", ex.ToString() }
                        });
                    }

                    // Enviar resposta no socket, com encoding UTF8
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(res.ToString()));
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