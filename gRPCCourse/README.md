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