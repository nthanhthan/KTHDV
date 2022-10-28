
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DatingApp.API.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _congigration;
        public TokenService(IConfiguration configuration)
        {
         _congigration=configuration;   
        }
        public string CreateToken(string username)
        {
            var claims=new List<Claim>(){
                new Claim(JwtRegisteredClaimNames.NameId, username),
                new Claim(JwtRegisteredClaimNames.Email,$"{username}@dating.app"),
            };
            var symmetrickey=new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_congigration["TokenKey"])
            );
            var tokenDescriptor=new SecurityTokenDescriptor{
                Subject=new ClaimsIdentity(claims),
                Expires=DateTime.Now.AddDays(1),
                SigningCredentials=new SigningCredentials(symmetrickey,SecurityAlgorithms.HmacSha512Signature)
            };
             var tokenHandler=new JwtSecurityTokenHandler();
             var token=tokenHandler.CreateToken(tokenDescriptor);
             return tokenHandler.WriteToken(token);
        }
    }
}