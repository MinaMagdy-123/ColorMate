using ColorMate.BL.UserService;
using ColorMate.Core.DTOs;
using ColorMate.Core.DTOs.FacebookDto;
using ColorMate.Core.Models;
using Google.Apis.Auth;
using JWT.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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


        public UsersController(IUserService userService,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
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



        //------------------------ Mina ----------------------------

        /*
        [HttpPost("registerV2")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(registerDto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            //SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }
        */


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _userService.GetTokenAsync(loginDto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }


        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {

            var result = await _userService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);


            return Ok(result);
        }


        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto revokeTokenDto)
        {
            var token = revokeTokenDto.Token;

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _userService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Token is invalid!");

            return Ok();
        }


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.ChangePasswordAsync(userId, dto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }


        //-----------------------------------------------------------


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

            var googleEmail = payload.Email;
            var googleFirstName = payload.GivenName;
            var googleLastName = payload.FamilyName;
            var googlePicture = payload.Picture;
            var googleSubject = payload.Subject;


            var user = await _userService.LoginWithGoogleAsync(new GoogleUserDto
            {
                Email = googleEmail,
                FirstName = googleFirstName,
                LastName = googleLastName,
                PictureUrl = googlePicture,
                Subject = googleSubject
            });

            if (user == null)
                return BadRequest("Failed to login/register with Google");

            var authResult = await _userService.GenerateAuthResultAsync(user);
            return Ok(authResult);
        }
       

        [HttpPost("LoginWithFacebook")]
        public async Task<IActionResult> LoginWithFacebook([FromBody] FacebookLoginDto facebookLoginDto)
        {
            if (string.IsNullOrWhiteSpace(facebookLoginDto.AccessToken))
                return BadRequest("Facebook access token is required");

            try
            {
                var user = await _userService.LoginWithFacebookAsync(facebookLoginDto.AccessToken);
                if (user == null)
                {
                    return BadRequest("Failed to login/register by Facebook");
                }

                var authResult = await _userService.GenerateAuthResultAsync(user);
                return Ok(authResult);
            }
            catch
            {
                return Unauthorized("Invalid Facebook token");
            }

        }

    }
}
