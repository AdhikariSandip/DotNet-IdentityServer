using ifmisIdentity.Dtos;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;


namespace ifmisIdentity.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        public ClientController(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = new List<object>();

            await foreach (var app in _applicationManager.ListAsync())
            {
                clients.Add(new
                {
                    ClientId = await _applicationManager.GetIdAsync(app),
                    DisplayName = await _applicationManager.GetDisplayNameAsync(app),
                    Permissions = await _applicationManager.GetPermissionsAsync(app),
                    RedirectUris = (await _applicationManager.GetRedirectUrisAsync(app)).Select(uri => uri.ToString()),
                    PostLogoutRedirectUris = (await _applicationManager.GetPostLogoutRedirectUrisAsync(app)).Select(uri => uri.ToString())
                });
            }

            return Ok(clients);
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetClientById(string clientId)
        {
            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null)
                return NotFound(new { message = "Client not found." });

            return Ok(new
            {
                ClientId = await _applicationManager.GetIdAsync(application),
                DisplayName = await _applicationManager.GetDisplayNameAsync(application),
                Permissions = await _applicationManager.GetPermissionsAsync(application),
                RedirectUris = (await _applicationManager.GetRedirectUrisAsync(application)).Select(uri => uri.ToString()),
                PostLogoutRedirectUris = (await _applicationManager.GetPostLogoutRedirectUrisAsync(application)).Select(uri => uri.ToString())
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingClient = await _applicationManager.FindByClientIdAsync(dto.ClientId);
            if (existingClient != null)
                return Conflict(new { message = "Client with the given ID already exists." });

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                DisplayName = dto.DisplayName
            };

            descriptor.Permissions.UnionWith(dto.Permissions);
            descriptor.RedirectUris.UnionWith(dto.RedirectUris.Select(uri => new Uri(uri)));
            descriptor.PostLogoutRedirectUris.UnionWith(dto.PostLogoutRedirectUris.Select(uri => new Uri(uri)));

            await _applicationManager.CreateAsync(descriptor);

            return Ok(new { message = "Client created successfully.", clientId = dto.ClientId });
        }

        [HttpPut("{clientId}")]
        public async Task<IActionResult> UpdateClient(string clientId, [FromBody] ClientDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null)
                return NotFound(new { message = "Client not found." });

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                DisplayName = dto.DisplayName
            };

            descriptor.Permissions.UnionWith(dto.Permissions);
            descriptor.RedirectUris.UnionWith(dto.RedirectUris.Select(uri => new Uri(uri)));
            descriptor.PostLogoutRedirectUris.UnionWith(dto.PostLogoutRedirectUris.Select(uri => new Uri(uri)));

            await _applicationManager.UpdateAsync(application, descriptor);

            return Ok(new { message = "Client updated successfully.", clientId = dto.ClientId });
        }

        [HttpDelete("{clientId}")]
        public async Task<IActionResult> DeleteClient(string clientId)
        {
            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null)
                return NotFound(new { message = "Client not found." });

            await _applicationManager.DeleteAsync(application);

            return Ok(new { message = "Client deleted successfully." });
        }
    }
}
