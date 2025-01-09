using AuthApplication.Authorization;
using AuthApplication.Database;
using AuthApplication.DataModels;
using Microsoft.AspNetCore.Identity;


namespace AuthApplication.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserService> _logger;
        private readonly HttpContextAccessor _httpContextAccessor;
        private readonly AuthTokenService _authTokenService;

        public UserService(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserService> logger,
            AuthTokenService authTokenService,
            HttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _authTokenService = authTokenService;
        }

        public async Task<RegisterationResponse> RegisterUser(UserRegisterRequest request, string? userRole)
        {
            if (userRole == null)
            {
                userRole = AuthRoles.User;
            }

            var user = new ApplicationUser()
            {
                UserName = request.UserName,
                Email = request.Email,
            };

            var registerationResult = await _userManager.CreateAsync(user,request.Password);

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

        public async Task<UserLoginResponse> UserLogin(UserLoginRequest request)
        {

            var user = await _userManager.FindByNameAsync(request.UserName);

            var response = new UserLoginResponse();

            if (user == null)
            {
                response.Success = false;
                response.Message = "Invalid username or password.";
                _logger.LogWarning($"Failed login attempt for user: {request.UserName} at {DateTime.UtcNow}.");
                return response;
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, false);
            if (!signInResult.Succeeded)
            {
                response.Success = false;
                response.Message = "Invalid username or password."; 
                _logger.LogWarning($"Failed login attempt for user: {request.UserName} at {DateTime.UtcNow}.");
                return response;
            }

            var userRefreshToken = await _authTokenService.GetUserActiveRefreshToken(user.Id);
            if (userRefreshToken != null)
            {
                userRefreshToken.Revoke();
            }
            

            var refreshToken = _authTokenService.GenerateRefreshToken();
            await _authTokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();

            response.Token = await _authTokenService.GenerateJwtToken(user, userRoles); 
            response.RefreshToken = refreshToken;
            response.Success = true;
            response.Roles = userRoles;

            return response;
        }

        public async Task<UserLogOutReponse> UserLogout()
        {
            var currentUserId = GetUserIdFromClaims();
     
            var user = await _userManager.FindByIdAsync(currentUserId);

            var response = new UserLogOutReponse();
            if (user == null)
            {
                response.Success = false;
                response.Message = "user not found";
                return response;
            }

            var userRefreshToken = await _authTokenService.GetUserActiveRefreshToken(user.Id);
            if(userRefreshToken == null)
            {
                throw new Exception("No Tokens found for this user");
            }
            await _authTokenService.RevokeRefreshToken(userRefreshToken);
          
            response.Success = true;
            return response;
        }

        public async Task<IdentityResult> AddUserToRole(ApplicationUser user, string userRole)
        {
            var roleExist = await _roleManager.RoleExistsAsync(userRole);

            if (!roleExist)
            {
                // Create the role if it does not exist
                var role = new IdentityRole(userRole);
                var createRoleResult =  await _roleManager.CreateAsync(role);
                if (!createRoleResult.Succeeded)
                    throw new Exception("Role is not exist");
            }

            return await _userManager.AddToRoleAsync(user, userRole);

        }

        public string GetUserIdFromClaims()
        {
            var userClaims = _httpContextAccessor.HttpContext.User;

            if (userClaims.Identity.IsAuthenticated)
            {
                var userId = userClaims.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User ID claim not found.");
                }

                return userId;
            }

            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        
    }

}

