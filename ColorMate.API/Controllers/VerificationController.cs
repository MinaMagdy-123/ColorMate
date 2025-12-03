using ColorMate.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ColorMate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public VerificationController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPost("VerifyEmailOtp")]
        public async Task<IActionResult> VerifyEmailOtp(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return BadRequest("Email or code is missing.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found.");

            if (user.EmailVerificationCode?.Trim() != code.Trim())
                return BadRequest("Invalid OTP code.");

            if (user.EmailCodeExpiration < DateTime.UtcNow)
                return BadRequest("OTP code expired.");

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.EmailCodeExpiration = null;

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Email verified successfully!" });
        }

        [HttpPost("ResendOtp")]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found");

            string newOtp = new Random().Next(100000, 999999).ToString();

            user.EmailVerificationCode = newOtp;
            user.EmailCodeExpiration = DateTime.UtcNow.AddMinutes(5);

            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Your New Verification Code",
                $"Your new OTP is: {newOtp}"
            );

            return Ok("A new OTP has been sent");
        }
    }
}
