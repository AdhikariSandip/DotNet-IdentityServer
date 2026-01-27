using ifmisIdentity.Data;
using ifmisIdentity.Dtos;
using ifmisIdentity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ifmisIdentity.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IdentityDbContext _dbContext;

        public OrganizationController(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrganizations()
        {
            var organizations = await _dbContext.Organizations.ToListAsync();
            return Ok(organizations.Select(o => new
            {
                o.Id,
                o.Name,
                o.DatabaseName,
                o.Description,
                o.OrgUrl
            }));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrganizationById(int id)
        {
            var organization = await _dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == id);
            if (organization == null)
                return NotFound(new { message = "Organization not found" });

            return Ok(new
            {
                organization.Id,
                organization.Name,
                organization.DatabaseName,
                organization.Description,
                organization.OrgUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] OrganizationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingOrg = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.DatabaseName == dto.DatabaseName);
            if (existingOrg != null)
                return Conflict(new { message = "An organization with this database name already exists" });

            var organization = new Organization
            {
                Name = dto.Name,
                DatabaseName = dto.DatabaseName,
                Description = dto.Description,
                OrgUrl = dto.OrgUrl
            };

            await _dbContext.Organizations.AddAsync(organization);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Organization created successfully", organization.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrganization(int id, [FromBody] OrganizationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == id);
            if (organization == null)
                return NotFound(new { message = "Organization not found" });

            organization.Name = dto.Name;
            organization.DatabaseName = dto.DatabaseName;
            organization.Description = dto.Description;
            organization.OrgUrl = dto.OrgUrl;

            _dbContext.Organizations.Update(organization);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Organization updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var organization = await _dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == id);
            if (organization == null)
                return NotFound(new { message = "Organization not found" });

            _dbContext.Organizations.Remove(organization);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Organization deleted successfully" });
        }

        [HttpGet("{id:int}/users")]
        public async Task<IActionResult> GetUsersInOrganization(int id)
        {
            var organization = await _dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == id);
            if (organization == null)
                return NotFound(new { message = "Organization not found" });

            var users = await _dbContext.Users
                .Where(u => u.OrganizationId == id)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.CreatedAt
            }));
        }
    }
}
