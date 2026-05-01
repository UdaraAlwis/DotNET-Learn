using Basics;
using Grpc.Core;

namespace FirstGrpc.Services
{
    public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
    {
        public override Task<Response> Unary(Request request, ServerCallContext context)
        {
            // To disable compression for this response,
            // we can set the WriteOptions in the ServerCallContext to NoCompress.
            context.WriteOptions = new WriteOptions(WriteFlags.NoCompress);

            var response = new Response
            {
                Message = $"{request.Content}, I got your message! {context.Host}"
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
            var headerFirst = context.RequestHeaders.Get("my-first-key");
            var headerSecond = context.RequestHeaders.Get("my-second-key");

            Console.WriteLine($"Received headers from Client: {headerFirst!.Value }");
            Console.WriteLine($"Received headers from Client: {headerSecond!.Value }");

            var myTrailer = new Metadata.Entry("my-trailer-key", "my-trailer-value");
            context.ResponseTrailers.Add(myTrailer);

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
