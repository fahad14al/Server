using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Server.DTOs.ProjectMemberDTo;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectMemberController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ProjectMemberController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Get All Member(byID)
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetProjectMember(int id)
        {
            //userid
            var userId = GetUserId();

            var project = await _dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == id);
            if (project == null)
                return NotFound("Project not found");

            //member or admin check
            var isAuthorized = User.IsInRole("Admin") 
                || project.CreatorId == userId 
                || await _dbContext.ProjectMembers.AnyAsync(pm => pm.UserId == userId && pm.ProjectId == id);
                
            if (!isAuthorized)
                return Forbid();

            //find other member
            var members = await _dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == id)
                .Select(pm => new ProjectMemberResponseDTO
                {
                    UserId = pm.UserId,
                    UserName = pm.User!.Username,
                    Role = pm.Role,
                    JoinedOn = pm.JoinedDate
                })
                .ToListAsync();

            return Ok(new
            {
                CreatorId = project.CreatorId,
                Members = members
            });
        }

        //Add Members 
        [HttpPost("{projectId:int}/add")]
        [Authorize]
        public async Task<ActionResult> AddMember(int projectId, AddProjectMemberDTO dto)
        {
            //user id
            var userId = GetUserId();
            // creator or admin check
            var isAuthorized = User.IsInRole("Admin") || await CheckIfProjectAdmin(projectId, userId);
            if (!isAuthorized)
                return Forbid();
            var alreadyExists = await _dbContext.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == dto.UserId);
            
            if (alreadyExists)
                return BadRequest("Already Existing");
            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = dto.UserId,
                Role = dto.Role,
                JoinedDate = DateTime.UtcNow
            };
            await _dbContext.ProjectMembers.AddAsync(member);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        //private helper
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        private async Task<bool> CheckIfMember(int projectId, int userId)
        {
            return await _dbContext.ProjectMembers.AnyAsync(pm => pm.UserId == userId && pm.ProjectId == projectId)
                || await _dbContext.Projects.AnyAsync(p => p.ProjectId == projectId && p.CreatorId == userId);
        }
        private async Task<bool> CheckIfProjectAdmin(int projectId, int userId)
        {
            return await _dbContext.Projects.AnyAsync(p => p.ProjectId == projectId && p.CreatorId == userId)
                || await _dbContext.ProjectMembers.AnyAsync(
                    pm =>
                    pm.UserId == userId &&
                    pm.ProjectId == projectId &&
                    pm.Role == "ProjectAdmin"
                    );
        }


    }
}
