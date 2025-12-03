using ColorMate.BL.UserService;
using ColorMate.Core.DTOs;
using ColorMate.Core.DTOs.FacebookDto;
using ColorMate.Core.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ColorMate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;



        public UsersController(IUserService userService, IConfiguration config, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userService = userService;
            _config = config;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var (result, user, otp) = await _userService.RegisterUserAsync(registerDto);

            if (result.Succeeded)
            {
                // Send OTP via email
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Email Verification Code",
                    $"Your verification code is: {otp}"
                );

                return Ok(new { message = "Registration successful! Please check your email for the OTP." });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return BadRequest(ModelState);
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userService.CheckLoginAsync(loginDto);

            if (user == null)
                return BadRequest("Invalid credentials");

            var token = await _userService.GenerateTokenAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }



        [HttpPost("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginDto googleDto)
        {
            if (string.IsNullOrWhiteSpace(googleDto.IdToken))
                return BadRequest("Google ID Token is required");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    googleDto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["Authentication:Google:ClientId"] }
                    });
            }
            catch
            {
                return Unauthorized("Invalid Google token");
            }
            // Google user information
            var googleEmail = payload.Email;
            var googleFName = payload.Name;
            var googleLname = payload.GivenName;

            // Login or Register inside your system
            var user = await _userService.LoginWithGoogleAsync(new GoogleUserDto
            {
                Email = googleEmail,
                fName = googleFName,
                lName = googleLname,
                
            });

            if (user == null)
                return BadRequest("Failed to login/register with Google");

            // Generate your JWT
            var token = await _userService.GenerateTokenAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }

        [HttpPost("TestGoogleLogin")]
        public async Task<IActionResult> TestGoogleLogin([FromBody] GoogleUserDto googleUser)
        {
            var user = await _userService.LoginWithGoogleAsync(googleUser);
            var token = await _userService.GenerateTokenAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email
            });
        }


        [HttpPost("LoginWithFacebook")]
        public async Task<IActionResult> LoginWithFacebook([FromBody] FacebookLoginDto facebookLoginDto)
        {
            if (string.IsNullOrWhiteSpace(facebookLoginDto.AccessToken))
                return BadRequest("Facebook access token is required");

            var user =  await _userService.LoginWithFacebookAsync(facebookLoginDto.AccessToken);
            if(user == null)
            {
                return BadRequest("Failed to login/register by Facebook");
            }

            var token = await _userService.GenerateTokenAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.ProfilePictureUrl
                }
            });

        }



    }
}
