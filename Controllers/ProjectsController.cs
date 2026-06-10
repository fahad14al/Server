using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models;
using Server.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ProjectsController(AppDbContext dbContext) {
            _dbContext = dbContext;
        }

        //create a project
        [HttpPost("create")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> CreateProject(CreateProjectDto dto)
        {
            int userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var project = new Project
            {
                ProjectName = dto.ProjectName,
                ProjectDescription = dto.ProjectDescription,
                CreatorId = userid
            };

            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();
            var member = new ProjectMember
            {
                ProjectId = project.ProjectId,
                UserId = userid,
                Role = "ProjectAdmin",
                JoinedDate = DateTime.UtcNow
            };
            await _dbContext.ProjectMembers.AddAsync(member);
            await _dbContext.SaveChangesAsync();
            return Ok(new {message= "Project Created",project.ProjectId});
        }

        //update project
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult> UpdateProjectById(int id, CreateProjectDto dto)
        {
            int userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var project = await _dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }
            if (project.CreatorId != userid && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            project.ProjectName = dto.ProjectName;
            project.ProjectDescription = dto.ProjectDescription;

            await _dbContext.SaveChangesAsync();


            return Ok(project);
        }
        //delete project
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteProjectById(int id)
        {
            int userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var project = await _dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }
            if (project.CreatorId != userid && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();


            return Ok("Project Delete");
        }
        //get project
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetProjectById(int id)
        {
            var project = await _dbContext.Projects
                .AsNoTracking()
                .Select(p => new ProjectResponseDTO
                {
                    ProjectName = p.ProjectName,
                    ProjectId = p.ProjectId,
                    CreatorName = p.Creator!.Username,
                    Description = p.ProjectDescription,
                }).FirstOrDefaultAsync(p =>p.ProjectId == id );

            if(project == null)
            {
                return NotFound();
            }


            return Ok(project);
        }

        //get all project 
        [HttpGet("all")]
        [Authorize]
        public async Task<ActionResult> GetProject(int id)
        {
            var projects = await _dbContext.Projects
                .AsNoTracking()
                .Select(p => new ProjectResponseDTO
                {
                    ProjectName = p.ProjectName,
                    ProjectId = p.ProjectId,
                    CreatorName = p.Creator!.Username,
                    Description = p.ProjectDescription,
                }).ToListAsync();

            if (projects == null)
            {
                return NotFound();
            }


            return Ok(projects);
        }

    }
}
