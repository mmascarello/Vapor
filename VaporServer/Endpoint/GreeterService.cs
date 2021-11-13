
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace VaporServer.Endpoint
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }
       public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CreateUserResponse
            {
                Name = "Request received with: " + request.Name
            });
        }
    }
}