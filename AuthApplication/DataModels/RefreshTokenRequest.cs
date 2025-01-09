using System.ComponentModel.DataAnnotations;

namespace AuthApplication.DataModels
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken {  get; set; }
    }
}
