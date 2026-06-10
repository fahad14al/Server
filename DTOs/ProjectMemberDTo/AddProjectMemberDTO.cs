using Server.Models;

namespace Server.DTOs.ProjectMemberDTo
{
    public class AddProjectMemberDTO
    {

        public int UserId { get; set; }

        public string Role { get; set; } = "Member";//Dev/Admin/owner
    }
}
