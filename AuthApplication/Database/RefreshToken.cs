namespace AuthApplication.Database
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }

        public DateTime ExpirationDate { get; set; }
        public DateTime? RevokedAt { get; set; }

        public void Revoke(){
            if(Token == null)
            {
                throw new Exception("token not found");  
            }
            if(RevokedAt != null)
            {
                throw new Exception("token is already revoked");
            }
            RevokedAt = DateTime.UtcNow;
        }

    }
}
