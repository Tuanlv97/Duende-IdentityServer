using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using TeduMicroservices.IDP.Infrastructure.Repositories;
using TeduMicroservices.IDP.Infrastructure.ViewModels;

namespace TeduMicroservices.IDP.Persentation.Controllers;

[ApiController]
[Route("/api/[controller]/roles/{roleId}")]
//[Authorize("Bearer")]
public class PermissionsController : ControllerBase
{
    private readonly IRepositoryManager _repositoryManager;
    public PermissionsController(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
    }
    [HttpGet]
    public async Task<IActionResult> GetPermissions(string roleId)
    {
        var result = await _repositoryManager.Permission.GetPermissionsByRole(roleId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PermissionViewModel), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreatePermission(string roleId, [FromBody] PermissionAddModel model)
    {
        var result = await _repositoryManager.Permission.CreatePermission(roleId, model);
        return result != null ? Ok(result) : NoContent();
    }

    [HttpDelete("function/{function}/command/{command}")]
    [ProducesResponseType(typeof(PermissionViewModel), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeletePermission(string roleId, [Required] string function, [Required] string commnad)
    {
        await _repositoryManager.Permission.DeletePermission(roleId, function, commnad);
        return  NoContent();
    }

    [HttpPost("update-permissions")]
    [ProducesResponseType(typeof(NoContentResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdatePermission(string roleId, [FromBody]IEnumerable<PermissionAddModel> permissions)
    {
         await _repositoryManager.Permission.UpdatePermissionsByRoleId(roleId, permissions);
        return  NoContent();
    }

}
