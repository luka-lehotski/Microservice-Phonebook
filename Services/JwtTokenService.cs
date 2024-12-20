using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var jti = Guid.NewGuid().ToString();
        Console.WriteLine($"Generated Jti: {jti}");

        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var signingKey = jwtSettings["SigningKey"];
        var expirationMinutes = int.Parse(jwtSettings["ExpiresInMinutes"]);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };
        Console.WriteLine("Subject: {0}\n", userId);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );
        PrintTokenClaims(new JwtSecurityTokenHandler().WriteToken(token).ToString());

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public static void PrintTokenClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        if (jsonToken != null)
        {
            Console.WriteLine("Claims in token:");
            foreach (var claim in jsonToken.Claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }
        }
        else
        {
            Console.WriteLine("Invalid token");
        }
    }
}
