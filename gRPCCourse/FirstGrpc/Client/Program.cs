using Basics;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var retryPolicy = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 5,
        BackoffMultiplier = 1,
        InitialBackoff = TimeSpan.FromSeconds(0.5),
        MaxBackoff = TimeSpan.FromSeconds(0.5),
        RetryableStatusCodes = { StatusCode.Internal }
    }
};

var options = new GrpcChannelOptions
{
    ServiceConfig = new ServiceConfig
    {
        MethodConfigs = { retryPolicy }
    }
};

using var channel = GrpcChannel.ForAddress("https://localhost:7157", options);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

//// Client side Load Balancing
//var factory = new StaticResolverFactory(addr => new[]
//{
//    new BalancerAddress("localhost", 5057),
//    new BalancerAddress("localhost", 5058),
//});

//// Create a service collection and register the resolver factory
//var services = new ServiceCollection();
//services.AddSingleton<ResolverFactory>(factory);

//// Create the gRPC channel with the custom resolver and load balancing configuration
//var channel = GrpcChannel.ForAddress("static://localhost", new GrpcChannelOptions()
//{
//    Credentials = ChannelCredentials.Insecure,
//    ServiceConfig = new ServiceConfig
//    {
//        // Configure the load balancing policy to use round-robin load balancing method
//        LoadBalancingConfigs = { new RoundRobinConfig() }
//    },
//    ServiceProvider = services.BuildServiceProvider()
//});

//// Create the gRPC client using the channel
//var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

// Call the Grpc method you want to test here
Unary(client);
// ClientStreaming(client);
// ServerStreaming(client);
// BiDirectionalStreaming(client);

Console.ReadLine();

void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    // Enable compression for this request
    var metadata = new Metadata { { "grpc-accept-encoding", "gzip" } };

    var request = new Request() { Content = "Hello" };
    //// The deadline is set to 3 seconds to trigger the retry policy configured in the GrpcChannelOptions.
    //var response = client.Unary(request, deadline: DateTime.UtcNow.AddSeconds(3), headers: metadata);
    var response = client.Unary(request, headers: metadata);
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