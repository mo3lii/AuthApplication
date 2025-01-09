namespace AuthApplication.DataModels
{
    public class UserLoginResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public List<string> Roles { get; set; } = new();
    }

}
