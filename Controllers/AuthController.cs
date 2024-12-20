using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IdentityProviderMicroservice.Services;
using IdentityProviderMicroservice.User;
using IdentityProviderMicroservice;

namespace IdentityProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityProviderService _identityProviderService;

        public AuthController(IIdentityProviderService identityProviderService)
        {
            _identityProviderService = identityProviderService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            AuthResult result = await _identityProviderService.VerifyUserPassword(loginRequest.Email, loginRequest.Password);

            if (result.Status=="Success")
            { 
                // Generate JWT token or handle successful login
                return Ok(new { Token = result.Jwt });
            }
            if(result.Status == "Wrong password")
            {
                return Unauthorized(new { message = "Invalid password." });
            }
            if (result.Status == "User doesn't exist")
            {
                return Unauthorized(new { message = result.Status });
            }
            return BadRequest();
        }
    }
}

