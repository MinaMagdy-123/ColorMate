using ColorMate.Core.DTOs;
using ColorMate.Core.Models;
using ColorMate.EF.Repositories.User;
using ColorMate.EF.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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


        public UserService(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            this.config = config;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            return result;
        }

        public async Task<bool> CheckLoginAsync(LoginDto loginDto)
        {
            ApplicationUser? userFromDB = await _userManager.FindByNameAsync(loginDto.UserName);

            if (userFromDB != null)
            {
                var result = await _signInManager.PasswordSignInAsync(userFromDB,
                    loginDto.Password, 
                    isPersistent:loginDto.RemmberMe,
                    lockoutOnFailure:false);
                if (result.Succeeded)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<JwtSecurityToken> GenerateToken(LoginDto loginDto)
        {
            ApplicationUser? user = await _userManager.FindByNameAsync(loginDto.UserName);

            //generate token
            List<Claim> userClaims = new List<Claim>();

            //Token Generated id change(JWT predefind claims => like jit)
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            userClaims.Add(new Claim(ClaimTypes.NameIdentifier, user!.Id));
            userClaims.Add(new Claim(ClaimTypes.Name, user.UserName ?? ""));

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            SymmetricSecurityKey signInKey = new(Encoding.UTF8.GetBytes(config["JWT:SecritKey"] ?? string.Empty));

            SigningCredentials signingCredentials = new(signInKey, SecurityAlgorithms.HmacSha256);

            //Design token

            JwtSecurityToken token = new(
                issuer: config["JWT:IssuerURL"],
                audience: config["JWT:AudienceURL"],
                expires: DateTime.Now.AddHours(1),
                claims: userClaims,
                signingCredentials: signingCredentials
            );

            return token;
        }
    }
}
