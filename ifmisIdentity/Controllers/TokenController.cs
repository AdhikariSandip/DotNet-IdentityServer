using ifmisIdentity.Models;
using ifmisIdentity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore;


namespace ifmisIdentity.Controllers
{
    [Route("connect/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IdentityDbContext _dbContext;
        

        public TokenController(UserManager<User> userManager, IdentityDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Exchange()
        {
            try
            {
                var request = HttpContext.GetOpenIddictServerRequest();
                if (request == null)
                    return BadRequest("Invalid request format.");

                if (request.IsPasswordGrantType())
                    return await HandlePasswordGrant(request);

                return BadRequest("Unsupported grant type.");
            }
            catch (Exception ex)
            {
                // This will show the actual error in Postman
                return StatusCode(500, new
                {
                    error = "Internal Server Error",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        private async Task<IActionResult> HandlePasswordGrant(OpenIddictRequest request)
        {

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User account is inactive." });
            }

   
            var lastPasswordChange = await _dbContext.oldPasswords
                .Where(op => op.UserId == user.Id)
                .OrderByDescending(op => op.ChangedDate)
                .FirstOrDefaultAsync();

            if (lastPasswordChange != null && (DateTime.UtcNow - lastPasswordChange.ChangedDate).TotalDays > 100)
            {
                return Unauthorized(new { message = "Your password has expired. Please change your password." });
            }

            var oldPasswords = await _dbContext.oldPasswords
                .OrderByDescending(x=>x.Id)
                .Where(op => op.UserId == user.Id)
                .Select(op => op.PasswordHash)
                .Skip(1)
                .Take(3)
                .ToListAsync();

            foreach (var oldPasswordHash in oldPasswords)
            {
                if (_userManager.PasswordHasher.VerifyHashedPassword(user, oldPasswordHash, request.Password) == PasswordVerificationResult.Success)
                {
                    return Unauthorized(new { message = "You cannot use a previously used password." });
                }
            }



            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == user.OrganizationId);

          
            var claims = new List<Claim>
            {
                        new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
                        new Claim(OpenIddictConstants.Claims.Username, user.UserName),
                        new Claim(OpenIddictConstants.Claims.Email, user.Email ?? string.Empty),

                        new Claim("GlobalUserId", user.GlobalUserId.ToString())
            };

            if (organization != null)
            {
                claims.Add(new Claim("M_1", organization.DatabaseName));
                claims.Add(new Claim("OrgUrl", organization.OrgUrl));
                claims.Add(new Claim("OrgName", organization.Name));
                claims.Add(new Claim("OrgId", organization.Id.ToString()));

            }
           
            var audiences = new[] {
                
              "https://financetunew.emis.com.np",
              "https://apitu.emis.com.np",
              "https://localhost:5080",
              "http://localhost:4500"

            };

            foreach (var audience in audiences)
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.Audience, audience));
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));

            }

            
            var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            
            principal.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Roles
            });

           
            foreach (var claim in principal.Claims)
            {
               // Console.WriteLine($"Claim: {claim.Type} = {claim.Value}"); 
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
       
       

    }
}
