using System.Security.Cryptography;
using System.Text;
using DatingApp.API.Data;
using DatingApp.API.Data.Entities;
using DatingApp.API.DTO;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
namespace DatingApp.API.Controllers
{
    public class AuthController : BaseController
    {
        private  readonly DataContext _context;
        private  readonly ITokenService _tokenservice;
        public AuthController(DataContext context,ITokenService tokenservice){
            _context=context;
            _tokenservice=tokenservice;
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] AuthUserDto authUserDto)
        {
            authUserDto.Username=authUserDto.Username.ToLower();
            if(_context.AppUsers.Any(u=>u.Username ==authUserDto.Username)){
                return BadRequest("Username is already registered!");
            }
            using var hmac=new HMACSHA512();
            var passworBytes=hmac.ComputeHash(Encoding.UTF8.GetBytes(authUserDto.Password));
            var newUser=new User{
                Username=authUserDto.Username,
                PasswordSalt=hmac.Key,
                PasswordHash=passworBytes
            };
            _context.AppUsers.Add(newUser);
            _context.SaveChanges();
            var token=_tokenservice.CreateToken(newUser.Username);
            return Ok(new UserTokenDto {
                Username=newUser.Username,
                Token=token
            });
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody]AuthUserDto authUserDto)
        {
            authUserDto.Username=authUserDto.Username.ToLower();
            var currentUser=_context.AppUsers.FirstOrDefault(u=>u.Username==authUserDto.Username);
            if(currentUser==null){
                return Unauthorized("Username is invalid");
            }
            using var hmac=new HMACSHA512(currentUser.PasswordSalt);
            var passworBytes=hmac.ComputeHash(Encoding.UTF8.GetBytes(authUserDto.Password));
            for(int i=0;i<currentUser.PasswordHash.Length;i++){
                if(currentUser.PasswordHash[i] != passworBytes[i]){
                    return Unauthorized("Password is valid");
                }
            }
            var token=_tokenservice.CreateToken(currentUser.Username);
               return Ok(new UserTokenDto {
                Username=currentUser.Username,
                Token=token
            });
        }
        // [Authorize]
        [HttpGet]
        public IActionResult Get(){
            return Ok(_context.AppUsers.ToList());
        }
    }
}
