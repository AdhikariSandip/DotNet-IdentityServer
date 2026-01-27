using ifmisIdentity.Data;
using ifmisIdentity.Models;
using ifmisIdentity.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace ifmisIdentity.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IdentityDbContext _dbContext;
        


        public AccountController(UserManager<User> userManager, RoleManager<Role> roleManager, IdentityDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
           
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegistrationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

           
            var existingOrg = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.DatabaseName == dto.OrganizationDatabaseName);
            if (existingOrg != null)
                return Conflict(new { message = "Organization with the provided database name already exists." });

           
            var organization = new Organization
            {
                Name = dto.OrganizationName,
                DatabaseName = dto.OrganizationDatabaseName,
                Description = dto.OrganizationDescription,
                OrgUrl = dto.OrganizationUrl
            };

            await _dbContext.Organizations.AddAsync(organization);
            await _dbContext.SaveChangesAsync();

            // Create the user
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                GlobalUserId = dto.GlobalUserId,
                OrganizationId = organization.Id
            };

            var createUserResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createUserResult.Succeeded)
                return BadRequest(createUserResult.Errors);

           
            var roles = dto.Roles ?? new List<string> { "USER" }; // Default to "USER" if roles are not provided
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role { Name = roleName };
                    await _roleManager.CreateAsync(role);
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addToRoleResult.Succeeded)
                    return BadRequest(addToRoleResult.Errors);
            }

            return Ok(new
            {
                message = "User and organization registered successfully.",
                userId = user.Id,
                organizationId = organization.Id
            });
        }


       

        //[HttpPost("logout")]
        //public async Task<IActionResult> LogoutAsync(bool logoutAll)
        //{
        //    try
        //    {
        //        // Extract token from the Authorization header
        //        string authorizationHeader = Request.Headers["Authorization"].ToString();
        //        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        //        {
        //            return Unauthorized("Invalid or missing Authorization header");
        //        }

        //        string token = authorizationHeader.Substring("Bearer ".Length).Trim();

        //        var handler = new JwtSecurityTokenHandler();

         
        //        if (!handler.CanReadToken(token))
        //        {
        //            return BadRequest("Invalid JWT token format");
        //        }

           
        //        var jwtToken = handler.ReadJwtToken(token);
            

           
        //        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        //        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        //        var tokenDelete = logoutAll
        //        ? await _openIddictDbContext.Tokens.Where(x => x.Subject == userId).ToListAsync()
        //        : await _openIddictDbContext.Tokens
        //        .Where(x => x.Subject == userId)
        //        .OrderByDescending(y => y.Id)
        //        .Take(1) 
        //        .ToListAsync();

        //            if (tokenDelete.Any())
        //            {
        //                _openIddictDbContext.Tokens.RemoveRange(tokenDelete);
        //                await _openIddictDbContext.SaveChangesAsync();
        //            }
        //            return Ok(new
        //        {
        //            Message = "Logout successful",
        //            UserId = userId,
        //            Username = username,
        //            LogoutAll = logoutAll
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error: {ex.Message}");
        //    }
        //}

        [HttpPost("change-password")]
        
        public async Task<IActionResult> ChangePasswordAsync(Guid globalUserId, string currentPassword, string newPassword)
        {
           int userId = await _userManager.Users.Where(x=>x.GlobalUserId== globalUserId).Select(x=>x.Id).FirstOrDefaultAsync();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

           
            var lastPasswordChange = await _dbContext.oldPasswords
                .Where(op => op.UserId == userId)
                .OrderByDescending(op => op.ChangedDate)
                .FirstOrDefaultAsync();

            if (lastPasswordChange != null && (DateTime.UtcNow - lastPasswordChange.ChangedDate).TotalDays > 100)
            {
                return Unauthorized(new { message = "Your password has expired. Please change it." });
            }

            // Check if the new password matches any old passwords
            var oldPasswords = await _dbContext.oldPasswords
                .Where(op => op.UserId == userId)
                
                .OrderByDescending(op => op.ChangedDate)
                .Take(3)
                .Select(op => op.PasswordHash)
                .ToListAsync();

            foreach (var oldPasswordHash in oldPasswords)
            {
                if (_userManager.PasswordHasher.VerifyHashedPassword(user, oldPasswordHash, newPassword) == PasswordVerificationResult.Success)
                {
                    return Unauthorized(new { message = "You cannot use a previously used password." });
                }
            }

            // Change the password (Requires current password)
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Password change failed", errors = result.Errors });
            }

            // Store the new password as old in the database
            var oldPassword = new OldPassword
            {
                UserId = userId,
                PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword),
                ChangedDate = DateTime.UtcNow
            };

            _dbContext.oldPasswords.Add(oldPassword);
            await _dbContext.SaveChangesAsync();

            // Return success response
            return Ok(new { message = "Password changed successfully." });
        }


    }
}
