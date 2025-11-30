using ColorMate.Core.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ColorMate.BL.UserService
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);

        Task<bool> CheckLoginAsync(LoginDto loginDto);

        Task<JwtSecurityToken> GenerateToken(LoginDto loginDto);
    }
}
