using ColorMate.BL.FacebookService;
using ColorMate.Core.DTOs;
using ColorMate.Core.Models;
using ColorMate.EF.Repositories.User;
using ColorMate.EF.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ColorMate.BL.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly UserManager<ApplicationUser> _userManager; // add user
        private readonly SignInManager<ApplicationUser> _signInManager; // sign in
        private readonly IConfiguration config;
        private readonly IFacebookAuthService _facebookAuthService;


        public UserService(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,IFacebookAuthService facebookAuthService )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            this.config = config;
            _facebookAuthService = facebookAuthService;
        }

        public async Task<(IdentityResult Result, ApplicationUser User, string? Otp)> RegisterUserAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                LoginProvider= "Local"
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                user.EmailVerificationCode = otp;
                user.EmailCodeExpiration = DateTime.UtcNow.AddMinutes(5);
                await _userManager.UpdateAsync(user);

                return (result, user, otp);
            }

            return (result, user, null);
        }





        public async Task<ApplicationUser?> CheckLoginAsync(LoginDto loginDto)
        {
            ApplicationUser? user = loginDto.UserName.Contains("@")
                ? await _userManager.FindByEmailAsync(loginDto.UserName)
                : await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
                return null;

            bool isValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            return isValid ? user : null;
        }





        public async Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
         new Claim("Provider", user.LoginProvider ?? "Local") 
    };

            // Add roles if exist
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JWT:SecritKey"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["JWT:IssuerURL"],
                audience: config["JWT:AudienceURL"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return token;
        }





        public async Task<ApplicationUser> LoginWithGoogleAsync(GoogleUserDto googleUser)
        {
            var user = await _userManager.FindByEmailAsync(googleUser.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = googleUser.Email,
                    Email = googleUser.Email,
                    FirstName = googleUser.fName,
                    LastName = googleUser.lName,
                    LoginProvider = "Google",
                    ProfilePictureUrl = googleUser.PictureUrl

                };
                await _userManager.AddLoginAsync(user, new UserLoginInfo(
                      "Google",
                       googleUser.Email,
                       "Google"
       ));
                var createuser = await _userManager.CreateAsync(user);
                if (!createuser.Succeeded)
                    return null;
            }

            return user;
        }




        public async Task<ApplicationUser> LoginWithFacebookAsync(string accessToken)
        {
            var validateTokenResult = await _facebookAuthService.ValitadeAccessTokenAsync(accessToken);
            if (!validateTokenResult.Data.IsValid)
            {
                return null;
            }
            var userInfo = await _facebookAuthService.GetUserInfoAsync(accessToken);
            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    UserName = userInfo.Email,
                    LoginProvider = "Facebook",
                    ProfilePictureUrl = userInfo.FacebookPicture.Data.Url.ToString()
                };

                var createdResult = await _userManager.CreateAsync(user);
                if (!createdResult.Succeeded)
                {
                    return null;
                }
            }
            return user;
        }
    }
}
