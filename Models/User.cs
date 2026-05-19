namespace Server.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string HashPassword { get; set; } = string.Empty;


        //user can create multiple projects

        public ICollection<Project> ? Projects { get; set; } 
        //user can create multiple tasks
        public ICollection<TaskItem> ? TaskItems { get; set; } 

        //projectmenber relation
        public ICollection<ProjectMember> ? ProjectMembers { get; set; }

        //user can comment on multiple tasks
        public ICollection<Comment> ? Comments { get; set; } 

        //task assignment relation
        public ICollection<TaskAssignment> ? Assignments { get; set; } 
        
        //user role relation
        public ICollection<UserRole> ? UserRoles { get; set; } 

    }
}
