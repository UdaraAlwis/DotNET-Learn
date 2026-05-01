using Basics;
using Grpc.Net.Client;

Console.WriteLine("Hello, World!");

var options = new GrpcChannelOptions
{

};

using var channel = GrpcChannel.ForAddress("https://localhost:7157", options);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

// Unary(client);
ClientStreaming(client);

Console.ReadLine();

void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request() { Content = "Hello" };
    var response = client.Unary(request);
    Console.WriteLine(response);
}

async void ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var requestStream = client.ClientStream();
    for (int i = 0; i < 1000; i++)
    {
        var request = new Request() { Content = i.ToString() };
        await requestStream.RequestStream.WriteAsync(request);
    }

    await requestStream.RequestStream.CompleteAsync();
    var response = await requestStream.ResponseAsync;

    Console.WriteLine(response);
}