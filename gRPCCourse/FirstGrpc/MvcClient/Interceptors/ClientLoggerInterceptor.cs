using Grpc.Core.Interceptors;

namespace MvcClient.Interceptors
{
    public class ClientLoggerInterceptor : Interceptor
    {
        private readonly ILogger<ClientLoggerInterceptor> _logger;

        public ClientLoggerInterceptor(ILoggerFactory loggerFactory)
        {  
            this._logger = loggerFactory.CreateLogger<ClientLoggerInterceptor>();
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                _logger.LogInformation($"Starting the client call of type: {context.Method.FullName}, {context.Method.Type}");
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error thrown by {context.Method.FullName}");
                throw;
            }
        }
    }
}
