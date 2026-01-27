using ifmisIdentity.Dtos;
using ifmisIdentity.Models;
using ifmisIdentity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifmisIdentity.Configuration;


namespace ifmisIdentity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IdentityDbContext _dbContext;
        private readonly EmailService _emailService;

        public UserController(UserManager<User> userManager, RoleManager<Role> roleManager, IdentityDbContext dbContext, EmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContext.Users
                .Include(u => u.Organization)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.IsActive,
                    u.CreatedAt,
                    Organization = u.Organization.Name,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContext.Users
                .Include(u => u.Organization)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.IsActive,
                    u.CreatedAt,
                    Organization = u.Organization.Name,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _dbContext.Users
                .Include(u => u.Organization)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserName == username)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.IsActive,
                    u.CreatedAt,
                    Organization = u.Organization.Name,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == dto.OrganizationID);
            if (organization == null)
                return NotFound(new { message = "Organization not found." });

            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                IsActive = true, 
                CreatedAt = DateTime.UtcNow,
                OrganizationId = dto.OrganizationID,
                GlobalUserId = dto.GlobalUserId
            };

            var createUserResult = await _userManager.CreateAsync(user, dto.PasswordHash);
            if (!createUserResult.Succeeded)
                return BadRequest(createUserResult.Errors);

            // Assign roles
            foreach (var roleName in dto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new Role { Name = roleName });

                await _userManager.AddToRoleAsync(user, roleName);
            }

          
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/user/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

           
            await _emailService.SendEmailAsync(user.Email, "Confirm Your Email",
                $"Click <a href='{confirmationLink}'>here</a> to activate your account.");

            return Ok(new { message = "User created successfully. Please check your email to activate your account.", userId = user.Id });
        }

        
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Invalid email confirmation request. Please provide a valid user ID and token." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found. The confirmation link might have expired or be incorrect." });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new { message = "Email confirmation failed. The token might be invalid or expired.", errors = result.Errors });

           
            user.IsActive = true;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return BadRequest(new { message = "Failed to activate the user account after email confirmation.", errors = updateResult.Errors });

            return Ok(new { message = "Your email has been successfully confirmed. Your account is now activated, and you can log in." });
        }



        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUserAsync(Guid globalUserId, [FromBody] UpdateUserDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            // Fetch user by GlobalUserId
            var user = await _userManager.Users
                .Where(x => x.GlobalUserId == globalUserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Handle Username Change
            if (!string.IsNullOrWhiteSpace(dto.NewUsername))
            {
                var usernameExists = await _userManager.Users.AnyAsync(x => x.UserName == dto.NewUsername);
                if (usernameExists)
                {
                    return BadRequest(new { message = "Username is already taken." });
                }

                user.UserName = dto.NewUsername;
            }

            // Handle Password Change
            if (!string.IsNullOrWhiteSpace(dto.CurrentPassword) && !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var lastPasswordChange = await _dbContext.oldPasswords
                    .Where(op => op.UserId == user.Id)
                    .OrderByDescending(op => op.ChangedDate)
                    .FirstOrDefaultAsync();

                if (lastPasswordChange != null && (DateTime.UtcNow - lastPasswordChange.ChangedDate).TotalDays > 100)
                {
                    return Unauthorized(new { message = "Your password has expired. Please change it." });
                }

                // Check if the new password matches old passwords
                var oldPasswords = await _dbContext.oldPasswords
                    .Where(op => op.UserId == user.Id)
                    .OrderByDescending(op => op.ChangedDate)
                    .Take(3)
                    .Select(op => op.PasswordHash)
                    .ToListAsync();

                foreach (var oldPasswordHash in oldPasswords)
                {
                    if (_userManager.PasswordHasher.VerifyHashedPassword(user, oldPasswordHash, dto.NewPassword) == PasswordVerificationResult.Success)
                    {
                        return Unauthorized(new { message = "You cannot use a previously used password." });
                    }
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Password change failed", errors = result.Errors });
                }

                // Store the new password in oldPasswords table
                var oldPassword = new OldPassword
                {
                    UserId = user.Id,
                    PasswordHash = _userManager.PasswordHasher.HashPassword(user, dto.NewPassword),
                    ChangedDate = DateTime.UtcNow
                };

                _dbContext.oldPasswords.Add(oldPassword);
                await _dbContext.SaveChangesAsync();
            }

            // Save changes if username was updated
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to update user.", errors = updateResult.Errors });
            }

            return Ok(new { message = "User updated successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://app.emis.com.np/reset-password?itoken={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

            // Send email (implement EmailService)
            await _emailService.SendEmailAsync(user.Email, "Password Reset", $"Click <a href='{resetLink}'>here</a> to reset your password.");

            return Ok(new { message = "Password reset link has been sent to your email." });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDTO model)
        {
            // Find user by GlobalUserId
            int userId = await _userManager.Users
                .Where(x => x.Email == model.Email)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Ensure token is valid
            var isTokenValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", model.Token);
            if (!isTokenValid)
            {
                return BadRequest(new { message = "Invalid or expired reset token." });
            }

            // Check password expiry policy
            var lastPasswordChange = await _dbContext.oldPasswords
                .Where(op => op.UserId == userId)
                .OrderByDescending(op => op.ChangedDate)
                .FirstOrDefaultAsync();

            if (lastPasswordChange != null && (DateTime.UtcNow - lastPasswordChange.ChangedDate).TotalDays > 100)
            {
                return Unauthorized(new { message = "Your password has expired. Please change it." });
            }

            // Check if the new password was previously used (last 3 passwords)
            var oldPasswords = await _dbContext.oldPasswords
                .Where(op => op.UserId == userId)
                .OrderByDescending(op => op.ChangedDate)
                .Take(3)
                .Select(op => op.PasswordHash)
                .ToListAsync();

            foreach (var oldPasswordHash in oldPasswords)
            {
                if (_userManager.PasswordHasher.VerifyHashedPassword(user, oldPasswordHash, model.NewPassword) == PasswordVerificationResult.Success)
                {
                    return Unauthorized(new { message = "You cannot use a previously used password." });
                }
            }

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Password reset failed.", errors = result.Errors });
            }

            // Store the new password in oldPasswords table
            var oldPassword = new OldPassword
            {
                UserId = userId,
                PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword),
                ChangedDate = DateTime.UtcNow
            };

            _dbContext.oldPasswords.Add(oldPassword);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }



        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(new { message = "User not found." });

            await _userManager.DeleteAsync(user);

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
