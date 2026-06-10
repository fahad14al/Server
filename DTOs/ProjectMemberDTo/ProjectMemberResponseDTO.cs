namespace Server.DTOs.ProjectMemberDTo
{
    public class ProjectMemberResponseDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public string Role { get; set; } = string.Empty;//Dev/Admin/owner

        public DateTime JoinedOn { get; set; }
    }
}
