namespace Api.Controllers
{
    using Api.Policies;
    using Application.Features.Role.Commands;
    using Application.Features.Role.Dtos;
    using Application.Features.Role.Queries;
    using Core.Dtos.ResponsesDto;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/V1/[controller]")]
    public class RoleController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Get All Roles.
        /// </summary>
        /// <returns>IEnumerable&lt;Role&gt;</returns>
        [HttpGet("GetAllRoles")]
        [Consumes("application/json")]
        [Authorize(Policy = "Administrador")]//Keycloak
        public Task<Result<IEnumerable<RoleDto>>> GetAllRolesAsync() => _mediator.Send(new GetAllRoleQuery());

        /// <summary>
        /// Create role.
        /// </summary>
        /// <param name="createRoleCommand">The parameters.</param>
        /// <returns>Response.</returns>
        [HttpPost("CreateRole")]
        [Consumes("application/json")]
        [Authorize(Policy = nameof(AdministratorPolicy))]//Local
        public async Task<Result> CreateRoleAsync([FromBody] CreateRoleCommand createRoleCommand) => await _mediator.Send(createRoleCommand);
    }
}