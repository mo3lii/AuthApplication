using AuthApplication.Database;
using AuthApplication.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApplication.Authorization
{
    public class AuthTokenService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthTokenService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
            {
                _dbContext = dbContext;
                _userManager = userManager;
                _configuration = configuration;
            }

        public async Task<string> GenerateJwtToken(ApplicationUser user, List<string> userRoles)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("id",user.Id.ToString()),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public async Task SaveRefreshTokenAsync(string userId, string token)
        {
            var refreshToken = new RefreshToken()
            {
                UserId = userId,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
            };
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

        }


        public async Task<UserLoginResponse> RefreshToken(string refreshToken)
        {
            var currentRefreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken && rt.RevokedAt == null);

            if (currentRefreshToken == null || currentRefreshToken.ExpirationDate <= DateTime.UtcNow)
            {
                return new UserLoginResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token."
                };
            }

            var user = await _userManager.FindByIdAsync(currentRefreshToken.UserId.ToString());
            if (user == null)
            {
                return new UserLoginResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();

            var newAccessToken = await GenerateJwtToken(user, userRoles);
            currentRefreshToken.Revoke();
            var newRefreshToken = GenerateRefreshToken();
            await SaveRefreshTokenAsync(user.Id, newRefreshToken);

            return new UserLoginResponse
            {
                Success = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Roles = userRoles
            };
        }


        public async Task RevokeRefreshToken(RefreshToken refreshToken)
        {
            refreshToken.Revoke();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetUserActiveRefreshToken(string userId) { 
            return await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.UserId == userId && rt.RevokedAt == null);
        } 


    }

}
