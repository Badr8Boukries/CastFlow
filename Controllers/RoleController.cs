using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CastFlow.Api.Controllers
{
    [Route("api")] 
    [ApiController]
    [Authorize] 
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("projets/{projetId}/roles")]
        [ProducesResponseType(typeof(RoleDetailResponseDto), StatusCodes.Status201Created)]
        // ... autres attributs ...
        public async Task<IActionResult> CreateRoleForProject(long projetId, [FromBody] RoleCreateRequestDto createDto) 
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var role = await _roleService.CreateRoleAsync(projetId, createDto);

                if (role == null) return NotFound($"Projet ID {projetId} non trouvé ou inaccessible.");

                return CreatedAtAction(nameof(GetRoleById), new { roleId = role.RoleId }, role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création rôle pour projet {ProjetId}", projetId);
                return StatusCode(500, "Erreur interne serveur.");
            }
        }


        [HttpGet("roles/{roleId}")]
        public async Task<IActionResult> GetRoleById(long roleId) 
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try
            {
                var role = await _roleService.GetRoleByIdAsync(roleId); 
                if (role == null) return NotFound($"Rôle ID {roleId} non trouvé.");
                return Ok(role);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération rôle {RoleId}", roleId); return StatusCode(500, "Erreur interne serveur."); }
        }
        // GET api/projets/{projetId}/roles
        [HttpGet("projets/{projetId}/roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleSummaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRolesForProjet(long projetId)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try
            {
                var roles = await _roleService.GetRolesForProjetAsync(projetId);
                return Ok(roles);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération rôles pour projet {ProjetId}", projetId); return StatusCode(500, "Erreur interne serveur."); }
        }


        // PUT api/roles/{roleId}
        [HttpPut("roles/{roleId}")]
        [ProducesResponseType(typeof(RoleDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(long roleId, [FromBody] RoleUpdateRequestDto updateDto)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updatedRole = await _roleService.UpdateRoleAsync(roleId, updateDto);
                if (updatedRole == null) return NotFound($"Rôle ID {roleId} non trouvé.");
                return Ok(updatedRole);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur mise à jour rôle {RoleId}", roleId); return StatusCode(500, "Erreur interne serveur."); }
        }

        // DELETE api/roles/{roleId}
        [HttpDelete("roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(long roleId)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try
            {
                var success = await _roleService.DeleteRoleAsync(roleId);
                if (!success) return NotFound($"Rôle ID {roleId} non trouvé.");
                return NoContent();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur suppression rôle {RoleId}", roleId); return StatusCode(500, "Erreur interne serveur."); }
        }


        private bool IsAdmin() { var userTypeClaim = User.FindFirstValue("userType"); return "Admin".Equals(userTypeClaim, StringComparison.OrdinalIgnoreCase); }
    }
}