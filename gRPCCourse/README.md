# gRPC in .NET

**Course:** [gRPC in .NET](https://dometrain.com/course/from-zero-to-hero-grpc-in-dotnet/)

I took this course to learn how to build high-performance, contract-first services in .NET using gRPC. The course walks through everything from the fundamentals of Protocol Buffers and HTTP/2 to building production-ready gRPC services in C#, covering all four communication patterns: unary, server streaming, client streaming, and bidirectional streaming.

Along the way, we explored core concepts like defining `.proto` contracts, generating client and server code, handling deadlines and cancellation, error handling with status codes, interceptors, and authentication. We also looked at how gRPC compares to REST and when each one is the right tool for the job.

Towards the end, we built reusable gRPC clients and integrated them into client applications, learning how to share contracts across projects cleanly.

Finally, we touched on more advanced scenarios like gRPC-Web for browser clients and hosting gRPC services alongside existing ASP.NET Core APIs.

I followed along with the course instructor, implementing each feature step-by-step. At the same time, I made sure to take notes and write down important code snippets for future reference. I hope this documentation will be helpful for others looking to learn about building gRPC services with .NET.

I highly recommend this course to anyone interested in backend development with .NET, it provides a solid foundation for building fast, strongly-typed, cross-platform service-to-service communication!

So, here we go!

## Sneak Peek: Final Working Solution

PENDING

## GRPC

gRPC is a high-performance, open-source framework for building remote procedure call (RPC) APIs, with the use of Binary Serialization.

It uses Protocol Buffers (protobuf) as its interface definition language and supports multiple programming languages. gRPC is designed for low latency and high throughput communication between services, making it ideal for microservices architectures.

Popular around Microservices and Cloud Native applications, gRPC is used by companies like Google, Netflix, and Square for building efficient and scalable APIs.

### gRPC vs WCF

- gRPC can replace WCF or SOAP services in .NET, providing a modern, cross-platform alternative for building APIs. It can also be used alongside REST APIs, allowing you to choose the right tool for each use case.

- With WCF you can only host on Windows, while gRPC can be hosted on any platform that supports .NET, including Linux and macOS.

- You can define your data structures in your programming language, and gRPC tooling will use this code to generate the necessary .proto files and server/client stubs. 

### gRPC vs REST

- **Contract first approach**: Protocol Buffers vs JSON/XML for data exchange.
- **Protocol**: HTTP/2 with multiplexing and server push vs HTTP/1.1 and HTTP/2.
- **Payload**: Compact binary format vs human-readable JSON or XML.
- **Strongly-typed**: Generated client and server code from .proto definitions vs dynamic typing.
- **Streaming**: Supports unary, server, client, and bidirectional streaming vs request-response pattern.
- **Client code generation**: Multi-language code generation tools vs manual implementation or third-party libraries.
- **Security**: Built-in TLS support vs additional configuration required.
- **Developer perspective**: Method-calling approach vs request-response approach. 

When to use gRPC:
- Microservices communication within a trusted network
- For low-latency communication between services
- Polyglot environments where multiple programming languages are used
- Performance-critical applications that require efficient serialization and transport
- IPC (Inter-Process Communication) on the same machine
- As a replacement for WCF or SOAP services in .NET

## Protocol Buffers

Language and platform-neutral mechanism for serializing structured data, used by gRPC for defining service contracts and message formats. It provides a compact binary format for efficient communication between services.

Strongly typed, with binary serialization, and supports schema evolution, making it ideal for building APIs that need to evolve over time without breaking existing clients.

Backwards and forwards compatible, allowing you to add new fields to your messages without breaking existing clients, as long as you follow the rules for field numbering and optional/required fields.

Extensible and multi-language support, with code generation tools for many programming languages, making it easy to integrate into different tech stacks.

### Proto file

The 'contract' between the client and server in gRPC is defined in a `.proto` file.

First, we define the version of the protocol syntax and the namespace for the generated code, and package name for the generated code, which is used to organize the generated classes and avoid naming conflicts. For example:
```proto
syntax = "proto3";
option csharp_namespace = "FirstGrpc";

package greet;
```

Then we define types, with field numbers, which are used to identify fields in the binary format and must be unique within a message type. For example:
```proto
message HelloRequest {
    string name = 1;
}
message HelloReply {
    string message = 1;
}
```

Finally, we define the service operations. Each method has a name, input message type, and output message type. For example:
```proto
service Greeter {
    rpc SayHello (HelloRequest) returns (HelloReply);
}
```

Scalar types: include `double`, `float`, `int32`, `int64`, `uint32`, `uint64`, `sint32`, `sint64`, `fixed32`, `fixed64`, `sfixed32`, `sfixed64`, `bool`, `string`, and `bytes`.

Enums: are defined with a name and a set of named values. For example:
```proto
enum Status {
    UNKNOWN = 0;
    ACTIVE = 1;
    INACTIVE = 2;
}
```

Well-known types: are predefined message types provided by the Protocol Buffers library, such as `google.protobuf.Timestamp` for representing date and time values.

Method with empty request or response: you can use the `google.protobuf.Empty` message type to represent an empty request or response. For example:
```proto
import "google/protobuf/empty.proto";
service MyService {
    rpc DoSomething (google.protobuf.Empty) returns (google.protobuf.Empty);
}
```

Nullable types: To represent nullable fields, you can use Well-known types like `google.protobuf.StringValue` for strings, `google.protobuf.Int32Value` for integers, etc. For example:
```proto
import "google/protobuf/wrappers.proto";
message MyMessage {
    google.protobuf.StringValue optional_string = 1;
    google.protobuf.Int32Value optional_int = 2;
}
```

Any type: The `google.protobuf.Any` type can be used to represent any arbitrary message type, allowing for more flexible and dynamic message structures. For example:
```proto
import "google/protobuf/any.proto";

message Request {
    string id = 1;
    google.protobuf.Any animal = 2;
}

message Cat {
    string breed = 1;
    string name = 2;
    int32 age = 3;
}

message Dog {
    string breed = 1;
    string name = 2;
    int32 age = 3;
}
```
The client can then pack a `Cat` or `Dog` message into the `Any` field when sending the request, and the server can unpack it based on the type information included in the message.

Oneof types: allow you to define a set of fields where only one can be set at a time. For example:
```proto
message PaymentMethod {
    oneof payment {
        CreditCard credit_card = 1;
        BankTransfer bank_transfer = 2;
        DigitalWallet digital_wallet = 3;
    }
}

message CreditCard {
    string card_number = 1;
    int32 expiry_month = 2;
    int32 expiry_year = 3;
}

message BankTransfer {
    string account_number = 1;
    string routing_number = 2;
}

message DigitalWallet {
    string wallet_id = 1;
    string provider = 2;
}
```

We can nest messages within other messages to create more complex data structures. Also use repeated fields to represent lists of values. For example:
```proto
message Person {
    string name = 1;
    int32 age = 2;

    enum PhoneType {
        MOBILE = 0;
        HOME = 1;
        WORK = 2;
    }

    message PhoneNumber {
        string number = 1;
        PhoneType type = 2;
    }
    repeated PhoneNumber phone_numbers = 3;
}
```

## Method types

- **Unary**: Single request, single response.
- **Client Streaming**: Client sends a stream of messages, server replies once with a single response.
- **Server Streaming**: Client sends a single request, server replies with a stream of messages.
- **Bidirectional Streaming**: Both client and server send streams of messages to each other simultaneously.

In every case, the client initiates the call and the server responds. The difference is in how many messages are sent and received in each direction.

## Grpc Channel

The abstraction for a connection to a gRPC service. It manages the underlying HTTP/2 connection and allows you to create gRPC clients that can call methods on the server. 
Once you have created a channel, you can use it to make several gRPC calls to the same server, which can improve performance by reusing the connection.

## First GRPC Service

Create a new ASP.NET Core gRPC service project in Visual Studio. 
This will set up the basic structure of your gRPC service, including a sample `.proto` file and generated code.

Create a new `.proto` file called `first.proto` in the `Protos` folder to define your service contract. 
```proto
syntax = "proto3";

option csharp_namespace = "Basics";

package basics;

message Request {
	string content = 1;
}

message Response {
	string message = 2;
}

service FirstServiceDefinition {
	rpc Unary(Request) returns (Response);
	rpc ClientStream (stream Request) returns (Response);
	rpc ServerStream (Request) returns (stream Response);
	rpc BiDirectionalStream (stream Request) returns (stream Response);
}
```

Edit the project file to include the new `.proto` file and specify the gRPC code generation options. For example:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  ...
  
  <ItemGroup>
	  <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
	  <Protobuf Include="Protos\first.proto" GrpcServices="Server" />
  </ItemGroup>

  ...
</Project>
```

Then implement the service called FirstService by creating a new class that inherits from the generated base class for your service definition.
```csharp
public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
{
    public override Task<Response> Unary(Request request, ServerCallContext context)
    {
        ... // Handle unary logic here
    }

    public async override Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        ... // Handle client streaming logic here
    }

    public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        ... // Handle server streaming logic here
    }

    public async override Task BiDirectionalStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        ... // Handle bidirectional streaming logic here
    }
}
```

Finally, register the service in the `Startup.cs` file to make it available for clients to call.
```csharp
var app = builder.Build();

...
app.MapGrpcService<FirstService>();

app.Run();
```

![First Grpc Service demo](./Screenshots/1%20FirstGrpc%20Service.jpg)

### Create a gRPC Client

To create a gRPC client, you can create a new console application called `Client` project in the same Solution level in Visual Studio.

Then add the reference to the gRPC service project to the client project, so that the client can access the generated code from the `.proto` files.

Right click on `Client` project -> Add -> Connected Service -> Service References (OpenApi, gRPC, ...) -> gRPC -> File -> Select the  `first.proto` file -> Finish.

![Connect Client app for Grpc Service](./Screenshots/2%20Connect%20Client%20app%20for%20Grpc%20Service.jpg)

Then it will import the necessary NuGet packages and generate the client code based on the `.proto` file.

In the csproj file, you should see something like this:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  ...

  <ItemGroup>
    <PackageReference Include="Google.Protobuf" Version="3.21.5" />
    <PackageReference Include="Grpc.Net.ClientFactory" Version="2.49.0" />
    <PackageReference Include="Grpc.Tools" Version="2.49.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <Protobuf Include="..\FirstGrpc\Protos\first.proto" GrpcServices="Client">
      <Link>Protos\first.proto</Link>
    </Protobuf>
  </ItemGroup>

</Project>
```

Then in the `Program.cs` file of the client project, you can create a gRPC channel to connect to the server and create a client instance to call the service methods. For example:
```csharp
var options = new GrpcChannelOptions
{
    ... // Configure channel options if needed, such as credentials, timeouts, etc.
};

using var channel = GrpcChannel.ForAddress("https://localhost:7157", options);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

Unary(client);
Console.ReadLine();

void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    ... // Call the Unary method and handle the response
}
```

![Running Client for Grpc Service](./Screenshots/3%20Running%20Client%20with%20for%20Grpc%20Service.jpg)

### Deadlines and Cancellation

Deadline: Specify how long the client is willing to wait for a response from the server before time out. This can be done by setting a  `deadline ` on the gRPC call. For example:
```csharp
var request = new Request() { Content = "Hello" };
var response = client.Unary(request, deadline: DateTime.UtcNow.AddMilliseconds(3));
```

Cancellation: Allow the client to cancel an ongoing gRPC call if it is no longer needed or if it is taking too long. This can be done using a `CancellationToken` that is passed to the gRPC call. For example:
```csharp
var cancellationToken = new CancellationTokenSource();
using var streamingCall = client.ServerStream(new Request() { Content = "Hello" });

await foreach (var response in streamingCall.ResponseStream.ReadAllAsync(cancellationToken.Token))
{
    ...
}
```

but make sure to handle the cancellation on the server side, by checking the `CancellationToken` in the `ServerCallContext` and gracefully terminating the call if cancellation is requested. For example:
```csharp
public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
{
    for (int i = 0; i < 100; i++)
    {
        if(context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        ... // Send response to client
    }
}
```

### Create a MVC gRPC Client

Add the GRPC client the same way as before, by adding the reference to the gRPC service project and importing the `.proto` file in the MVC project.

Then in the `Program.cs` file of the MVC project, you can create a gRPC channel and register the gRPC client in the dependency injection container. For example:
```csharp
builder.Services.AddGrpcClient<FirstServiceDefinition.FirstServiceDefinitionClient>(options =>
{
    options.Address = new Uri("https://localhost:7157");
});
```

Then you can inject the gRPC client into your MVC controllers and use it to call the gRPC service methods. For example:
```csharp
public class HomeController (FirstServiceDefinition.FirstServiceDefinitionClient client) : Controller
{
    public IActionResult Index()
    {
        var firstCall = client.Unary(new Request { Content = "Hello from MVC Client!" });

        return View();
    }
}
```

![Connect MVC Client app for Grpc Service](./Screenshots/4%20Connect%20MVC%20Client%20app%20for%20Grpc%20Service.jpg)

## Http Request and Response in gRPC

Request,
Inside a HTTP request body we can have a single or multiple, stream of gRPC messages, depending on the method type (unary, client streaming, server streaming, bidirectional streaming). 

There are two parts to the gRPC message format,
- the header/metadata which contains information about the message, such as content type, encoding, and any custom metadata fields
- body which contains the actual message data.

On the client side we can attach headers/metadata to the gRPC call, using the `Metadata` object. For example:
```csharp
var metadata = new Metadata();
metadata.Add("my-first-key", "my-first-value");
metadata.Add("my-second-key", "my-second-value");

using var streamingCall = client.ServerStream(new Request() { Content = "Hello" }, metadata);
```

Then you can access the metadata on the server side using the `ServerCallContext` object. For example:
```csharp
public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
{
    var headerFirst = context.RequestHeaders.Get("my-first-key");
    var headerSecond = context.RequestHeaders.Get("my-second-key");

    Console.WriteLine($"Received headers from Client: {headerFirst!.Value }");
    Console.WriteLine($"Received headers from Client: {headerSecond!.Value }");

    ... // Handle server streaming logic here
}
```

Response and Trailers,
Inside a HTTP response body we can have a single or multiple, stream of gRPC messages same as the request, depending on the method type.

At the end of the response, there are Trailers, which are sent after all the grpc messages have been sent. Trailers contain additional metadata about the response, such as status codes, error messages, and any custom metadata fields.

On the server side, you can set trailers using the `ServerCallContext` object. For example:
```csharp
public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
{
    var myTrailer = new Metadata.Entry("my-trailer-key", "my-trailer-value");
    context.ResponseTrailers.Add(myTrailer);
    ... // Handle server streaming logic here
}
```

Then you can access the trailers on the client side after the call has completed. For example:
```csharp
async void ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    ... // Call the ServerStream method and handle the response stream

    var myTrailer = streamingCall.GetTrailers().Get("my-trailer-key");
    Console.WriteLine($"Received trailer from Server: {myTrailer?.Value}");
}
```

Status Codes,
gRPC uses a set of standard status codes to indicate the outcome of a gRPC call that are different from HTTP status codes. These status codes are sent in the trailers of the HTTP response.
```csharp
try
{
    ... // Call a gRPC method that may throw an exception or return an error status code
    
    var status = streamingCall.GetStatus();
    Console.WriteLine($"Call status: {status.StatusCode}");
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
{
    Console.WriteLine("Call was cancelled.");
}
```

![Handling Metadata, Trailers and Status Codes](./Screenshots/5%20Handling%20metadata%20and%20trailers%20and%20status%20codes.jpg)

## Interceptors

Interceptors in gRPC allows you to intercept and modify gRPC calls on both the client and server sides. 
They can be used for various purposes, such as logging, authentication, error handling, and more.

We can attach several interceptors to a gRPC channel, and they will be executed in the order they were added.

Client Interceptors,
- BlockingUnaryInterceptor: Intercept unary calls and block them based on certain conditions, such as authentication or rate limiting.
- AsyncUnaryInterceptor: Intercept unary calls and perform asynchronous operations, such as logging or modifying the request/response.
- AsyncClientStreamingInterceptor: Intercept client streaming calls and perform asynchronous operations on the request stream.
- AsyncServerStreamingInterceptor: Intercept server streaming calls and perform asynchronous operations on the response stream.
- AsyncDuplexStreamingInterceptor: Intercept bidirectional streaming calls and perform asynchronous operations on both the request and response streams.

On the Client side, you can create an interceptor by inheriting from the `Interceptor` class and overriding the appropriate methods for the type of calls you want to intercept.
For example,
```csharp
public class ClientLoggerInterceptor : Interceptor
{
    ... // You can inject dependencies like ILogger here

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request, 
        ClientInterceptorContext<TRequest, TResponse> context, 
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        try
        {
            _logger.LogInformation($"Starting the client call fo type: {context.Method.FullName}, {context.Method.Type}");
            return continuation(request, context);
        }
        catch (Exception ex)
        {
            ... // Log the exception
        }
    }
}
```

Then you can add the interceptor to the gRPC client configuration in the `Program.cs` file. For example:
```csharp
...
builder.Services.AddTransient<ClientLoggerInterceptor>();

builder.Services.AddGrpcClient<FirstServiceDefinition.FirstServiceDefinitionClient>(options =>
{
    options.Address = new Uri("https://localhost:7157");
}).AddInterceptor<ClientLoggerInterceptor>();
...
```

Server Interceptors,
- UnaryServerInterceptor: Intercept unary calls and perform operations before and after the call is handled by the server method.
- ClientStreamingServerInterceptor: Intercept client streaming calls and perform operations on the request stream before and after the call is handled by the server method.
- ServerStreamingServerInterceptor: Intercept server streaming calls and perform operations on the response stream before and after the call is handled by the server method.
- DuplexStreamingServerInterceptor: Intercept bidirectional streaming calls and perform operations on both the request and response streams before and after the call is handled by the server method.

On the server side, you can create an interceptor by inheriting from the `Interceptor` class and overriding the appropriate methods for the type of calls you want to intercept. For example:
```csharp
public class ServerLoggingInterceptor : Interceptor
{
    ... // You can inject dependencies like ILogger here

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, 
        ServerCallContext context, 
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            _logger.LogInformation($"Server Intercepting here!");
            return await continuation( request, context );
        }
        catch (Exception ex)
        {
            ... // Log the exception
        }
    }
}
```

Then you can add the interceptor to the gRPC server configuration in the `Program.cs` file. For example:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(option =>
{
    option.Interceptors.Add<ServerLoggingInterceptor>();
});

...
```

![Client and Server Interceptors](./Screenshots/6%20Client%20and%20Server%20Interceptors.jpg)

## Compression

We can modify the compression settings for gRPC calls to optimize performance.
gRPC supports several compression algorithms, such as gzip, deflate, and snappy.

On the server side, you can enable compression for responses by setting the `ResponseCompressionAlgorithm` property in the gRPC service configuration. For example:
```csharp
builder.Services.AddGrpc(options =>
{
    ...
    option.ResponseCompressionAlgorithm = "gzip";
    option.ResponseCompressionLevel = CompressionLevel.SmallestSize;
});
```

Then in the client side, you can enable compression for your request by adding the metadata header `grpc-accept-encoding` with the value of the desired compression algorithm. For example:
```csharp
var metadata = new Metadata { { "grpc-accept-encoding", "gzip" } };

var request = new Request() { Content = "Hello" };
var response = client.Unary(request, deadline: DateTime.UtcNow.AddSeconds(3), headers: metadata);
```

![Configuring Compression](./Screenshots/7%20Configuring%20Compression.jpg)

### Disable Compression for specific calls

You can disable compression for specific gRPC endpoints in the server by setting the `WriteOptions` on the `ServerCallContext` to `WriteFlags.NoCompress`. For example:
```csharp
public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
{
    public override Task<Response> Unary(Request request, ServerCallContext context)
    {
        context.WriteOptions = new WriteOptions(WriteFlags.NoCompress);
        ... // Handle unary logic here
    }
}
```

## Client Side load balancing

We can configure client-side load balancing in gRPC by providing a list of server addresses to the gRPC channel and using a load balancing policy to distribute requests across those servers.

Imaging we have two instances of the gRPC server running on different ports, for example, one on `localhost:5057` and another on `localhost:5058`. We can create a custom resolver factory that returns these addresses to the gRPC channel. For example:
```csharp
// Client side Load Balancing
var factory = new StaticResolverFactory(addr => new[]
{
    new BalancerAddress("localhost", 5057),
    new BalancerAddress("localhost", 5058),
});

// Create a service collection and register the resolver factory
var services = new ServiceCollection();
services.AddSingleton<ResolverFactory>(factory);

// Create the gRPC channel with the custom resolver and load balancing configuration
var channel = GrpcChannel.ForAddress("static://localhost", new GrpcChannelOptions()
{
    Credentials = ChannelCredentials.Insecure,
    ServiceConfig = new ServiceConfig
    {
        // Configure the load balancing policy to use round-robin load balancing method
        LoadBalancingConfigs = { new RoundRobinConfig() }
    },
    ServiceProvider = services.BuildServiceProvider()
});

// Create the gRPC client using the channel
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);
```

Then in the server side you can access `context.Host` and return it in the response message to verify that the requests are being distributed across both server instances. For example:
```csharp
public override Task<Response> Unary(Request request, ServerCallContext context)
{
    ...
    var response = new Response
    {
        Message = $"{request.Content}, I got your message! {context.Host}"
    };
    return Task.FromResult(response);
}
```

So now to test this client-side load balancing configuration, we can run two instances of the gRPC server on different ports, for example:
```bash
dotnet run --urls="http://localhost:5057"
dotnet run --urls="http://localhost:5058"
```

Then when we make gRPC calls using the client, the requests will be distributed across both server instances according to the round-robin load balancing policy. 
You can verify this by checking the response messages that the client receives, which should indicate which server instance handled each request based on the `context.Host` value included in the response message.

![Client Side Load Balancing](./Screenshots/8%20Client%20side%20load%20balancing.jpg)

## Transient Fault handling

Temporary faults or network glitches can occur in distributed systems, and gRPC provides mechanisms for handling these transient faults gracefully.

Retry Policies: Automatically retry failed gRPC calls on the client for specific transient errors (for example, selected status codes).

Hedging Policies: Send the same request to multiple servers in parallel, use the first successful response, and cancel the rest.

### Retry Policy

You can configure a retry policy for gRPC calls on the client side by setting the `ServiceConfig` property in the `GrpcChannelOptions`. For example:
```csharp
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
```

Then to simulate a transient fault on the server side, you can throw an `RpcException` with a status code that is included in the `RetryableStatusCodes` of the retry policy. 
For example:
```csharp
public override Task<Response> Unary(Request request, ServerCallContext context)
{
    // For Testing, to check if this is the first attempt of the RPC call
    // The "grpc-previous-rpc-attempts" header is added by gRPC client when it retries a call
    if (!context.RequestHeaders.Where(x => x.Key == "grpc-previous-rpc-attempts").Any())
    {
        // This is the first attempt of the RPC call
        throw new RpcException(new Status(StatusCode.Internal, "This is the first attempt, please retry!"));
    }

    ... // Handle unary logic here
}
```

You can see during debug the `grpc-previous-rpc-attempts` value keeps increasing with each retry attempt.

![Retry Policy Demo](./Screenshots/9%20Client%20side%20retry%20policy.jpg)

### Hedging Policy


TBC!

Learning on going...
