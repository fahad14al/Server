namespace Server.DTOs
{
    public class UserLoginDto
    {
        public string UserEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

