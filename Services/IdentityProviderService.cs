using Grpc.Core;
using Grpc.Net.Client;
using IdentityProviderMicroservice.User;
using System.Net.Http;

namespace IdentityProviderMicroservice.Services
{
    public class IdentityProviderService : IIdentityProviderService
    {
        private readonly UserGrpc.UserGrpcClient _userServiceClient;
        private readonly JwtTokenService _jwtTokenService;

        public IdentityProviderService(IConfiguration configuration, JwtTokenService jwtTokenService)
        {
            var userServiceAddress = Environment.GetEnvironmentVariable("GRPC_USER_SERVICE_ADDRESS");
            //var userServiceAddress = configuration["GrpcClient:UserServiceAddress"];

            // Configure HttpClientHandler to bypass SSL validation
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // Bypass validation

            var channel = GrpcChannel.ForAddress(userServiceAddress, new GrpcChannelOptions
            {
                HttpClient = new HttpClient(httpHandler)
            });

            _userServiceClient = new UserGrpc.UserGrpcClient(channel);
            _jwtTokenService = jwtTokenService;
        }

        public Task<AuthResult> VerifyUserPassword(string email, string password)
        {
            var request = new GetUserRequest
            {
                Email = email,
            };

            GetUserResponse response = _userServiceClient.GetUser(request);
            if (response.Exists == false)
                return Task.FromResult(new AuthResult { Status = "User doesn't exist", Jwt = "" });

            if (BCrypt.Net.BCrypt.Verify(password, response.Password) == false)
            {
                return Task.FromResult(new AuthResult { Status = "Wrong password", Jwt = "" });
            }

            return Task.FromResult(new AuthResult { Status = "Success", Jwt = _jwtTokenService.GenerateToken(response.Id.ToString()) });
        }
    }
}
