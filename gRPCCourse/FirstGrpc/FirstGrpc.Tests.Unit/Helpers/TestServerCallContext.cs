using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstGrpc.Tests.Unit.Helpers
{
    public class  TestServerCallContext : ServerCallContext
    {
        private readonly Metadata requestHeaders;
        private readonly CancellationToken cancellationToken;

        private TestServerCallContext(Metadata requestHeaders, CancellationToken cancellationToken)
        {
            this.requestHeaders = requestHeaders;
            this.cancellationToken = cancellationToken;
        }

        public static TestServerCallContext Create()
        {
            return new TestServerCallContext(new Metadata(), CancellationToken.None);
        }

        protected override string MethodCore => "MethodName";

        protected override string HostCore => "HostName";

        protected override string PeerCore => "PeerName";

        protected override DateTime DeadlineCore { get; }

        protected override Metadata RequestHeadersCore => requestHeaders;

        protected override CancellationToken CancellationTokenCore => cancellationToken;

        protected override Metadata ResponseTrailersCore => throw new NotImplementedException();

        protected override Status StatusCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected override WriteOptions? WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override AuthContext AuthContextCore => throw new NotImplementedException();

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            throw new NotImplementedException();
        }
    }
}
