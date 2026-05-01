using Basics;
using Grpc.Core;

namespace FirstGrpc.Services
{
    public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
    {
        public override Task<Response> Unary(Request request, ServerCallContext context)
        {
            var response = new Response
            {
                Message = request.Content + ", I got your message!"
            };
            return Task.FromResult(response);
        }

        public async override Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var requestPayload = requestStream.Current;
                Console.WriteLine($"Received from Client: { requestPayload }");
            }

            var response = new Response
            {
                Message = "I got all the data from Client!"
            };

            return response;
        }

        public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < 100; i++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var response = new Response() { Message = i.ToString() };
                await responseStream.WriteAsync(response);
            }
        }

        public async override Task BiDirectionalStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            var response = new Response
            {
                Message = ""
            };

            while (await requestStream.MoveNext())
            {
                var requestPayload = requestStream.Current;
                response.Message = $"Received from Client: { requestPayload.ToString() }";
                await responseStream.WriteAsync(response);
            }
        }
    }
}
