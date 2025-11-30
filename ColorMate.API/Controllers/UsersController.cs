using ColorMate.BL.UserService;
using ColorMate.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ColorMate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration config;


        public UsersController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            this.config = config;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {

            var result = await _userService.RegisterUserAsync(registerDto);

            if (result.Succeeded)
            {
                return Ok("Created");
            }

            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
            }

            return BadRequest(ModelState);
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _userService.CheckLoginAsync(loginDto);

            if (result)
            {
                JwtSecurityToken token = await _userService.GenerateToken(loginDto);
                return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo });
            }
            return BadRequest();
        }


    }
}
