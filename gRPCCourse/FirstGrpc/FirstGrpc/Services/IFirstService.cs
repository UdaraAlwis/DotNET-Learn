using Basics;
using Grpc.Core;

namespace FirstGrpc.Services
{
    public interface IFirstService
    {
        Task BiDirectionalStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context);
        Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context);
        Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context);
        Task<Response> Unary(Request request, ServerCallContext context);
    }
}