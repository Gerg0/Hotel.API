using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.API.Core.Dtos.User;
using Microsoft.AspNetCore.Identity;

namespace Hotel.API.Core.Contracts
{
    public interface IAuthManager
    {
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
        Task<string> CreateRefreshToken();
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
    }
}