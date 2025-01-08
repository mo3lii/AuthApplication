using AuthApplication.Database;
using AuthApplication.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApplication.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;

        }

        public async Task<RegisterationResponse> RegisterUser(UserRegisterDto userRegisterDto, string? userRole)
        {
            if (userRole == null)
            {
                userRole = Roles.User;
            }

            var user = new ApplicationUser()
            {
                UserName = userRegisterDto.UserName,
                Email = userRegisterDto.Email,
            };

            var registerationResult = await _userManager.CreateAsync(user,userRegisterDto.Password);

            var response = new RegisterationResponse();
            if (registerationResult.Succeeded)
            {
                var roleResult = await AddUserToRole(user,userRole.ToString());
                if (roleResult.Succeeded)
                {
                    response.Success = true;
                    response.Message = "user registered successfully";
                    return response;
                }
            }
            
            response.Success = false;
            response.Message = "User registration failed";
            response.Errors = registerationResult.Errors.Select(e=>e.Description).ToList();
            return response;
        }

        public async Task<UserLoginResponse> UserLogin(UserLoginDto userLoginDto)
        {

            var user = await _userManager.FindByNameAsync(userLoginDto.UserName);

            var response = new UserLoginResponse();

            if (user == null)
            {
                response.Success = false;
                response.Message = "Invalid email or password.";
                _logger.LogWarning($"Failed login attempt for user: {userLoginDto.UserName} at {DateTime.UtcNow}.");
                return response;
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, userLoginDto.Password, false, false);
            if (!signInResult.Succeeded)
            {
                response.Success = false;
                response.Message = "Invalid username or password."; 
                _logger.LogWarning($"Failed login attempt for user: {userLoginDto.UserName} at {DateTime.UtcNow}.");
                return response;
            }

            response.Success = true;
            response.Token = await GenerateJwtToken(user);
            response.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            
            return response;
        }
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("id",user.Id.ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach(var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token) ;
        }

        public async Task<IdentityResult> AddUserToRole(ApplicationUser user, string userRole)
        {
            var roleExist = await _roleManager.RoleExistsAsync(userRole);

            if (!roleExist)
            {
                // Create the role if it does not exist
                //TODO : seed it to Db
                var role = new IdentityRole(userRole);
                var createRoleResult =  await _roleManager.CreateAsync(role);
                if (!createRoleResult.Succeeded)
                    throw new Exception("Technichal error occurred");
            }

            return await _userManager.AddToRoleAsync(user, userRole);

        }
    }
}
