using Basics;
using Grpc.Core;
using Grpc.Net.Client;

Console.WriteLine("Hello, World!");

var options = new GrpcChannelOptions
{

};

using var channel = GrpcChannel.ForAddress("https://localhost:7157", options);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

Unary(client);
// ClientStreaming(client);
// ServerStreaming(client);
// BiDirectionalStreaming(client);

Console.ReadLine();

void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var metadata = new Metadata { { "grpc-accept-encoding", "gzip" } };

    var request = new Request() { Content = "Hello" };
    var response = client.Unary(request, deadline: DateTime.UtcNow.AddSeconds(3), headers: metadata);
    Console.WriteLine($"Response from Server: {response}");
}

async void ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var requestStream = client.ClientStream();
    for (int i = 0; i < 1000; i++)
    {
        var request = new Request() { Content = i.ToString() };
        await requestStream.RequestStream.WriteAsync(request);
    }

    await requestStream.RequestStream.CompleteAsync();
    var response = await requestStream.ResponseAsync;

    Console.WriteLine(response);
}

async void ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var cancellationToken = new CancellationTokenSource();

    var metadata = new Metadata();
    metadata.Add("my-first-key", "my-first-value");
    metadata.Add("my-second-key", "my-second-value");

    try
    {
        using var streamingCall = client.ServerStream(new Request() { Content = "Hello" }, metadata);

        await foreach (var response in streamingCall.ResponseStream.ReadAllAsync(cancellationToken.Token))
        {
            Console.WriteLine($"Response from Server: {response}");

            //Simulate cancellation when the message contains "2"
            //if (response.Message.Contains("2"))
            //{
            //    cancellationToken.Cancel();
            //}
        }

        var myTrailer = streamingCall.GetTrailers().Get("my-trailer-key");
        Console.WriteLine($"Received trailer from Server: {myTrailer?.Value}");

        var status = streamingCall.GetStatus();
        Console.WriteLine($"Call status: {status.StatusCode}");
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    {
        Console.WriteLine("Call was cancelled.");
    }
}

async void BiDirectionalStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var streamingCall = client.BiDirectionalStream();
    {
        var request = new Request();

        for (int i = 0; i < 10; i++)
        {
            request.Content = i.ToString();
            Console.WriteLine($"Sending to Server: {request.Content}");
            await streamingCall.RequestStream.WriteAsync(request);
        }

        while (await streamingCall.ResponseStream.MoveNext())
        {
            var message = streamingCall.ResponseStream.Current;
            Console.WriteLine($"Received from Server: {message}");
        }
    }
}