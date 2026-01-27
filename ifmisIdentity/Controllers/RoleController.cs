
using ifmisIdentity.Dtos;
using ifmisIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ifmisIdentity.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleController(RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles.Select(r => new
            {
                r.Id,
                r.Name
            }));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            return Ok(new
            {
                role.Id,
                role.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _roleManager.RoleExistsAsync(dto.Name))
                return Conflict(new { message = "Role already exists" });

            var role = new Role { Name = dto.Name };
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Role created successfully", role.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound(new { message = "Role not found" });

            role.Name = dto.Name;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Role updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Role deleted successfully" });
        }

        [HttpGet("{id:int}/users")]
        public async Task<IActionResult> GetUsersInRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            return Ok(usersInRole.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email
            }));
        }

        [HttpPost("{id:int}/users/{userId:int}")]
        public async Task<IActionResult> AddUserToRole(int id, int userId)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (await _userManager.IsInRoleAsync(user, role.Name))
                return Conflict(new { message = "User already assigned to this role" });

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = $"User {user.UserName} added to role {role.Name}" });
        }

        [HttpDelete("{id:int}/users/{userId:int}")]
        public async Task<IActionResult> RemoveUserFromRole(int id, int userId)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!await _userManager.IsInRoleAsync(user, role.Name))
                return BadRequest(new { message = "User is not assigned to this role" });

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = $"User {user.UserName} removed from role {role.Name}" });
        }
    }
}
