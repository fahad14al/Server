using Server.Models;

namespace Server.DTOs
{
    public class CreateProjectDto
    {

        
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;

        
    }
}
