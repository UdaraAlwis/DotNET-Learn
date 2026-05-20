using Basics;

namespace FirstGrpc.Tests.Integration
{
    public class FirstServiceTests : IClassFixture<MyFactory<Program>>
    {
        private readonly MyFactory<Program> _factory;

        public FirstServiceTests(MyFactory<Program> factory)
        {
            this._factory = factory;
        }

        [Fact]
        public void GetUnaryMessage()
        {
            // Arrange
            var client = _factory.CreateGrpcClient();
            var expectedResponse = new Response
            {
                Message = "Hello this is Server, I got your message!"
            };

            // Act
            var actualResponse = client.Unary(new Request { Content = "Hello, Server!" });

            // Assert
            Assert.Equal(expectedResponse.Message, actualResponse.Message);
        }
    }
}
