using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Hotel.API.Core.Contracts;
using Hotel.API.Core.Dtos.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hotel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion(1.0)]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
        {
            _logger = logger;
            _authManager = authManager;
        }

        // POST: api/Account/register
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] ApiUserDto apiUserDto)
        {
            _logger.LogInformation($"Registration attempt for {apiUserDto.Email}");
            try
            {
                var errors = await _authManager.Register(apiUserDto);
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(Register)} - User Registration attempt for {apiUserDto.Email}");
                return Problem($"Something went wrong in the {nameof(Register)}", statusCode: 500);
            }

        }

        // POST: api/Account/login
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto apiUserDto)
        {
            _logger.LogInformation($"Login attempt for {apiUserDto.Email}");
            try
            {
                var authResponse = await _authManager.Login(apiUserDto);
                if (authResponse == null)
                {
                    return Unauthorized();
                }
                return Ok(authResponse);
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(Login)}");
                return Problem($"Something went wrong in the {nameof(Login)}", statusCode: 500);
            }
            
        }

        // POST: api/Account/refreshtoken
        [HttpPost]
        [Route("refreshtoken")]
        public async Task<ActionResult> RefreshToken([FromBody] AuthResponseDto request)
        {
            try
            {
                var authResponse = await _authManager.VerifyRefreshToken(request);
                if (authResponse == null)
                {
                    return Unauthorized();
                }
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(RefreshToken)}");
                return Problem($"Something went wrong in the {nameof(RefreshToken)}", statusCode: 500);
            }
            
        }
    }
}