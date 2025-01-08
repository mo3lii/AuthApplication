using AuthApplication.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok("You are authorized user");
        }

        [HttpGet("auth-user")]
        [Authorize(Roles = Roles.User)]
        public IActionResult TestAuthUser()
        {
            return Ok("You are authorized user with role = user");
        }

        [HttpGet("auth-admin")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult TestAuthAdmin()
        {
            return Ok("You are authorized user with role = admin");
        }

        [HttpGet("auth-super")]
        [Authorize(Roles = Roles.SuperAdmin)]
        public IActionResult TestAuthSuper()
        {
            return Ok("You are authorized user with role = super admin ");
        }

        [HttpGet("error")]
        public IActionResult error()
        {
            throw new Exception("thrown exception in controller");
            return Ok("passed the error");
        }

        [HttpGet("error-auth")]
        public IActionResult errorAuth()
        {
            throw new UnauthorizedAccessException("thrown unauthorized exception");
            return Ok("passed the error");
        }
    }
}
