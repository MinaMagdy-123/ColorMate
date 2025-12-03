using ColorMate.Core.DTOs;
using ColorMate.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ColorMate.BL.UserService
{
    public interface IUserService
    {
        Task<(IdentityResult Result, ApplicationUser User, string? Otp)> RegisterUserAsync(RegisterDto registerDto);

        Task<ApplicationUser?> CheckLoginAsync(LoginDto loginDto);

        Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser user);


        Task<ApplicationUser> LoginWithGoogleAsync(GoogleUserDto googleUser);

        Task<ApplicationUser> LoginWithFacebookAsync(string accessToken);

    }
}
