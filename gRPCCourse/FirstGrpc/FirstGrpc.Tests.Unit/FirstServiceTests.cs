using Basics;
using FirstGrpc.Services;
using FirstGrpc.Tests.Unit.Helpers;
using FluentAssertions;

namespace FirstGrpc.Tests.Unit
{
    public class FirstServiceTests
    {
        private readonly IFirstService sut;
        public FirstServiceTests()
        {
            sut = new FirstService();
        }

        [Fact]
        public async Task Unary_Should_Return_An_Object()
        {
            // Arrange
            var request = new Request
            {
                Content = "Hello, Server!"
            };

            var callContext = TestServerCallContext.Create();

            var expectedResponse = new Response
            {
                Message = "Hello this is Server, I got your message!"
            };

            // Act
            var actualResponse = await sut.Unary(request, callContext);

            // Assert
            actualResponse.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
