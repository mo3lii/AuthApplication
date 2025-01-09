using AuthApplication.Authorization;
using AuthApplication.DataModels;
using AuthApplication.DataModels;
using AuthApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthTokenService _authTokenService;
        public UserController(UserService userService,AuthTokenService authTokenService)
        {
            _userService = userService;
            _authTokenService = authTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest userRegisterDto)
        {
            var result = await _userService.RegisterUser(userRegisterDto,AuthRoles.User);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("registerAdmin")]
        [Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin}")]
        public async Task<IActionResult> RegisterAdmin(UserRegisterRequest userRegisterDto)
        {
            var result = await _userService.RegisterUser(userRegisterDto, AuthRoles.Admin);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("registerSuperAmin")]
        [Authorize(Roles = AuthRoles.SuperAdmin)]
        public async Task<IActionResult> RegisterSuperAdmin(UserRegisterRequest request)
        {
            var result = await _userService.RegisterUser(request, AuthRoles.SuperAdmin);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var result = await _userService.UserLogin(request);
            if (!result.Success)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        [HttpPut("logout")]
        [Authorize()]
        public async Task<IActionResult> Logout()
        {
            var result = await _userService.UserLogout();
            if (!result.Success)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authTokenService.RefreshToken(request.RefreshToken);
            if (!result.Success)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }
        
    }
}
