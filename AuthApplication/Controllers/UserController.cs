using AuthApplication.DTOs;
using AuthApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var result = await _userService.RegisterUser(userRegisterDto,Roles.User);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdmin(UserRegisterDto userRegisterDto)
        {
            var result = await _userService.RegisterUser(userRegisterDto,Roles.Admin);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("registerSuperAmin")]
        public async Task<IActionResult> RegisterSuperAdmin(UserRegisterDto userRegisterDto)
        {
            var result = await _userService.RegisterUser(userRegisterDto, Roles.SuperAdmin);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var result = await _userService.UserLogin(userLoginDto);
            if (!result.Success)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        
    }
}
